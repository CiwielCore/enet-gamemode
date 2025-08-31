using System;
using System.Reflection;

namespace eNetwork.Framework.Singleton
{
    public class Singleton<T> where T : class
    {
        private static volatile T _instance;
        private readonly static object _syncLocker = new();

        private static T CreateInstance()
        {
            ConstructorInfo cInfo = typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                new ParameterModifier[0]);

            return (T)cInfo?.Invoke(Array.Empty<object>());
        }

        public static T Instance
        {
            get
            {
                lock (_syncLocker)
                {
                    _instance ??= CreateInstance();
                    return _instance;
                }
            }
        }
    }
}
