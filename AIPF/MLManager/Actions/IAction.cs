using Microsoft.ML;

namespace AIPF.MLManager.Actions
{
    public interface IAction
    {
        void Execute(IDataView dataView, out IDataView trasformedDataView);
    }
}
