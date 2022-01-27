using AIPF.MLManager.Modifiers.Date;
using AIPF_Console.TaxiFare_example.Model;
using Microsoft.ML;
using NUnit.Framework;
using System;

namespace AIPF_Test.TransformerTests
{
    class GenericDateParserTests
    {
        private MLContext mlContext;
        private RawStringTaxiFare goodItem;
        private RawStringTaxiFare badItem;

        [SetUp]
        public void Setup()
        {
            mlContext = new MLContext();
            goodItem = new RawStringTaxiFare()
            {
                DateAsString = "2011-08-18 00:35:00 UTC",
                X1 = -73.982738f,
                Y1 = 40.76127f,
                X2 = -73.991242f,
                Y2 = 40.750562f,
                PassengersCount = 2,
                FareAmount = 5.7f
            };
            badItem = new RawStringTaxiFare()
            {
                DateAsString = "2011-08-18 00:35:00 UTC Error",
                X1 = -73.982738f,
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
            var parser = new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute);
            var pipeline = parser.GetPipeline(mlContext);
            var data = mlContext.Data.LoadFromEnumerable(new[] { goodItem });
            var model = pipeline.Fit(data);
            var outData = model.Transform(data);
            var processed = mlContext.Data.CreateEnumerable<MinutesTaxiFare>(outData, reuseRowObject: true);
            foreach (var item in processed)
            {
                Assert.AreEqual(35f, item.Date);
                Assert.AreEqual(goodItem.FareAmount, item.FareAmount);
                Assert.AreEqual(goodItem.PassengersCount, item.PassengersCount);
                Assert.AreEqual(goodItem.X1, item.X1);
                Assert.AreEqual(goodItem.X2, item.X2);
                Assert.AreEqual(goodItem.Y1, item.Y1);
                Assert.AreEqual(goodItem.Y2, item.Y2);
            }
        }

        [Test]
        public void TestGetPipeline_WrongDateFormat()
        {
            var parser = new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute);
            var pipeline = parser.GetPipeline(mlContext);
            var data = mlContext.Data.LoadFromEnumerable(new RawStringTaxiFare[] { });
            var model = pipeline.Fit(data);
            var engine = mlContext.Model.CreatePredictionEngine<RawStringTaxiFare, MinutesTaxiFare>(model);
            Assert.Throws<Exception>(() => engine.Predict(badItem));
            Assert.DoesNotThrow(() => engine.Predict(goodItem));
        }
    }
}
