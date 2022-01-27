using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AIPF.MLManager;
using AIPF.MLManager.Actions.Filters;
using AIPF.MLManager.Metrics;
using AIPF.MLManager.Modifiers;
using AIPF.MLManager.Modifiers.Columns;
using AIPF.MLManager.Modifiers.Date;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.MLManager.Modifiers.TaxiFare;
using AIPF.Models.Taxi;
using AIPF_Console.MNIST_example.Model;
using AIPF_Console.MNIST_example.Modifiers;
using AIPF_Console.RobotLoccioni_example.Model;
using Microsoft.ML;

namespace AIPF_RESTController.Models
{
    public class MLService
    {
        private MLManager<RawStringTaxiFare, PredictedFareAmount> taxiFareMlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>();
        private MLManager<VectorRawImage, OutputImage> mnistMlManager = new MLManager<VectorRawImage, OutputImage>();
        private MLManager<RobotData, OutputMeasure> robotMlManager = new MLManager<RobotData, OutputMeasure>();

        public MLService()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "/AIPF-Console";

            taxiFareMlManager.CreatePipeline()
                .AddFilter(new MissingPropertyFilter<RawStringTaxiFare>())
                .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                .AddTransformer(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Build()
                .AddFilter(i => i.Distance > 0 && i.Distance <= 0.5)
                .AddTransformer(new ConcatenateColumn<ProcessedTaxiFare>("input", nameof(ProcessedTaxiFare.Date), nameof(ProcessedTaxiFare.Distance), nameof(ProcessedTaxiFare.PassengersCount)))
                .Append(new ApplyOnnxModel<ProcessedTaxiFare, object>($"{dir}/TaxiFare-example/Data/Onnx/skl_pca.onnx"))
                .Append(new DeleteColumn<object>("input"))
                .Append(new RenameColumn<object>("variable", "input"))
                .Append(new DeleteColumn<object>("variable"))
                .Append(new ApplyOnnxModel<object, PredictedFareAmount>($"{dir}/TaxiFare-example/Data/Onnx/skl_pca_linReg.onnx"))
                .Build();

            mnistMlManager.CreatePipeline()
                .AddTransformer(new ProgressIndicator<VectorRawImage>(@"Process#1"))
                // Using our custom image resizer
                //.Append(new CustomImageResizer())
                // OR using the ml.net default ResizeImages method
                .Append(new VectorImageResizer())
                .Append(new SdcaMaximumEntropy(3))
                .Build();


            var propertiesName = typeof(RobotData).GetProperties().Where(p => p.Name.Contains("Axis")).Select(p => p.Name).ToArray();
            robotMlManager.CreatePipeline()
                //.AddFilter(new MissingPropertyFilter<RobotData>())
                //.AddFilter(i => i.EventType != 0)
                .AddTransformer(new ConcatenateColumn<RobotData>("float_input", propertiesName))
                .Append(new ApplyOnnxModel<RobotData, OutputMeasure>($"{dir}/RobotLoccioni-example/Data/Onnx/modello_correnti_robot.onnx"))
                .Build();
        }

        public Task<IDataView> Fit(FitBody fitBody)
        {
            switch (fitBody.ModelName)
            {
                case "Taxi-Fare":
                    var list1 = CastListObject<RawStringTaxiFare>(fitBody.Data);
                    return taxiFareMlManager.Fit(list1);
                case "MNIST":
                    var list2 = CastListObject<VectorRawImage>(fitBody.Data);
                    return mnistMlManager.Fit(list2);
                case "Robot-Loccioni":
                    var list3 = CastListObject<RobotData>(fitBody.Data);
                    return robotMlManager.Fit(list3);
                default:
                    throw new ArgumentException("Model name not found");
            }
        }

        public async Task<object> Predict(string modelName, JsonElement value)
        {
            switch (modelName)
            {
                case "Taxi-Fare":
                    RawStringTaxiFare obj =
                        JsonSerializer.Deserialize<RawStringTaxiFare>(value.GetRawText());
                    return await taxiFareMlManager.Predict(obj);
                case "MNIST":
                    VectorRawImage obj2 =
                        JsonSerializer.Deserialize<VectorRawImage>(value.GetRawText());
                    return await mnistMlManager.Predict(obj2);
                case "Robot-Loccioni":
                    RobotData obj3 = JsonSerializer.Deserialize<RobotData>(value.GetRawText());
                    return await robotMlManager.Predict(obj3);
                default:
                    throw new ArgumentException("Model name not found");
            }
        }

        public async Task<List<MetricContainer>> Metrics(FitBody fitBody)
        {
            List<MetricContainer> metrics = null;
            switch (fitBody.ModelName)
            {
                case "Taxi-Fare":
                    var list1 = CastListObject<RawStringTaxiFare>(fitBody.Data);
                    metrics = await taxiFareMlManager.EvaluateAll(list1);
                    break;
                case "MNIST":
                    var list2 = CastListObject<VectorRawImage>(fitBody.Data);
                    metrics = await mnistMlManager.EvaluateAll(list2);
                    break;
                case "Robot-Loccioni":
                    var list3 = CastListObject<RobotData>(fitBody.Data);
                    metrics = await robotMlManager.EvaluateAll(list3);
                    break;
                default:
                    throw new ArgumentException("Model name not found");
            }
            return metrics;
        }

        private List<T> CastListObject<T>(IList<JsonElement> values)
        {
            var list = new List<T>();
            foreach (var e in values)
            {
                T obj = JsonSerializer.Deserialize<T>(e.GetRawText());
                list.Add(obj);
            }
            return list;
        }

        private T CastObject<T>(object value)
        {
            return (T)value;
        }
    }
}
