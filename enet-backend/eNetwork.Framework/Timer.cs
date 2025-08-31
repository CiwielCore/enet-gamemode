using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eNetwork.Framework
{
    public static class Timers
    {
        public static Dictionary<string, eTimer> timers = new Dictionary<string, eTimer>();
        private static readonly Logger _logger = new Logger("Timers");
        
        private static Thread thread;

        private static int delay;
        private static int clearDelay;

        public static void Initialize()
        {
            delay = 100;
            clearDelay = 300000;

            thread = new Thread(Logic);
            thread.IsBackground = true;
            thread.Name = "ENetTimer";
            thread.Start();

            Start(clearDelay, () =>
            {
                lock (timers)
                {
                    List<eTimer> timers_ = new List<eTimer>(Timers.timers.Values);
                    foreach (eTimer t in timers_)
                    {
                        if (t.isFinished) Timers.timers.Remove(t.ID);
                    }
                }
            });
        }
        private static void Logic()
        {
            while (true)
            {
                try
                {
                    if (timers.Count < 1) continue;

                    List<eTimer> timers_ = new List<eTimer>(timers.Values);

                    foreach (eTimer timer in timers_)
                    {
                        timer.Elapsed();
                    }
                    Thread.Sleep(delay);

                }
                catch (Exception e) { _logger.WriteError("Logic", e); }
            }
        }

        public static eTimer Get(string id)
        {
            if (timers.ContainsKey(id))
                return timers[id];
            return null;
        }

        public static string Start(int interval, Action action, bool InMainThread = false)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new eTimer(action, id, interval, inMainThread: InMainThread));
                return id;
            }
            catch (Exception e) { _logger.WriteError("Start", e); return null; }
        }

        public static string Start(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new eTimer(action, id, interval));
                return id;
            }
            catch (Exception e) { _logger.WriteError("Start", e); return null; }
        }

        public static string StartOnce(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new eTimer(action, id, interval, true));
                return id;
            }
            catch (Exception e) { _logger.WriteError("StartOnce", e); return null; }
        }

        public static string StartOnce(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new eTimer(action, id, interval, true));
                return id;
            }
            catch (Exception e) { _logger.WriteError("StartOnve", e); return null; }
        }

        public static string StartTask(int interval, Action action, bool InMainThread = false)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new eTimer(action, id, interval, false, true, InMainThread));
                return id;
            }
            catch (Exception e) { _logger.WriteError("StartTask", e); return null; }
        }

        public static string StartTask(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new eTimer(action, id, interval, false, true));
                return id;
            }
            catch (Exception e) { _logger.WriteError("StartTask", e); return null; }
        }

        public static string StartOnceTask(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new eTimer(action, id, interval, true, true));
                return id;
            }
            catch (Exception e) { _logger.WriteError("StartTaskOnce", e); return null; }
        }

        public static string StartOnceTask(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new eTimer(action, id, interval, true, true));
                return id;
            }
            catch (Exception e) { _logger.WriteError("StartTaskOnce", e); return null; }
        }

        public static void Stop(string id)
        {
            if (id is null) throw new Exception("Trying to stop timer with NULL ID");
            if (timers.ContainsKey(id))
            {
                timers[id].isFinished = true;
                timers.Remove(id);
            }
        }

        public static void Stats()
        {
            string timers_ = "";
            foreach (eTimer t in timers.Values)
            {
                string state = (t.isFinished) ? "stopped" : "active";
                timers_ += $"{t.ID}:{state} ";
            }

            _logger.WriteDebug(
                $"\nThread State = {thread.ThreadState.ToString()}" +
                $"\nTimers Count = {timers.Count}" +
                $"\nTimers = {timers_}" +
                $"\n");
        }
        public class eTimer
        {
            public string ID { get; }
            public int MS { get; set; }
            public DateTime Next { get; private set; }

            public Action action { get; set; }

            public bool isOnce { get; set; }
            public bool isTask { get; set; }
            public bool isFinished { get; set; }

            public bool InMainThread { get; set; }

            public eTimer(Action action_, string id_, int ms_, bool isonce_ = false, bool istask_ = false, bool inMainThread = false)
            {
                action = action_;

                ID = id_;
                MS = ms_;
                Next = DateTime.Now.AddMilliseconds(MS);

                isOnce = isonce_;
                isTask = istask_;
                isFinished = false;
                InMainThread = inMainThread;
            }

            public void Elapsed()
            {
                try
                {
                    if (isFinished) return;

                    if (Next <= DateTime.Now)
                    {
                        if (isOnce) isFinished = true;
                        Next = DateTime.Now.AddMilliseconds(MS);

                        _logger.WriteDebug($"Timer.Elapsed.{ID}.Invoke");

                        if (isTask) Task.Run(() => action.Invoke());
                        else
                        {
                            if (!InMainThread)
                            {
                                action.Invoke();
                            }
                            else
                            {
                                NAPI.Task.Run(() => action.Invoke());
                            }
                        }

                        _logger.WriteDebug($"Timer.Elapsed.{ID}.Completed");
                    }

                }
                catch (Exception e) { _logger.WriteError($"Timer.Elapsed.{ID}", e); }
            }
        }
    }
}
