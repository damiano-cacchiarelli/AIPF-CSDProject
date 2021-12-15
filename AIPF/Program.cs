using Microsoft.ML;
using System.IO;
using System.Linq;
using AIPF.MLManager;
using AIPF.MLManager.Modifiers;
using AIPF.Images;
using System;
using AIPF.Common;

namespace AIPF
{
    class Program
    {
        static void Main(string[] args)
        {
            ExampleMNIST();
            //PredictUsingVectorPipeline();
            //PredictUsingBitmapPipeline();
            //PredictUsingMorePipeline();
        }

        static void PredictUsingVectorPipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadImageFromFile($"{dir}/Data/optdigits_original_training.txt", 21);

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
            var rawImageDataList = Utils.ReadBitmapFromFile($"{dir}/Data/optdigits_original_training.txt", 21);

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

        static void ExampleMNIST()
        {

            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var mlMaster = new MLManager<VectorRawImage, OutputImage>();


            ConsoleHelper.WriteLine(@"       
                       _____ _____  ______ 
                 /\   |_   _|  __ \|  ____|
                /  \    | | | |__) | |__   
               / /\ \   | | |  ___/|  __|  
              / ____ \ _| |_| |    | |     
             /_/    \_\_____|_|    |_|      v1.0

       Cacchiarelli, Cesetti, Romagnoli 17/12/2021
            ");

            string line = string.Empty;
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleHelper.Write("$ ");
            Console.CursorTop = ConsoleHelper.NextCursorTop - 1;
            while ((line = Console.ReadLine()) != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.CursorVisible = false;
                var cmd = line.Split(" ");
                if (line.StartsWith("$"))
                    cmd = cmd.Skip(1).ToArray();
                if (cmd.Length == 0) continue;
                try
                {
                    switch (cmd[0])
                    {
                        case "training":
                            if (cmd.Length != 2)
                            {
                                ConsoleHelper.WriteLine("Command not found");
                                break;
                            }
                            //$"{dir}/Data/optdigits_original_training.txt"
                            var rawImageDataList = Utils.ReadImageFromFile(cmd[1], 21);
                            mlMaster.CreatePipeline(new ProgressIndicator<VectorRawImage>(@"Sdca Trainer"))
                                .Append(new VectorImageResizer())
                                .Append(new SdcaMaximumEntropy(10));
                            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);
                            break;
                        case "predict":
                            if (cmd.Length != 2)
                            {
                                ConsoleHelper.WriteLine("Command not found");
                                break;
                            }
                            //$"{dir}/Data/image_to_predict.txt"
                            VectorRawImage rawImageToPredict = Utils.ReadImageFromFile(cmd[1]).First();
                            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
                            Utils.PrintPrediction(predictedImage, rawImageToPredict.Digit);
                            break;
                        case "metrics":
                            if (cmd.Length != 1)
                            {
                                ConsoleHelper.WriteLine("Command not found");
                                break;
                            }
                            var metrics = mlMaster.EvaluateAll();
                            Utils.PrintMetrics(metrics);
                            break;
                        case "help":
                            ConsoleHelper.WriteLine(
                                @"List of commands:
$ training  <trainingFilePath>  Train the model on the spicific data set;
$ predict   <fileToPredict>     Predict a value using the created model;
$ metrics                       Print the metrics of the Machine Learning model;
$ help                          Print the list of commands;
$ exit"
                                );
                            break;
                        case "exit":
                            return;
                        default:
                            ConsoleHelper.WriteLine("Command not found");
                            break;
                    }
                    
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    ConsoleHelper.WriteLine("ERROR: " + e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleHelper.Write("$ ");
                Console.CursorTop = ConsoleHelper.NextCursorTop - 1;
                Console.CursorVisible = true;
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        
    }
}
