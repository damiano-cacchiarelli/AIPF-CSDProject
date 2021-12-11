using Microsoft.ML;
using System.IO;
using System.Linq;
using AIPF.MLManager;
using AIPF.MLManager.Modifiers;
using AIPF.Images;

namespace AIPF
{
    class Program
    {
        static void Main(string[] args)
        {
            PredictUsingOnePipeline();
        }

        static void PredictUsingOnePipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadImageFromFile($"{dir}/Data/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<RawImage, OutputImage>();
            mlMaster.CreatePipeline(new ProgressIndicator<RawImage>(@"Process#1"))
                // Using our custom image resizer
                .Append(new CustomImageResizer())
                .Append(new ConcatenateColumn<ProcessedImage>(nameof(ProcessedImage.Pixels), "Features"))
                .Append(new RenameColumn<ProcessedImage>(nameof(ProcessedImage.Digit), "Label"))
                // OR using the ml.net default ResizeImages method
                //.Append(new VectorImageResizer())
                .Append(new SdcaMaximumEntropy(1));

            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);

            var metrics = mlMaster.EvaluateAll();
            Utils.PrintMetrics(metrics);

            // Digit = 6
            RawImage rawImageToPredict = Utils.ReadImageFromFile($"{dir}/Data/image_to_predict.txt").First();
            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 6);
        }

        static void PredictUsingMorePipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadImageFromFile($"{dir}/Data/optdigits_original_training.txt", 21);

            // Data pre-processing pipeline: 32x32 => 8x8 images (We don't use this pipeline to predict, only to get processed data!!)
            var preProcessingMlMaster = new MLManager<RawImage, ProcessedImage>();
            preProcessingMlMaster.CreatePipeline(new ProgressIndicator<RawImage>(@"Process#1_ResizingImage"))
                .Append(new CustomImageResizer());
            preProcessingMlMaster.Fit(rawImageDataList, out IDataView transformedDataView);
            transformedDataView.Preview();
            var processedImages = preProcessingMlMaster.GetEnumerable(transformedDataView);

            // Train & predict Pipeline
            var mlMaster = new MLManager<ProcessedImage, OutputImage>();
            mlMaster.CreatePipeline(new ProgressPercentageIndicator<ProcessedImage>(@"Process#2"))
                .Append(new RenameColumn<ProcessedImage>(nameof(ProcessedImage.Digit), "Label"))
                .Append(new ConcatenateColumn<ProcessedImage>(nameof(ProcessedImage.Pixels), "Features"))
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
    }
}
