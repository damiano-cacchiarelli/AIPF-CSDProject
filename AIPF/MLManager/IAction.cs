using Microsoft.ML;

namespace AIPF.MLManager
{
    public interface IAction
    {
        void Execute(IDataView dataView, out IDataView trasformedDataView);
    }
}
