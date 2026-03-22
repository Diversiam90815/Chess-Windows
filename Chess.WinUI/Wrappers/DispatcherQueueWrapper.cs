using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.UI.Wrappers
{
    public interface IDispatcherQueueWrapper
    {
        bool TryEnqueue(DispatcherQueueHandler callback);
        bool TryDequeue(DispatcherQueuePriority priority, DispatcherQueueHandler callback);
        Task EnqueueAsync(Action callback);
    }


    public class DispatcherQueueWrapper : IDispatcherQueueWrapper
    {
        private readonly DispatcherQueue _dispatcherQueue;

        public DispatcherQueueWrapper()
        {
            // Must run on main thread!
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }


        public bool TryDequeue(DispatcherQueuePriority priority, DispatcherQueueHandler callback)
        {
            return _dispatcherQueue.TryEnqueue(priority, callback);
        }

        public bool TryEnqueue(DispatcherQueueHandler callback)
        {
            return _dispatcherQueue.TryEnqueue(callback);
        }

        public Task EnqueueAsync(Action callback)
        {
            var tcs = new TaskCompletionSource<bool>();

            _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    callback();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
