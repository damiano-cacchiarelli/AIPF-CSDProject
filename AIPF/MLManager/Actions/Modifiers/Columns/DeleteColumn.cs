using Microsoft.ML;

namespace AIPF.MLManager.Modifiers.Columns
{
    public class DeleteColumn<I> : IModifier<I, I> where I : class, new()
    {
        private readonly string[] columnNames;

        public DeleteColumn(params string[] columnNames)
        {
            this.columnNames = columnNames;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.DropColumns(columnNames);
        }
    }
}
