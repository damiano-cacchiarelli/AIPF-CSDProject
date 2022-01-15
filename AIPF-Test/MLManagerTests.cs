using AIPF.MLManager;
using AIPF.MLManager.Actions.Filters;
using AIPF.MLManager.Modifiers;
using AIPF.MLManager.Modifiers.Columns;
using AIPF.MLManager.Modifiers.Date;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.MLManager.Modifiers.TaxiFare;
using AIPF.Models.Taxi;
using Microsoft.ML;
using NUnit.Framework;
using System;
using System.IO;

namespace AIPF_Test
{
    public class MLManagerTests
    {
        private string directory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName+"/AIPF-Console/TaxiFare-example";
        private RawStringTaxiFare goodItem;
        private RawStringTaxiFare longDistanceItem;
        private RawStringTaxiFare missingValueItem;
        private RawStringTaxiFare tooPassengersItem;

        [SetUp]
        public void Setup()
        {
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
            longDistanceItem = new RawStringTaxiFare()
            {
                DateAsString = "2011-08-18 00:35:00 UTC",
                X1 = 5,
                Y1 = 4,
                X2 = 9,
                Y2 = 7,
                PassengersCount = 2,
                FareAmount = 5.7f
            };
            missingValueItem = new RawStringTaxiFare()
            {
                DateAsString = "2011-08-18 00:35:00 UTC",
                X1 = float.NaN,
                Y1 = 40.76127f,
                X2 = -73.991242f,
                Y2 = 40.750562f,
                PassengersCount = 2,
                FareAmount = 5.7f
            };
            tooPassengersItem = new RawStringTaxiFare()
            {
                DateAsString = "2011-08-18 00:35:00 UTC",
                X1 = -73.982738f,
                Y1 = 40.76127f,
                X2 = -73.991242f,
                Y2 = 40.750562f,
                PassengersCount = 12,
                FareAmount = 5.7f
            };
        }

        [Test]
        public void MLManagerCreate()
        {
            var mlManager = new MLManager<object, object>();
            var obj = new object[] { };

            Assert.Throws<Exception>(() => mlManager.Fit(obj, out IDataView _));
            Assert.Throws<Exception>(() => mlManager.Predict(null));
            Assert.Throws<Exception>(() => mlManager.EvaluateAll(obj));
        }

        [Test]
        public void MLManagerFit()
        {
            var mlManager = new MLManager<RawStringTaxiFare, ProcessedTaxiFare>();
            mlManager.CreatePipeline()
                .AddFilter(new MissingPropertyFilter<RawStringTaxiFare>())
                .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                .AddTransformer(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Build();

            mlManager.Fit(new[] { longDistanceItem }, out var dataView);
            var preview = dataView.Preview();
            Assert.AreEqual(1, preview.RowView.Length);

            mlManager.Fit(new[] { missingValueItem }, out dataView);
            preview = dataView.Preview();
            Assert.AreEqual(0, preview.RowView.Length);

            mlManager.Fit(new[] { longDistanceItem, missingValueItem, tooPassengersItem }, out dataView);
            preview = dataView.Preview();
            Assert.AreEqual(1, preview.RowView.Length);
        }

        [Test]
        public void MLManagerPredict()
        {
            var mlManager = new MLManager<RawStringTaxiFare, ProcessedTaxiFare>();
            mlManager.CreatePipeline()
                .AddFilter(new MissingPropertyFilter<RawStringTaxiFare>())
                .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                .AddTransformer(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Build();

            Assert.Throws<Exception>(() => mlManager.Predict(longDistanceItem));

            mlManager.Fit(new RawStringTaxiFare[] { }, out var _);

            Assert.Throws<Exception>(() => mlManager.Predict(null), "Item null");
            Assert.Throws<Exception>(() => mlManager.Predict(missingValueItem), "Missing values");
            Assert.Throws<Exception>(() => mlManager.Predict(tooPassengersItem), "Too passengers");

            var processed = mlManager.Predict(longDistanceItem);
            Assert.AreEqual(35, processed.Date);
            Assert.AreEqual(2, processed.PassengersCount);
            Assert.AreEqual(5, processed.Distance);
        }

        [Test]
        public void MLManagerPredict2()
        {
            var mlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>();
            mlManager.CreatePipeline()
                .AddFilter(new MissingPropertyFilter<RawStringTaxiFare>())
                .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                .AddTransformer(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Build()
                .AddFilter(i => i.Distance > 0 && i.Distance <= 0.5)
                .AddTransformer(new ConcatenateColumn<ProcessedTaxiFare>("input", nameof(ProcessedTaxiFare.Date), nameof(ProcessedTaxiFare.Distance), nameof(ProcessedTaxiFare.PassengersCount)))
                .Append(new ApplyOnnxModel<ProcessedTaxiFare, object>($"{directory}/Data/Onnx/skl_pca.onnx"))
                .Append(new DeleteColumn<object>("input"))
                .Append(new RenameColumn2<object>("variable", "input"))
                .Append(new DeleteColumn<object>("variable"))
                .Append(new ApplyOnnxModel<object, PredictedFareAmount>($"{directory}/Data/Onnx/skl_pca_linReg.onnx"))
                .Build();

            mlManager.Fit(new RawStringTaxiFare[] { }, out var _);

            Assert.Throws<Exception>(() => mlManager.Predict(longDistanceItem), "Long distance");
            var processed = mlManager.Predict(goodItem);
            Assert.IsTrue(processed.FareAmount.Length == 1);
            Assert.AreEqual(7.58416f, processed.FareAmount[0]);
        }
    }
}