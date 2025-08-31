using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;
using System.Drawing;

namespace eNetwork.Framework
{
    public class Logger
    {
        private string Type { get; set; }

        public Logger(string type) { Type = type; }

        public void WriteError(string message, Exception exception)
        {
            try
            {
                WriteError($"{message}: {exception.ToString()}");
            }
            catch { }
        }

        public void WriteError(string message)
        {
            try
            {
                Console.Write($"[{Type}] ", Color.Red);
                Console.Write($"{message}\n", Color.WhiteSmoke);
            }
            catch { }
        } 
        
        public void WriteWarning(string message)
        {
            try
            {
                Console.Write($"[{Type}] ", Color.DarkOrange);
                Console.Write($"{message}\n", Color.WhiteSmoke);
            }
            catch { }
        }

        public void WriteDone(string message)
        {
            try
            {
                Console.Write($"[{Type}] ", Color.GreenYellow);
                Console.Write($"{message}\n", Color.WhiteSmoke);
            }
            catch { }
        }

        public void WriteInfo(string message)
        {
            try
            {
                Console.Write($"[{Type}] ", Color.Cyan);
                Console.Write($"{message}\n", Color.WhiteSmoke);
            }
            catch { }
        }

        public void WriteSave(string message)
        {
            try
            {
                Console.Write($"[{Type}] ", Color.Aqua);
                Console.Write($"{message}\n", Color.WhiteSmoke);
            }
            catch { }
        }

        public void WriteDebug(string message)
        {
            try
            {
                return;
                #if DEBUG
                Console.Write($"[{Type}] ", Color.Gray);
                Console.Write($"{message}\n", Color.WhiteSmoke);
                #endif
            }
            catch { }
        }
    }
}
