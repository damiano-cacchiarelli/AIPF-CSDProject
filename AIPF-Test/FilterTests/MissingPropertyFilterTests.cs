using AIPF.MLManager.Actions.Filters;
using AIPF_Console.TaxiFare_example.Model;
using Microsoft.ML;
using NUnit.Framework;

namespace AIPF_Test.FilterTests
{
    class MissingPropertyFilterTests
    {
        private MLContext mlContext;
        private MissingPropertyFilter<RawStringTaxiFare> filter;
        private RawStringTaxiFare goodItem;
        private RawStringTaxiFare badItem;

        [SetUp]
        public void Setup() 
        {
            mlContext = new MLContext();
            filter = new MissingPropertyFilter<RawStringTaxiFare>();
            filter.MLContext = mlContext;

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
                DateAsString = "2011-08-18 00:35:00 UTC",
                X1 = float.NaN,
                Y1 = 40.76127f,
                X2 = -73.991242f,
                Y2 = 40.750562f,
                PassengersCount = 2,
                FareAmount = 5.7f
            };
        }

        [Test]
        public void ApplyFilter()
        {
            Assert.IsTrue(filter.ApplyFilter(goodItem));
            Assert.IsFalse(filter.ApplyFilter(badItem));
        }

        [Test]
        public void TestExecute()
        {
            var items = new[] { goodItem, badItem };
            IDataView dataView = mlContext.Data.LoadFromEnumerable(items);
            filter.Execute(dataView, out IDataView transformedDataView);
            var preview = transformedDataView.Preview();
            Assert.AreEqual(1, preview.RowView.Length);
        }
    }
}
