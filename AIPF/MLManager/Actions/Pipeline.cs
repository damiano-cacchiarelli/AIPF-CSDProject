using Microsoft.ML;
using AIPF.MLManager.Modifiers;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AIPF.MLManager.Actions
{
    public class Pipeline<T, O> : IPipeline where T : class, new() where O : class, new()
    {
        private readonly IModificator modificator;
        private readonly IMLBuilder mlBuilder;
        private IPipeline next = null;

        public Pipeline(IModificator modificator, IMLBuilder mlBuilder)
        {
            this.modificator = modificator;
            this.mlBuilder = mlBuilder;
        }

        public Pipeline<R, O> Append<R>(IModifier<T, R> modifier) where R : class, new()
        {
            if (modifier == null) 
                throw new Exception("The modificator cannot be null!");
            
            var pipeline = new Pipeline<R, O>(modifier, mlBuilder);
            next = pipeline;
            return pipeline;
        }

        public MLBuilder<T, O> Build()
        {
            var newMlBuilder = new MLBuilder<T, O>(mlBuilder.MLContext);
            mlBuilder.Next = newMlBuilder;
            return newMlBuilder;
        }

        public IPipeline GetNext()
        {
            return next;
        }

        public IModificator GetModificator()
        {
            return modificator;
        }

        /*
        public void PrintPipelineStructure()
        {
            int index = 1;
            foreach (var p in this)
            {
                Console.WriteLine($"{index++} - { p.GetModificator().GetType() }");
            }
            Console.WriteLine("");
        }
        */

        public int GetModifierCount()
        {
            int index = 0;
            foreach (var p in this)
            {
                index++;
            }
            return index;
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
