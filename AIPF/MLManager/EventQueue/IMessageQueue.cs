using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIPF.MLManager.EventQueue
{
    public interface IMessageQueue<T>
    {
        void Register(string id);
        void Unregister(string id);
        IAsyncEnumerable<T> DequeueAsync(string id, CancellationToken cancelToken);
        Task EnqueueAsync(string id, T message, CancellationToken cancelToken);
    }
}