using Microsoft.ML;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AIPF.MLManager
{
    public class MLBuilder<I, O> : IMLBuilder where I : class, new() where O : class, new()
    {
        private readonly MLContext mlContext;
        private IAction action;

        public MLContext MLContext => mlContext;

        public IMLBuilder Next { get; set; }

        public MLBuilder(MLContext mlContext = null)
        {
            this.mlContext = mlContext ?? new MLContext();
        }

        public Pipeline<R, O> AddTransformer<R>(IModifier<I, R> modifier) where R : class, new()
        {
            PipelineBuilder<I, O> pipelineBuilder = new PipelineBuilder<I, O>(mlContext, this);
            action = pipelineBuilder;
            return pipelineBuilder.CreatePipeline(modifier);
        }

        public MLBuilder<I, O> AddFilter(Func<I, bool> filterFunction)
        {
            action = new Filter<I>(mlContext, filterFunction);
            var mlBuilder = new MLBuilder<I, O>(mlContext);
            Next = mlBuilder;
            return mlBuilder;
        }

        public MLBuilder<I, O> AddFilter(IFilterAction<I> filter)
        {
            action = filter;
            var mlBuilder = new MLBuilder<I, O>(mlContext);
            Next = mlBuilder;
            return mlBuilder;
        }

        public void Fit(IDataView rawData, out IDataView transformedDataView)
        {
            /*
            transformedDataView = null;
            foreach (var action in Actions)
            {
                action.Execute(rawData, out transformedDataView);
            }*/
            transformedDataView = rawData;
            action?.Execute(rawData, out transformedDataView);
        }

        public object Predict(object toPredict)
        {
            return Predict(toPredict as I);
        }

        public O Predict(I toPredict)
        {
            if (action == null)
            {
                if(Next == null)
                {
                    return toPredict as O;
                }
                return Next.Predict(toPredict) as O;
            }

            object predicted;

            if(action is ITransformerAction<I, O>)
            {
                var a = action as ITransformerAction<I, O>;
                predicted = a.Predict(toPredict);
            }
            else
            {
                var a = action as IFilterAction<I>;
                if (!a.ApplyFilter(toPredict))
                    throw new Exception("The item does not satisfy the filter!");
                predicted = toPredict;
            }

            return Next.Predict(predicted) as O;
        }

        public IEnumerator<IMLBuilder> GetEnumerator()
        {
            IMLBuilder current = this;
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
