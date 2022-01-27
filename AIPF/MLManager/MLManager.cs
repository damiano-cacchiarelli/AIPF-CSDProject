using AIPF.MLManager.Actions;
using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AIPF.MLManager
{
    public class MLManager<I, O> where I : class, new() where O : class, new()
    {
        public static readonly ActivitySource source = new ActivitySource(nameof(MLManager));

        private readonly MLContext mlContext;

        private MLBuilder<I, O> mlBuilder;
        private bool trained = false;

        public MLLoader<I> Loader { get; private set; }

        public MLManager()
        {
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
                mlBuilder.Fit(rawData, out IDataView transformedDataView);
                trained = true;
                activity?.AddEvent(new ActivityEvent("End fit!", DateTimeOffset.Now));
                return transformedDataView;
            });
        }

        public async Task<O> Predict(I toPredict)
        {
            if (mlBuilder == null)
                throw new Exception("The pipeline must be valid");
            if (!trained)
                throw new Exception("You must call Fit() before Predict()!");
            if (toPredict == null)
                throw new Exception("You must pass a valid item!");

            return await Task.Run(() => {
                using var activity = source.StartActivity("Predict");
                var prediction = mlBuilder.Predict(toPredict);
                activity?.AddEvent(new ActivityEvent("End Prediction!", DateTimeOffset.Now));
                return prediction;
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
            if (!trained)
                throw new Exception("You must call Fit() before EvaluateAll()!");

            return await Task.Run(() =>
            {

                return setMetrics(mlBuilder.EvaluateAll(dataView));

                //activity?.AddEvent(new ActivityEvent("End EvaluateAll!", DateTimeOffset.Now));
                //return metrics;
            });
        }

        private List<MetricContainer> setMetrics(List<MetricContainer> metrics)
        {
            using var activity = source.StartActivity("EvaluateAll");
            foreach (var m in metrics)
            {
                foreach (var mm in m.Metrics)
                {
                    //m.Name + mm.Name + ", " + mm.Value;
                    activity?.SetTag(m.Name+"."+mm.Name, mm.Value);
                }
            }

            return metrics;
        }
    }
}
