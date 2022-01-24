﻿using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIPF.MLManager.Modifiers.TaxiFare
{
    public class ApplyOnnxModel<I, O> : IModifier<I, O>, IEvaluable where O : class, new()
    {
        private string modelPath;
        private string[] inputColumnNames;
        private string[] outputColumnNames;


        public ApplyOnnxModel(string modelPath, string[] inputColumnNames = null, string[] outputColumnNames = null)
        {
            this.modelPath = modelPath;
            this.inputColumnNames = inputColumnNames;
            this.outputColumnNames = outputColumnNames;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            inputColumnNames ??= new string[] { };
            outputColumnNames ??= new string[] { };
            return mlContext.Transforms.ApplyOnnxModel(modelFile: modelPath, outputColumnNames: outputColumnNames, inputColumnNames: inputColumnNames);
        }

        public MetricContainer Evaluate(MLContext mlContext, IDataView data)
        {
            return null;
        }
    }
}
