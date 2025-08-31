using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using eNetwork.Framework;
using GTANetworkAPI;

namespace eNetwork.API.Task
{
    public class TaskManager
    {
        private static readonly Logger _logger = new Logger("task-controller");
        public TaskManager() { }

        public void SetTask(Action action)
        {
            if (Thread.CurrentThread.Name != "Main")
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e) { _logger.WriteError($"SetTask", e); }
            }
            else
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e) { _logger.WriteError($"SetTask Task", e); }
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        public void SetMainTask(Action action)
        {
            if (Thread.CurrentThread.Name != "Main")
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e) { _logger.WriteError($"SetMainTask", e); }
            }
            else
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e) { _logger.WriteError($"SetMainTask Task", e); }
                });
            }
        }

        public void SetAsyncTask(Func<System.Threading.Tasks.Task> action)
        {

            if (Thread.CurrentThread.Name != "Main")
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e) { _logger.WriteError($"SetAsyncTask", e); }
            }
            else
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e) { _logger.WriteError($"SetAsyncTask Task", e); }
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }
    }
}
