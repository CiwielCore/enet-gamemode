using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.ChatCommand.Data
{
    public class ChatCommandAttributeData
    {
        public MethodInfo Method { get; }
        public Delegate FastInvokeHandler { get; }
        public object Instance { get; internal set; }
        public ChatCommandAttributeData(MethodInfo method, Delegate fastInvokeHandler)
        {
            Method = method;
            FastInvokeHandler = fastInvokeHandler;
        }
    }
}
