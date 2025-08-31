using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace eNetwork
{
    public class ENetThread
    {
        public static List<ENetThread> Threads = new List<ENetThread>();
        public static ENetThread GetThread(string name)
        {
            return Threads.Find(x => x.Name == name);
        }

        public string Name { get; set; }
        public Thread Thread { get; set; }
        public ENetThread(string name, ParameterizedThreadStart parameterizedThreadStart, params object[] arguments)
        {
            Name = name; Thread = new Thread(parameterizedThreadStart)
            {
                IsBackground = true
            };
            Threads.Add(this);
        }

        public void Start()
        {
            if (Thread.IsAlive) return;
            Thread.Start();
        } 
    }
}
