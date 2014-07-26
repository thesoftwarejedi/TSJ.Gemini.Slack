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

        //dangerous, always lock in this order
        //this is done for thread safety outside of the object
        //on the mutex given, yet also on a private mutex to
        //use for notification - we don't want to reuse the provided
        //mutex for internal notification purposes
        private object _mutex;
        private object _privateMutex = new object();

        public DateTime Timeout
        {
            get { return _timeout; }
            set
            {
                lock (_mutex) 
                lock (_privateMutex)
                {
                    if (Dead) throw new Exception("IdleTimeoutExecutor already fired");
                    _timeout = value;
                    //alert the waiting thread
                    Monitor.PulseAll(_privateMutex);
                }
            }
        }

        public bool Dead
        {
            get;
            private set;
        }

        public IdleTimeoutExecutor(DateTime timeout, Action onTimeoutExpired, Action cleanup, object mutex = null)
        {
            DateTime created = DateTime.Now;
            if (DateTime.Now > timeout)
            {
                onTimeoutExpired();
            }
            else
            {
                _timeout = timeout;
                _mutex = mutex ?? new object();
                new Thread(() =>
                {
                    lock (_mutex)
                    lock (_privateMutex)
                    {
                        while (_timeout > DateTime.Now)
                        {
                            //release the lock while we wait for timeout or notification
                            Monitor.Wait(_privateMutex, _timeout - DateTime.Now);
                        }
                        try
                        {
                            onTimeoutExpired();
                        }
                        catch { } //just let it go, nothing we can do
                        cleanup();
                        Dead = true;
                    }
                }).Start();
            }
        }

    }
}
