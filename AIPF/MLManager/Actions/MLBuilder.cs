using Microsoft.ML;
using System;
using System.Linq.Expressions;

namespace AIPF.MLManager.Actions
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

        public MLBuilder<I, O> AddFilter(Expression<Func<I, bool>> filterFunction)
        {
            return AddFilter(new Filter<I>(filterFunction));
        }

        public MLBuilder<I, O> AddFilter(IFilterAction<I> filter)
        {
            filter.MLContext = mlContext;
            action = filter;
            var mlBuilder = new MLBuilder<I, O>(mlContext);
            Next = mlBuilder;
            return mlBuilder;
        }

        public void Fit(IDataView rawData, out IDataView transformedDataView)
        {
            transformedDataView = rawData;
            action?.Execute(rawData, out transformedDataView);
            if (Next != null)
            {
                Next.Fit(transformedDataView, out transformedDataView);
            }
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
                    throw new Exception($"The filter \"{a}\" is not satified!");
                predicted = toPredict;
            }

            return Next.Predict(predicted) as O;
        }
    }
}
