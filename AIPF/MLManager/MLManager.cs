using AIPF.MLManager.Actions;
using AIPF.MLManager.Metrics;
using AIPF.Utilities;
using Microsoft.ML;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AIPF.MLManager
{
    public class MLManager<I, O> where I : class, new() where O : class, new()
    {
        public static readonly ActivitySource source = new ActivitySource(nameof(MLManager));

        private readonly MLContext mlContext;

        private MLBuilder<I, O> mlBuilder;
        public bool Trained { get; private set; } = false;
        public string Name { get; private set; }

        public MLLoader<I> Loader { get; private set; }

        public MLManager(string name = "Default model")
        {
            Name = name;
            mlContext = new MLContext();
            //mlContext.Log += new EventHandler<LoggingEventArgs>(Log);
            Loader = new MLLoader<I>(this.mlContext);
        }

        private void Log(object sender, LoggingEventArgs e)
        {
            //if (e.Source.Contains("SdcaTrainerBase"))
            //ConsoleHelper.WriteLine(sender.GetType() + " " + e.Message);
        }

        public MLBuilder<I, O> CreatePipeline()
        {
            mlBuilder = new MLBuilder<I, O>(mlContext);
            return mlBuilder;
        }

        public async Task<IDataView> Fit(IEnumerable<I> rawData)
        {
            return await Fit(mlContext.Data.LoadFromEnumerable(rawData));
        }

        public async Task<IDataView> Fit(IDataView rawData)
        {
            if (mlBuilder == null)
                throw new Exception("The pipeline must be valid");

            return await Task.Run(() =>
            {
                using var activity = source.StartActivity("Fit");
                activity?.AddTag("model_name", Name);
                activity?.AddTag("type", typeof(MLManager<I, O>).ToGenericTypeString());
                activity?.AddTag("processed_elements", MLUtils.GetDataViewLength<I>(mlContext, rawData));
                activity?.AddTag("input.type", typeof(I).ToGenericTypeString());
                activity?.AddTag("output.type", typeof(O).ToGenericTypeString());

                try
                {
                    mlBuilder.Fit(rawData, out IDataView transformedDataView);
                    Trained = true;
                    activity?.SetStatus(Status.Ok.WithDescription("All fine"));
                    return transformedDataView;
                }
                catch (Exception ex)
                {
                    activity?.RecordException(ex);
                    activity?.SetStatus(Status.Error.WithDescription(ex.Message));
                    throw ex;
                }
                finally
                {
                    activity?.AddEvent(new ActivityEvent("End fit!", DateTimeOffset.UtcNow));
                }
            });
        }

        public async Task<O> Predict(I toPredict) 
        {
            if (mlBuilder == null)
                throw new Exception("The pipeline must be valid");
            if (!Trained)
                throw new Exception("You must call Fit() before Predict()!");
            if (toPredict == null)
                throw new Exception("You must pass a valid item!");

            return await Task.Run(() =>
            {
                using var activity = source.StartActivity("Predict");
                activity?.AddTag("model_name", Name);
                activity?.AddTag("type", typeof(MLManager<I, O>).ToGenericTypeString());
                activity?.AddTag("input.type", typeof(I).ToGenericTypeString());
                activity?.AddTag("output.type", typeof(O).ToGenericTypeString());

                Array.ForEach(typeof(I).GetProperties(), p => activity?.AddTag($"input.{p.Name}", p.GetValue(toPredict)));

                try
                {
                    var prediction = mlBuilder.Predict(toPredict);
                    activity?.SetStatus(Status.Ok.WithDescription("All fine")); 
                    Array.ForEach(typeof(O).GetProperties(), p => activity?.AddTag($"output.{p.Name}", p.GetValue(prediction)));
                    return prediction;
                }
                catch (Exception ex)
                {
                    activity?.RecordException(ex);
                    activity?.SetStatus(Status.Error.WithDescription(ex.Message));
                    throw ex;
                }
                finally
                {
                    activity?.AddEvent(new ActivityEvent("End Prediction!", DateTimeOffset.UtcNow));
                }

            });
        }

        public async Task<List<MetricContainer>> EvaluateAll(IEnumerable<I> data)
        {
            return await Task.Run(() => EvaluateAll(mlContext.Data.LoadFromEnumerable(data)));
        }

        public async Task<List<MetricContainer>> EvaluateAll(IDataView dataView)
        {
            if (mlBuilder == null)
                throw new Exception("The pipeline must be valid");
            if (dataView == null)
                throw new Exception("You need to pass a data set to test the accurancy of the model!");
            if (!Trained)
                throw new Exception("You must call Fit() before EvaluateAll()!");

            return await Task.Run(() =>
            {
                using var activity = source.StartActivity("EvaluateAll");
                activity?.AddTag("model_name", Name);
                activity?.AddTag("type", typeof(MLManager<I, O>).ToGenericTypeString());
                activity?.AddTag("processed_elements", MLUtils.GetDataViewLength<I>(mlContext, dataView));
                activity?.AddTag("input.type", typeof(I).ToGenericTypeString());
                activity?.AddTag("output.type", typeof(O).ToGenericTypeString());

                try
                {
                    var metrics = mlBuilder.EvaluateAll(dataView);
                    activity?.SetStatus(Status.Ok.WithDescription("All fine"));
                    return setMetrics(activity, metrics);
                }
                catch (Exception ex)
                {
                    activity?.RecordException(ex);
                    activity?.SetStatus(Status.Error.WithDescription(ex.Message));
                    throw ex;
                }
                finally
                {
                    activity?.AddEvent(new ActivityEvent("End Evaluate!", DateTimeOffset.UtcNow));
                }
            });
        }

        private List<MetricContainer> setMetrics(Activity activity, List<MetricContainer> metrics)
        {
            //using var activity = source.StartActivity("EvaluateAll");
            //activity?.AddTag("model_name", Name);
            foreach (var m in metrics)
            {
                foreach (var mm in m.Metrics)
                {
                    //m.Name + mm.Name + ", " + mm.Value;
                    activity?.SetTag($"metric.{m.Name}.{mm.Name}", mm.Value);
                }
            }

            return metrics;
        }
    }
}
