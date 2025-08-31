using eNetwork.API.Functions;
using eNetwork.Framework.API.SceneManager.SceneAction.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.SceneManager.SceneAction
{
    public class SceneActionManager
    {
        private readonly static Logger _logger = new Logger("sceneactions");

        private static ConcurrentDictionary<SceneActionType, SceneActionAttributeData> Actions = new ConcurrentDictionary<SceneActionType, SceneActionAttributeData>();
        public void Initialize()
        {
            var _fastMethodInvoker = new FastMethodInvoker();

            var assembly = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
            var methods = assembly.SelectMany(type => type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
               .Where(m => m.GetCustomAttribute<SceneActionAttribute>(false) != null));


            foreach (MethodInfo method in methods)
            {
                var cmdAttribute = method.GetCustomAttribute<SceneActionAttribute>();
                if (Actions.ContainsKey(cmdAttribute.Type))
                {
                    _logger.WriteWarning($"Действие сцен {cmdAttribute.Type} уже объявлено");
                    continue;
                }
                if (!Actions.ContainsKey(cmdAttribute.Type))
                {
                    var fastInvokeHandler = _fastMethodInvoker.GetMethodInvoker(method);
                    var methodData = new SceneActionAttributeData(method, fastInvokeHandler);
                    if (AddInstanceIfRequired(method, methodData))
                        Actions.TryAdd(cmdAttribute.Type, methodData);
                }
            }

            _logger.WriteInfo($"Загружено {Actions.Count} действий сцен");
        }
        private bool AddInstanceIfRequired(MethodInfo method, SceneActionAttributeData methodData)
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
        public static void Call(SceneActionType type, params object[] parameters)
        {
            try
            {
                if (!Actions.ContainsKey(type)) return;
                SceneActionAttributeData methodData = Actions[type];

                if (methodData.FastInvokeHandler is FastInvokeHandler nonStaticHandler)
                    nonStaticHandler.Invoke(methodData.Instance, parameters);
                else if (methodData.FastInvokeHandler is FastInvokeHandlerStatic staticHandler)
                    staticHandler.Invoke(parameters);
            }
            catch (Exception ex) { _logger.WriteError($"Call", ex); }
        }
    }
}
