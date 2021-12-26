using Microsoft.ML;

namespace AIPF.MLManager.Actions
{
    public interface IFilterAction<I> : IAction where I : class, new()
    {
        public MLContext MLContext { set; }

        bool ApplyFilter(I item);
    }
}
