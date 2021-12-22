using Microsoft.ML;
using System.IO;
using System.Linq;
using AIPF.MLManager;
using AIPF.MLManager.Modifiers;
using AIPF.Images;
using System;
using AIPF.Data;
using Microsoft.ML.Transforms;
using System.Collections.Generic;

namespace AIPF
{
    class Program
    {
        static void Main(string[] args)
        {
            //PredictUsingVectorPipeline();
            //PredictUsingBitmapPipeline();
            //PredictUsingMorePipeline();
            TaxiFarePrediction();
            //Example();

        }

        private static void TaxiFarePrediction()
        {

            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var mlManager = new MLManager<TaxiFareRaw, TaxiFareRaw>();
            //Load csv data
            var data = mlManager.MlLoader.Load($"{dir}/Data/TaxiFare/train_mini.csv");
            /*
            mlManager.CreatePipeline(new ParseDate<TaxiFareRaw>())
                .Append(new EuclideanDistance<TaxiFareRawWithDatatime>(filtro))*/
            
        }

        static void PredictUsingVectorPipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadImageFromFile($"{dir}/Data/MNIST/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<VectorRawImage, OutputImage>();
            mlMaster.CreatePipeline(new ProgressIndicator<VectorRawImage>(@"Process#1"))
                // Using our custom image resizer
                //.Append(new CustomImageResizer())
                // OR using the ml.net default ResizeImages method
                .Append(new VectorImageResizer())
                .Append(new SdcaMaximumEntropy(3));

            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);

            var metrics = mlMaster.EvaluateAll();
            Utils.PrintMetrics(metrics);

            // Digit = 6
            VectorRawImage rawImageToPredict = Utils.ReadImageFromFile($"{dir}/Data/image_to_predict.txt").First();
            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 0);
        }

        static void PredictUsingBitmapPipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadBitmapFromFile($"{dir}/Data/MNIST/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<BitmapRawImage, OutputImage>();
            mlMaster.CreatePipeline(new ProgressIndicator<BitmapRawImage>(@"Process#1"))
                .Append(new BitmapResizer())
                .Append(new SdcaMaximumEntropy(10));

            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);

            var metrics = mlMaster.EvaluateAll();
            Utils.PrintMetrics(metrics);

            // Digit = 6
            BitmapRawImage rawImageToPredict = Utils.ReadBitmapFromFile($"{dir}/Data/image_to_predict.txt").First();
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

        //-------------------------------------------------------------------------------

        // This example shows how to define and apply a custom mapping of input
        // columns to output columns with a contract name. The contract name is
        // used in the CustomMappingFactoryAttribute that decorates the custom
        // mapping action. The pipeline containing the custom mapping can then be
        // saved to disk, and it can be loaded back after the assembly containing
        // the custom mapping action is registered.
        public static void Example()
        {
            // Create a new ML context, for ML.NET operations. It can be used for
            // exception tracking and logging, as well as the source of randomness.
            var mlContext = new MLContext();

            // Get a small dataset as an IEnumerable and convert it to an IDataView.
            var samples = new List<InputData>
            {
                new InputData { Age = 26, Asd = "1213" },
                new InputData { Age = 35, Asd = "456456" },
                new InputData { Age = 34, Asd = "6" },
                new InputData { Age = 28, Asd = "8" },
            };
            var data = mlContext.Data.LoadFromEnumerable(samples);

            // Custom transformations can be used to transform data directly, or as
            // part of a pipeline of estimators. The contractName must be provided
            // in order for a pipeline containing a CustomMapping estimator to be
            // saved and loaded back. The contractName must be the same as in the
            // CustomMappingFactoryAttribute used to decorate the custom action
            // defined by the user.
            var pipeline = mlContext.Transforms.CustomMapping(new
                IsUnderThirtyCustomAction().GetMapping(), contractName:
                "IsUnderThirty");

            var transformer = pipeline.Fit(data);

            // To save and load the CustomMapping estimator, the assembly in which
            // the custom action is defined needs to be registered in the
            // environment. The following registers the assembly where
            // IsUnderThirtyCustomAction is defined.    
            // This is necessary only in versions v1.5-preview2 and earlier
            mlContext.ComponentCatalog.RegisterAssembly(typeof(
                IsUnderThirtyCustomAction).Assembly);

            // Now the transform pipeline can be saved and loaded through the usual
            // MLContext method. 
            mlContext.Model.Save(transformer, data.Schema, "customTransform.zip");
            var loadedTransform = mlContext.Model.Load("customTransform.zip", out
                var inputSchema);

            // Now we can transform the data and look at the output to confirm the
            // behavior of the estimator. This operation doesn't actually evaluate
            // data until we read the data below.
            var transformedData = loadedTransform.Transform(data);

            var dataEnumerable = mlContext.Data.CreateEnumerable<TransformedData>(
                transformedData, reuseRowObject: true);

            Console.WriteLine("Age\tIsUnderThirty");
            foreach (var row in dataEnumerable)
                Console.WriteLine($"\t {row.IsUnderThirty}\t {row.Asd}");

            // Expected output:
            // Age      IsUnderThirty
            // 26       True
            // 35       False
            // 34       False
            // 28       True
        }

        // The custom action needs to implement the abstract class
        // CustomMappingFactory, and needs to have attribute
        // CustomMappingFactoryAttribute with argument equal to the contractName
        // used to define the CustomMapping estimator which uses the action.
        [CustomMappingFactoryAttribute("IsUnderThirty")]
        private class IsUnderThirtyCustomAction : CustomMappingFactory<InputData,
            CustomMappingOutput>
        {
            // We define the custom mapping between input and output rows that will
            // be applied by the transformation.
            public static void CustomAction(InputData input, CustomMappingOutput
                output) => output.IsUnderThirty = input.Age < 30;

            public override Action<InputData, CustomMappingOutput> GetMapping()
                => CustomAction;
        }

        // Defines only the column to be generated by the custom mapping
        // transformation in addition to the columns already present.
        private class CustomMappingOutput
        {
            public bool IsUnderThirty { get; set; }
        }

        // Defines the schema of the input data.
        private class InputData
        {
            public float Age { get; set; }
            public string Asd { get; set; }
        }

        // Defines the schema of the transformed data, which includes the new column
        // IsUnderThirty.
        private class TransformedData
        {
            public bool IsUnderThirty { get; set; }
            public string Asd { get; set; }
        }

    }
}
