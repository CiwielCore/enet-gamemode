using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.InteractionDepricated.Data
{
    public class InteractionDeprecatedAttributeData
    {
        public MethodInfo Method { get; }
        public Delegate FastInvokeHandler { get; }
        public object Instance { get; internal set; }
        public InteractionDeprecatedAttributeData(MethodInfo method, Delegate fastInvokeHandler)
        {
            Method = method;
            FastInvokeHandler = fastInvokeHandler;
        }
    }
}