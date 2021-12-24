namespace AIPF.MLManager
{
    public interface IFilterAction<I> : IAction where I : class, new()
    {
        bool ApplyFilter(I item);
    }
}
