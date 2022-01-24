using AIPF.MLManager;
using AIPF.MLManager.Actions;
using AIPF.MLManager.Actions.Filters;
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
    class PipelineTests
    {
        private MLContext mlContext;

        [SetUp]
        public void Setup()
        {
            mlContext = new MLContext();
        }

        [Test]
        public void PipelineCreation()
        {
            var parser = new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute);
            var distance = new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>();
            EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare> nullDistance = null;

            var pipeline = new Pipeline<MinutesTaxiFare, ProcessedTaxiFare>(parser, null);
            Assert.IsNull(pipeline.GetNext());
            Assert.AreEqual(parser, pipeline.GetModificators());
            Assert.AreEqual(1, pipeline.GetModificators().Count);

            var pipeline2 = pipeline.Append(distance);
            Assert.IsNull(pipeline2.GetNext());
            Assert.AreEqual(distance, pipeline2.GetModificators());
            Assert.AreEqual(1, pipeline2.GetModificators().Count);

            Assert.IsNotNull(pipeline.GetNext());
            Assert.AreEqual(2, pipeline.GetModificators().Count);

            Assert.Throws<Exception>(() => pipeline.Append(nullDistance));
        }

        [Test]
        public void PipelineBuild()
        {
            var parser = new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute);
            var distance = new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>();
            var mlBuilder = new MLBuilder<RawStringTaxiFare, PredictedFareAmount>();
            var newMlBuilder = new Pipeline<MinutesTaxiFare, ProcessedTaxiFare>(parser, mlBuilder)
                .Append(distance)
                .Build();

            Assert.AreNotEqual(mlBuilder, newMlBuilder);
        }

        [Test]
        public void GetPipeline()
        {
            var parser = new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute);
            var distance = new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>();
            var pipeline = new Pipeline<MinutesTaxiFare, ProcessedTaxiFare>(parser, null);
            pipeline.Append(distance);

            var goodItem = new RawStringTaxiFare()
            {
                DateAsString = "2011-08-18 00:35:00 UTC",
                X1 = 5,
                Y1 = 4,
                X2 = 9,
                Y2 = 7,
                PassengersCount = 2,
                FareAmount = 5.7f
            };
            var mlPipeline = pipeline.GetPipeline(mlContext);
            var data = mlContext.Data.LoadFromEnumerable(new[] { goodItem });
            var model = mlPipeline.Fit(data);
            var engine = mlContext.Model.CreatePredictionEngine<RawStringTaxiFare, ProcessedTaxiFare>(model);
            var processed = engine.Predict(goodItem);

            Assert.AreEqual(35, processed.Date);
            Assert.AreEqual(2, processed.PassengersCount);
            Assert.AreEqual(5, processed.Distance);
        }
    }
}
