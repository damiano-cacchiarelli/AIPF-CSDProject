using AIPF.MLManager.Actions.Modifiers.Maths;
using AIPF_Console.TaxiFare_example.Model;
using Microsoft.ML;
using NUnit.Framework;

namespace AIPF_Test.TransformerTests
{
    class EuclideanDistanceTests
    {
        private MLContext mlContext;
        private MinutesTaxiFare goodItem;
        private MinutesTaxiFare badItem;

        [SetUp]
        public void Setup()
        {
            mlContext = new MLContext();
            goodItem = new MinutesTaxiFare()
            {
                Date = 24f,
                X1 = 4,
                Y1 = -3,
                X2 = 2,
                Y2 = -3,
                PassengersCount = 2,
                FareAmount = 5.7f
            };
            badItem = new MinutesTaxiFare()
            {
                Date = 55f,
                X1 = float.NaN,
                Y1 = 40.76127f,
                X2 = -73.991242f,
                Y2 = 40.750562f,
                PassengersCount = 12,
                FareAmount = 5.7f
            };
        }

        [Test]
        public void TestGetPipeline()
        {
            var distance = new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>();
            var pipeline = distance.GetPipeline(mlContext);
            var data = mlContext.Data.LoadFromEnumerable(new[] { goodItem });
            var model = pipeline.Fit(data);
            var engine = mlContext.Model.CreatePredictionEngine<MinutesTaxiFare, ProcessedTaxiFare>(model);
            var processed = engine.Predict(goodItem);

            Assert.AreEqual(goodItem.Date, processed.Date);
            Assert.AreEqual(goodItem.PassengersCount, processed.PassengersCount);
            Assert.AreEqual(2, processed.Distance);
        }

        [Test]
        public void TestGetPipeline_NaN()
        {
            var distance = new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>();
            var pipeline = distance.GetPipeline(mlContext);
            var data = mlContext.Data.LoadFromEnumerable(new MinutesTaxiFare[] { });
            var model = pipeline.Fit(data);
            var engine = mlContext.Model.CreatePredictionEngine<MinutesTaxiFare, ProcessedTaxiFare>(model);
            var processed = engine.Predict(badItem);

            Assert.AreEqual(badItem.Date, processed.Date);
            Assert.AreEqual(badItem.PassengersCount, processed.PassengersCount);
            Assert.AreEqual(float.NaN, processed.Distance);
        }
    }
}
