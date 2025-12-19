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
    }
}
