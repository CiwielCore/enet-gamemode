using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using eNetwork.Framework;
using eNetwork.API.Functions;
using eNetwork.Framework.API.InteractionDepricated.Data;

namespace eNetwork.Framework.API.Interaction
{
    [Obsolete("InteractionDeprecated is deprecated, please use Interaction instead")]
    public class InteractionManagerDeprecated
    {
        private readonly static Logger _logger = new Logger("interactions");

        private static ConcurrentDictionary<string, InteractionDeprecatedAttributeData> Interactions = new ConcurrentDictionary<string, InteractionDeprecatedAttributeData>();
        public void Initialize()
        {
            var _fastMethodInvoker = new FastMethodInvoker();

            var assembly = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
            var methods = assembly.SelectMany(type => type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
               .Where(m => m.GetCustomAttribute<InteractionDeprecatedAttribute>(false) != null));

            foreach (MethodInfo method in methods)
            {
                var cmdAttribute = method.GetCustomAttribute<InteractionDeprecatedAttribute>();
                if (Interactions.ContainsKey(cmdAttribute.Name))
                {
                    _logger.WriteWarning($"Взаимодействие с колшейпами {cmdAttribute.Name} уже объявлено");
                    continue;
                }
                if (!Interactions.ContainsKey(cmdAttribute.Name))
                {
                    var fastInvokeHandler = _fastMethodInvoker.GetMethodInvoker(method);
                    var methodData = new InteractionDeprecatedAttributeData(method, fastInvokeHandler);
                    if (AddInstanceIfRequired(method, methodData))
                        Interactions.TryAdd(cmdAttribute.Name, methodData);
                }
            }

            _logger.WriteInfo($"Загружено {Interactions.Count} взаимодействий с колшейпами");
        }
        private bool AddInstanceIfRequired(MethodInfo method, InteractionDeprecatedAttributeData methodData)
        {
            if (method.IsStatic) return true;

            var instance = GetMethodInstance(method);
            if (instance is null) return false;
            methodData.Instance = instance;
            return true;
        }

        private readonly Dictionary<Type, object> _instancesPerClass = new Dictionary<Type, object>();
        private object GetMethodInstance(MethodInfo method)
        {
            var classType = method.DeclaringType;
            if (_instancesPerClass.TryGetValue(classType, out var instance))
                return instance;

            instance = Activator.CreateInstance(classType);
            if (instance is null) return null;
            _instancesPerClass[classType] = instance;
            return instance;
        }

        public static void Call(string name, params object[] parameters)
        {
            try
            {
                if (!Interactions.ContainsKey(name)) return;
                InteractionDeprecatedAttributeData methodData = Interactions[name];

                if (methodData.FastInvokeHandler is FastInvokeHandler nonStaticHandler)
                    nonStaticHandler.Invoke(methodData.Instance, parameters);
                else if (methodData.FastInvokeHandler is FastInvokeHandlerStatic staticHandler)
                    staticHandler.Invoke(parameters);
            }
            catch (Exception e) { _logger.WriteError("Call", e); }
        }
    }
}
