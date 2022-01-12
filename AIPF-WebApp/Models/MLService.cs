using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AIPF.MLManager;
using AIPF.MLManager.Actions.Filters;
using AIPF.MLManager.Modifiers;
using AIPF.MLManager.Modifiers.Columns;
using AIPF.MLManager.Modifiers.Date;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.MLManager.Modifiers.TaxiFare;
using AIPF.Models.Images;
using AIPF.Models.Taxi;

namespace AIPF_WebApp.Models
{
    public class MLService
    {
        private MLManager<RawStringTaxiFare, PredictedFareAmount> taxiFareMlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>();
        private MLManager<VectorRawImage, OutputImage> mnistMlManager = new MLManager<VectorRawImage, OutputImage>();

        public MLService()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            taxiFareMlManager.CreatePipeline()
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

            mnistMlManager.CreatePipeline()
                .AddTransformer(new ProgressIndicator<VectorRawImage>(@"Process#1"))
                // Using our custom image resizer
                //.Append(new CustomImageResizer())
                // OR using the ml.net default ResizeImages method
                .Append(new VectorImageResizer())
                .Append(new SdcaMaximumEntropy(3))
                .Build();
        }

        public void Fit(FitBody fitBody)
        {
            switch (fitBody.ModelName)
            {
                case "taxifare":
                    Console.WriteLine((IEnumerable<RawStringTaxiFare>)fitBody.Data);
                    taxiFareMlManager.Fit((IEnumerable<RawStringTaxiFare>)fitBody.Data, out var _);
                    break;
                case "MNIST":
                    mnistMlManager.Fit(fitBody.Data as IEnumerable<VectorRawImage>, out var _);
                    break;
                default:
                    throw new ArgumentException("Model name not found");
            }
        }

    }
}
