using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TSJ.Gemini.Slack
{

    /**
     * not the cleanest implementation ever of a way to have thread safety around a future action
     * whose execution time may change.  Thread safety is dependent on proper use of the locking
     * around the given mutex from outside the object.  Lock around creation, be sure to invalidate references
     * on "removed" being called, and otherwise check the "Dead" property before adjusting the timeout
     * **/
    public class IdleTimeoutExecutor
    {

        private DateTime _timeout;
        private object _mutex;

        public DateTime Timeout
        {
            get { return _timeout; }
            set
            {
                lock (_mutex)
                {
                    if (Dead) throw new Exception("IdleTimeoutExecutor already fired");
                    _timeout = value;
                    //alert the waiting thread
                    Monitor.PulseAll(_mutex);
                }
            }
        }

        public bool Dead
        {
            get;
            private set;
        }

        public IdleTimeoutExecutor(DateTime timeout, Action onTimeoutExpired, object mutex, Action onFinish)
        {
            DateTime created = DateTime.Now;
            if (DateTime.Now > timeout)
            {
                onTimeoutExpired();
            }
            else
            {
                _timeout = timeout;
                _mutex = mutex;
                new Thread(() =>
                {
                    lock (_mutex)
                    {
                        while (_timeout > DateTime.Now)
                        {
                            Monitor.Wait(_mutex, _timeout - DateTime.Now);
                        }
                        try
                        {
                            onTimeoutExpired();
                        }
                        catch { }
                        onFinish();
                        Dead = true;
                    }
                }).Start();
            }
        }

    }
}
