using AIPF.MLManager.Actions;
using AIPF.MLManager.EventQueue;
using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System;
using System.Collections.Generic;

namespace AIPF.MLManager
{
    public class MLManager<I, O> where I : class, new() where O : class, new()
    {
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

        public void Fit(IEnumerable<I> rawData, out IDataView transformedDataView)
        {
            Fit(mlContext.Data.LoadFromEnumerable(rawData), out transformedDataView);
        }

        public void Fit(IDataView rawData, out IDataView transformedDataView)
        {
            rawData.Preview();
            if (mlBuilder == null)
                throw new Exception("The pipeline must be valid");

            mlBuilder.Fit(rawData, out transformedDataView);
            trained = true;
            //this.MessageQueue.Unregister("1");
        }

        public O Predict(I toPredict)
        {
            if (mlBuilder == null)
                throw new Exception("The pipeline must be valid");
            if (!trained)
                throw new Exception("You must call Fit() before Predict()!");
            if (toPredict == null)
                throw new Exception("You must pass a valid item!");

            return mlBuilder.Predict(toPredict);
        }

        public List<MetricContainer> EvaluateAll(IEnumerable<I> data)
        {
            return EvaluateAll(mlContext.Data.LoadFromEnumerable(data));
        }

        public List<MetricContainer> EvaluateAll(IDataView dataView)
        {
            if (mlBuilder == null)
                throw new Exception("The pipeline must be valid");
            if (dataView == null)
                throw new Exception("You need to pass a data set to test the accurancy of the model!");
            if (!trained)
                throw new Exception("You must call Fit() before EvaluateAll()!");

            return mlBuilder.EvaluateAll(dataView);
        }
    }
}
