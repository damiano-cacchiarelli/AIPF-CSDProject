using Microsoft.ML;
using AIPF.MLManager.Modifiers;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AIPF.MLManager.Actions
{
    public class Pipeline<T, O> : IPipeline where T : class, new() where O : class, new()
    {
        private readonly IMLBuilder mlBuilder;
    
        public IPipeline Next { get; private set; } = null;
        public IModificator Modificator { get; private set; }

        public Pipeline(IModificator modificator, IMLBuilder mlBuilder)
        {
            Modificator = modificator;
            this.mlBuilder = mlBuilder;
        }

        public Pipeline<R, O> Append<R>(IModifier<T, R> modifier) where R : class, new()
        {
            if (modifier == null) 
                throw new Exception("The modificator cannot be null!");
            
            var pipeline = new Pipeline<R, O>(modifier, mlBuilder);
            Next = pipeline;
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
            return Next;
        }

        public List<IModificator> GetModificators()
        {
            var modificators = new List<IModificator>();
            foreach (var p in this)
            {
                modificators.Add(p.Modificator);
            }
            return modificators;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            IEstimator<ITransformer> pipeline = null;
            foreach (var p in this)
            {
                if (pipeline == null) pipeline = p.Modificator.GetPipeline(mlContext);
                else pipeline = pipeline.Append(p.Modificator.GetPipeline(mlContext));
            }
            return pipeline;
        }

        public IEnumerator<IPipeline> GetEnumerator()
        {
            IPipeline current = this;
            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
