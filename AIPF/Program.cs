using Microsoft.ML;
using System.IO;
using AIPF.MLManager;
using System;
using AIPF.MLManager.Modifiers.Date;
using AIPF.Models.Taxi;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.MLManager.Modifiers.TaxiFare;
using AIPF.MLManager.Modifiers.Columns;
using AIPF.MLManager.Actions.Filters;
using System.Linq;
using AIPF.Models.Images;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.ML.Data;
using AIPF.MLManager.Actions;
using System.Reflection;
using AIPF.MLManager.Modifiers;
using AIPF.Reflection;

namespace AIPF
{
    class Program
    {
        static void Main(string[] args)
        {
            DynamicIdataview();

            //PredictUsingVectorPipeline();
            //PredictUsingBitmapPipeline();
            //PredictUsingMorePipeline();
            //TaxiFarePrediction();
        }

        public static void DynamicIdataview()
        {
            var properties = new List<DynamicTypeProperty>()
                {
                    new DynamicTypeProperty("FareAmount", typeof(float)),
                    new DynamicTypeProperty("X1", typeof(float)),
                    new DynamicTypeProperty("Y1", typeof(float)),
                    new DynamicTypeProperty("X2", typeof(float)),
                    new DynamicTypeProperty("Y2", typeof(float)),
                    new DynamicTypeProperty("PassengersCount", typeof(float)),
                    new DynamicTypeProperty("Distance", typeof(float))
                };

            // create the new type
            var dynamicType = DynamicType.CreateDynamicType(properties, typeof(ICoordinates), typeof(ICopy<>), typeof(IDistance));
            var schema = SchemaDefinition.Create(dynamicType);


            var dynamicList = DynamicType.CreateDynamicList(dynamicType);

            // get an action that will add to the list
            var addAction = DynamicType.GetAddAction(dynamicList);

            // call the action, with an object[] containing parameters in exact order added
            addAction.Invoke(new object[] { 5.7f, -73.982738f, 40.76127f, -73.991242f, 40.750562f, 2f, 0f });
            // call add action again for each row.

            var mlContext = new MLContext();
            var dataType = mlContext.Data.GetType();
            var loadMethodGeneric = dataType.GetMethods().First(method => method.Name == "LoadFromEnumerable" && method.IsGenericMethod);
            var loadMethod = loadMethodGeneric.MakeGenericMethod(dynamicType);
            var trainData = (IDataView)loadMethod.Invoke(mlContext.Data, new[] { dynamicList, schema });
            trainData.Preview();

            Func<object, bool> func = i =>
            {
                dynamic f = i;
                return f.PassengersCount > 2;
            };

            var mapType = typeof(CustomMappingCatalog);
            var loadMethodGeneric2 = mapType.GetMethods().First(method => method.Name == "FilterByCustomPredicate" && method.IsGenericMethod);
            var loadMethod2 = loadMethodGeneric2.MakeGenericMethod(dynamicType);
            var filteredData = (IDataView)loadMethod2.Invoke(mlContext.Data, new object[] { mlContext.Data, trainData, func });
            filteredData.Preview();

            // new MLManager<dynamicType, dynamicType>
            var mlManType = typeof(MLManager<,>);
            Type[] typeArgs = { dynamicType, dynamicType };
            var makeme = mlManType.MakeGenericType(typeArgs); // MLManager<dynamicType, dynamicType>
            object o = Activator.CreateInstance(makeme); // o = new MLManager<dynamicType, dynamicType>()
            MethodInfo myMethod = makeme.GetMethod("CreatePipeline");
            var mlbuilder = myMethod.Invoke(o, new object[] { }); // mlManager.CreatePipeline()

            //mlManager.CreatePipeline().AddFilter(expression);
            var mlBuiType = mlbuilder.GetType();
            var loadMethodGeneric4 = mlBuiType.GetMethods().First(method => method.Name == "AddFilter");

            var parameter = Expression.Parameter(dynamicType, "i");
            var memberExpression = Expression.Property(parameter, "PassengersCount");
            var gte = Expression.LessThanOrEqual(memberExpression, Expression.Constant(1f));
            var lte = Expression.GreaterThanOrEqual(memberExpression, Expression.Constant(10f));
            var lambdaExpression = Expression.Lambda(Expression.AndAlso(gte, lte), parameter); // i => i.PassengersCount >= 1 && i.PassengersCount <= 10

            var mlbuilder2 = loadMethodGeneric4.Invoke(mlbuilder, new object[] { lambdaExpression });


            //mlManager....AddTransformer(new EuclideanDistance<dynamicType, dynamicType>())
            var mlTranType = typeof(EuclideanDistance<,>);
            var makemetran = mlTranType.MakeGenericType(typeArgs);
            object tran = Activator.CreateInstance(makemetran); // tran = new EuclideanDistance<dynamicType, dynamicType>()

            //var mlBuiType2 = mlbuilder2.GetType();
            var loadMethodGeneric5 = mlBuiType.GetMethods().First(method => method.Name == "AddTransformer");
            var loadMethod5 = loadMethodGeneric5.MakeGenericMethod(dynamicType);
            var mlbuilder3 = loadMethod5.Invoke(mlbuilder2, new[] { tran });

            MethodInfo fitMethod = makeme.GetMethods().First(m => m.Name == "Fit" && m.GetParameters()[0].ParameterType == typeof(IDataView));
            IDataView tr = null;
            var parameters = new object[] { trainData, tr };
            fitMethod.Invoke(o, parameters); // mlManager.CreatePipeline()

            var transformedDataView = (IDataView)parameters[1];
            transformedDataView.Preview();


            /*
            var mlManager = new MLManager<object, object>();

            Func<object, bool> func = i =>
            {
                dynamic f = i;
                Console.WriteLine("cioqa");
                //return (int)GetPropValue(i, "b") > 10;
                return true;
            };
            Expression<Func<object, bool>> expression = i => func(i);
            mlManager.CreatePipeline().AddFilter(expression);

            mlManager.Fit(trainData, out IDataView tr);
            tr.Preview();
            */
        }

        private static void TaxiFarePrediction()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var mlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>();
            mlManager.CreatePipeline()
                .AddFilter(new MissingPropertyFilter<RawStringTaxiFare>())
                .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                .AddTransformer(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Build()
                .AddFilter(i => i.Distance > 0 && i.Distance <= 0.5)
                .AddTransformer(new ConcatenateColumn<ProcessedTaxiFare>("input", nameof(ProcessedTaxiFare.Date), nameof(ProcessedTaxiFare.Distance), nameof(ProcessedTaxiFare.PassengersCount)))
                .Append(new ApplyOnnxModel<ProcessedTaxiFare, object>($"{dir}/Data/TaxiFare/Onnx/skl_pca.onnx"))
                .Append(new DeleteColumn<object>("input"))
                .Append(new RenameColumn2<object>("variable", "input"))
                .Append(new DeleteColumn<object>("variable"))
                .Append(new ApplyOnnxModel<object, PredictedFareAmount>($"{dir}/Data/TaxiFare/Onnx/skl_pca_linReg.onnx"))
                .Build();
            var data = new RawStringTaxiFare[] { };
            mlManager.Fit(data, out var dataView);
            dataView.Preview();
            var prediction = mlManager.Predict(new RawStringTaxiFare()
            {
                DateAsString = "2011-08-18 00:35:00 UTC",
                X1 = -73.982738f,
                Y1 = 40.76127f,
                X2 = -73.991242f,
                Y2 = 40.750562f,
                PassengersCount = 2,
                // FareAmount = 5.7
            });
            // hubReg = 6.7486925
            // linReg = 7.584161
            // pca_hubReg = 6.7486873
            // pca_linReg = 7.58416
            if (prediction != null) Console.WriteLine(prediction.FareAmount[0]);


            var metrics = mlManager.EvaluateAll(mlManager.Loader.LoadFile($"{dir}/Data/TaxiFare/train_mini.csv"));

            /*
            var loadedData = new MLContext().Data.CreateEnumerable<RawStringTaxiFare>(mlManager.Loader.LoadFile($"{dir}/Data/TaxiFare/train_mini.csv"),
                reuseRowObject: true);
            foreach (var item in loadedData)
            {
                var prediction2 = mlManager.Predict(item);
                if (prediction2 != null) Console.WriteLine($"actual:{item.FareAmount}, prediction:{prediction2.FareAmount[0]}");
            }
            */
        }

        static void PredictUsingVectorPipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadImageFromFile($"{dir}/Data/MNIST/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<VectorRawImage, OutputImage>();
            mlMaster.CreatePipeline()
                .AddTransformer(new ProgressIndicator<VectorRawImage>(@"Process#1"))
                // Using our custom image resizer
                //.Append(new CustomImageResizer())
                // OR using the ml.net default ResizeImages method
                .Append(new VectorImageResizer())
                .Append(new SdcaMaximumEntropy(3))
                .Build();

            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);

            //var metrics = mlMaster.EvaluateAll();
            //Utils.PrintMetrics(metrics);

            // Digit = 6
            VectorRawImage rawImageToPredict = Utils.ReadImageFromFile($"{dir}/Data/MNIST/image_to_predict.txt").First();
            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 0);

            var metrics = mlMaster.EvaluateAll(new MLContext().Data.LoadFromEnumerable(rawImageDataList));
        }

        /*
        static void PredictUsingBitmapPipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadBitmapFromFile($"{dir}/Data/MNIST/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<BitmapRawImage, OutputImage>();
            mlMaster.CreatePipeline(new ProgressIndicator<BitmapRawImage>(@"Process#1"))
                .Append(new BitmapResizer())
                .Append(new SdcaMaximumEntropy(100));

            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);

            var metrics = mlMaster.EvaluateAll();
            Utils.PrintMetrics(metrics);

            // Digit = 6
            BitmapRawImage rawImageToPredict = Utils.ReadBitmapFromFile($"{dir}/Data/MNIST/image_to_predict.txt").First();
            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 0);
        }

        static void PredictUsingMorePipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadImageFromFile($"{dir}/Data/optdigits_original_training.txt", 21);

            // Data pre-processing pipeline: 32x32 => 8x8 images (We don't use this pipeline to predict, only to get processed data!!)
            var preProcessingMlMaster = new MLManager<VectorRawImage, ProcessedImage>();
            preProcessingMlMaster.CreatePipeline(new ProgressIndicator<VectorRawImage>(@"Process#1_ResizingImage"))
                .Append(new CustomImageResizer());
            preProcessingMlMaster.Fit(rawImageDataList, out IDataView transformedDataView);
            transformedDataView.Preview();
            var processedImages = preProcessingMlMaster.GetEnumerable(transformedDataView);

            // Train & predict Pipeline
            var mlMaster = new MLManager<ProcessedImage, OutputImage>();
            mlMaster.CreatePipeline(new ProgressPercentageIndicator<ProcessedImage>(@"Process#2"))
                .Append(new SdcaMaximumEntropy(1));
            mlMaster.Fit(processedImages, out _);

            ProcessedImage rawImageToPredict = new ProcessedImage()
            {
                // Digit = 7
                Pixels = new float[] { 0, 0, 0, 0, 14, 13, 1, 0, 0, 0, 0, 5, 16, 16, 2, 0, 0, 0, 0, 14, 16, 12, 0, 0, 0, 1, 10, 16, 16, 12, 0, 0, 0, 3, 12, 14, 16, 9, 0, 0, 0, 0, 0, 5, 16, 15, 0, 0, 0, 0, 0, 4, 16, 14, 0, 0, 0, 0, 0, 1, 13, 16, 1, 0 }
            };

            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 7);
        }
        */
    }
}
