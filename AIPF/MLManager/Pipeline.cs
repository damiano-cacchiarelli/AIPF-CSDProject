using Microsoft.ML;
using AIPF.MLManager.Modifiers;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AIPF.MLManager
{
    public class Pipeline<T> : IPipeline where T : class, new()
    {
        private readonly IModificator modificator;
        private IPipeline next = null;

        public Pipeline(IModificator modificator)
        {
            this.modificator = modificator;
        }

        public Pipeline<R> Append<R>(IModifier<T, R> modifier) where R : class, new()
        {
            var pipeline = new Pipeline<R>(modifier);
            next = pipeline;
            return pipeline;
        }

        public IPipeline GetNext()
        {
            return next;
        }

        public IModificator GetModificator()
        {
            return modificator;
        }

        public void PrintPipelineStructure()
        {
            int index = 1;
            foreach (var p in this)
            {
                Console.WriteLine($"{index++} - { p.GetModificator().GetType() }");
            }
            Console.WriteLine("");
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            IEstimator<ITransformer> pipeline = null;
            foreach (var p in this)
            {
                if (pipeline == null) pipeline = p.GetModificator().GetPipeline(mlContext);
                else pipeline = pipeline.Append(p.GetModificator().GetPipeline(mlContext));
            }
            return pipeline;
        }

        /* Another implementation of GetPipeline (using while instead of foreach)
        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            var pipeline = modificator.GetPipeline(mlContext);
            var nextPipeline = next;
            while(nextPipeline != null)
            {
                pipeline = pipeline.Append(nextPipeline.GetModificator().GetPipeline(mlContext));
                nextPipeline = nextPipeline.GetNext();
            }
            return pipeline;
        }
        */

        public IEnumerator<IPipeline> GetEnumerator()
        {
            IPipeline current = this;
            while (current != null)
            {
                yield return current;
                current = current.GetNext();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
