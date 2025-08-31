using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using eNetwork.Framework;
using System.Reflection;
using System.Collections.Concurrent;
using eNetwork.API.Functions;
using eNetwork.Framework.API.CustomEvents.Methods;
using eNetwork.Framework.API.CustomEvents.Data;

namespace eNetwork.Framework.API.CustomEvents
{
    public class CustomEventManager : Script
    {
        private static readonly Logger _logger = new Logger("event-manager");

        private static ConcurrentDictionary<string, EventParser> Events = new ConcurrentDictionary<string, EventParser>();

        private const string eventCallback = "kuertov_lox";
        private static string secretCode = "";

        public void Initialize()
        {
            try {
                secretCode = GenerateSecretCode();

                var assembly = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
                var methods = assembly.SelectMany(type => type
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                   .Where(m => m.GetCustomAttribute<CustomEventAttribute>(false) != null));


                foreach (MethodInfo method in methods)
                {
                    var cmdAttribute = method.GetCustomAttribute<CustomEventAttribute>();
                    if (Events.ContainsKey(cmdAttribute.EventName))
                    {
                        _logger.WriteWarning($"Ивент {cmdAttribute.EventName} уже объявлен");
                        continue;
                    }
                    if (cmdAttribute is CustomEventAttribute && !Events.ContainsKey(cmdAttribute.EventName))
                    {
                        var fastInvokeHandler = new FastMethodInvoker().GetMethodInvoker(method);
                        var methodData = new CustomEventAttributeData(method, fastInvokeHandler);

                        if (AddInstanceIfRequired(method, methodData))
                        {
                            EventParser eventParser = new EventParser()
                            {
                                MethodData = methodData,
                                Parameters = method.GetParameters(),
                            };
                            Events.TryAdd(cmdAttribute.EventName, eventParser);
                        }
                    }
                }

                _logger.WriteInfo($"Загруженно {Events.Count} ивентов");
                _logger.WriteWarning($"Секретный ключ шифрации: {secretCode}");
            }
            catch(Exception ex) { _logger.WriteError("EventHandler", ex); }
        }
        private bool AddInstanceIfRequired(MethodInfo method, CustomEventAttributeData methodData)
        {
            if (method.IsStatic) return true;

            var instance = GetMethodInstance(method);
            if (instance is null) return false;
            methodData.Instance = instance;
            return true;
        }
        private string GenerateSecretCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[12];

            for (int i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[new Random().Next(chars.Length)];

            var finalString = new String(stringChars);
            return finalString;
        }

        private bool IsValidEvent(string clientCode) => secretCode == clientCode;
        public string GetSecretCode() => secretCode;
        public string GetCallbackCode() => eventCallback;

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

        [RemoteEvent(eventCallback)]
        public void CallLocal(ENetPlayer player, params object[] arguments)
        {
            try
            {
                string eventName = arguments[0].ToString();
                string clientCode = arguments[1].ToString();

                if (!IsValidEvent(clientCode))
                {
                    _logger.WriteError($"Detected invalid code: {player.Name} ({player.Value})");
                    player.ExtKick("Прервано соединение с сервером");
                    return;
                }
                if (!Events.TryGetValue(eventName, out EventParser parser)) return;

                object[] array = new object[arguments.Length - 1];
                for(int i = 2; i < arguments.Length; i++)
                    array[i-2] = arguments[i];

                parser.Execute(player, array);
            }
            catch (Exception ex) { _logger.WriteError("CallLocal", ex); }
        }
    }
}
