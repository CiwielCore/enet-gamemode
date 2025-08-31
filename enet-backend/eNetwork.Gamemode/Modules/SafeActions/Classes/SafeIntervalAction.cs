using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace eNetwork.Modules.SafeActions.Classes
{
    public class SafeIntervalAction
    {
        private static readonly Logger Logger = new Logger("safe-interval");
        public string ThreadName { get; set; }
        public int IntervalTime { get; set; }
        public bool AtOnce { get; set; } = true;

        private ENetThread Thread { get; set; }
        public void Initialize()
        {
            try
            {
                if (!AtOnce)
                    _lastTime = DateTime.Now.AddSeconds(IntervalTime);

                Thread = new ENetThread(ThreadName, new ParameterizedThreadStart(Worker));
                Thread.Start();
            }
            catch(Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public virtual void Action(object obj) { }

        private DateTime _lastTime = DateTime.Now;
        private void Worker(object obj)
        {
            while(true)
            {
                if (_lastTime < DateTime.Now)
                {
                    try
                    {
                        _lastTime = DateTime.Now.AddSeconds(IntervalTime);
                        Action(obj);
                    }
                    catch (Exception ex) { Logger.WriteError("Worker", ex); }
                }
            }
        }
    }
}
