using AIPF.MLManager.Actions;
using AIPF.MLManager.Modifiers.Date;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.Models.Taxi;
using Microsoft.ML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF_Test
{
    class PipelineBuilderTests
    {
        private MLContext mlContext;
        private PipelineBuilder<RawStringTaxiFare, ProcessedTaxiFare> pipelineBuilder;
        private RawStringTaxiFare goodItem;

        [SetUp]
        public void Setup()
        {
            goodItem = new RawStringTaxiFare()
            {
                DateAsString = "2011-08-18 00:35:00 UTC",
                X1 = 5,
                Y1 = 4,
                X2 = 9,
                Y2 = 7,
                PassengersCount = 2,
                FareAmount = 5.7f
            };
            mlContext = new MLContext();
            var parser = new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute);
            var distance = new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>();
            pipelineBuilder = new PipelineBuilder<RawStringTaxiFare, ProcessedTaxiFare>(mlContext, null);
            pipelineBuilder.CreatePipeline(parser).Append(distance);
        }

        [Test]
        public void CreatePipeline()
        {
            Assert.IsNull(pipelineBuilder.Model);
        }

        [Test]
        public void TestExecute()
        {
            var data = mlContext.Data.LoadFromEnumerable(new[] { goodItem });

            var newPipelineBuilder = new PipelineBuilder<RawStringTaxiFare, ProcessedTaxiFare>(mlContext, null);
            Assert.Throws<Exception>(() => newPipelineBuilder.Execute(data, out IDataView _));

            pipelineBuilder.Execute(data, out IDataView processedData);
            var processed = mlContext.Data.CreateEnumerable<ProcessedTaxiFare>(processedData, reuseRowObject: true);
            foreach (var item in processed)
            {
                CheckEquality(item);
            }
        }

        [Test]
        public void TestPredict()
        {
            var newPipelineBuilder = new PipelineBuilder<RawStringTaxiFare, ProcessedTaxiFare>(mlContext, null);
            Assert.Throws<Exception>(() => newPipelineBuilder.Predict(goodItem));
            Assert.Throws<Exception>(() => pipelineBuilder.Predict(goodItem));

            var data = mlContext.Data.LoadFromEnumerable(new RawStringTaxiFare[] { });
            pipelineBuilder.Execute(data, out IDataView _);
            var output = pipelineBuilder.Predict(goodItem);
            CheckEquality(output);
        }

        private void CheckEquality(ProcessedTaxiFare item)
        {
            Assert.AreEqual(35, item.Date);
            Assert.AreEqual(2, item.PassengersCount);
            Assert.AreEqual(5, item.Distance);
        }
    }
}
