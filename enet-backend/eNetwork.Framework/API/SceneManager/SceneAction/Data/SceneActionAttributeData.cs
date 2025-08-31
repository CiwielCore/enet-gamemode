using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.SceneManager.SceneAction.Data
{
    public class SceneActionAttributeData
    {
        public MethodInfo Method { get; }
        public Delegate FastInvokeHandler { get; }
        public object Instance { get; internal set; }
        public SceneActionAttributeData(MethodInfo method, Delegate fastInvokeHandler)
        {
            Method = method;
            FastInvokeHandler = fastInvokeHandler;
        }
    }
}
