using System;

namespace eNetwork
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomEventAttribute : Attribute
    {
        public string EventName { get; set; }
        public CustomEventAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}
