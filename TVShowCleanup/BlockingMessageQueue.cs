using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TVShowCleanup
{
    public class BlockingQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly Semaphore _signal;

        public BlockingQueue(Queue<T> queue)
        {
            _queue = queue;
            _signal = new Semaphore(0, int.MaxValue);
        }

        public void Enqueue(T message)
        {
            lock (_queue)
            {
                CleanupLog.WriteMethod(message.ToString());
                _queue.Enqueue(message);
            }
            _signal.Release();
        }

        public T Dequeue()
        {
            _signal.WaitOne();
            T message;
            lock (_queue)
            {
                if (_queue.Any())
                {
                    message = _queue.Dequeue();

                    CleanupLog.WriteMethod(message.ToString());

                    return message;
                }
            }
            return default(T);
        }

        internal bool HasItem(T message)
        {
            lock (_queue)
            {
                return _queue.Contains(message);
            }
        }

        internal bool HasItems()
        {
            lock (_queue)
            {
                return _queue.Any();
            }
        }
    }
}
