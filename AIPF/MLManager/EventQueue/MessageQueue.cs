using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AIPF.MLManager.EventQueue
{
	public class MessageManager
    {
		public static IMessageQueue<double> IMessageQueue { get; private set; } = new MessageQueue<double>();
    }
    public class MessageQueue<T> : IMessageQueue<T>
    {
		private ConcurrentDictionary<string, Channel<T>> clientToChannelMap;
	
		internal MessageQueue()
		{
			clientToChannelMap = new ConcurrentDictionary<string, Channel<T>>();
		}

		public IAsyncEnumerable<T> DequeueAsync(string id, CancellationToken cancelToken)
		{
			if (clientToChannelMap.TryGetValue(id, out Channel<T> channel))
			{

				return channel.Reader.ReadAllAsync(cancelToken);
			}
			else
			{
				throw new ArgumentException($"Id {id} isn't registered");
			}
		}

		public async Task EnqueueAsync(string id, T message, CancellationToken cancelToken)
		{
			if (clientToChannelMap.TryGetValue(id, out Channel<T> channel))
			{
				await channel.Writer.WriteAsync(message, cancelToken);
			}
		}

		public void Register(string id)
		{
			if (!clientToChannelMap.TryAdd(id, Channel.CreateUnbounded<T>()))
			{
				throw new ArgumentException($"Id {id} is already registered");
			}
		}

		public void Unregister(string id)
		{
			clientToChannelMap.TryRemove(id, out _);
		}

		private Channel<T> CreateChannel()
		{
			return Channel.CreateUnbounded<T>();
		}
    }
}