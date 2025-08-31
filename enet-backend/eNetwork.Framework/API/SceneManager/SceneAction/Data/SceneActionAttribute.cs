using System;

namespace eNetwork.Framework.API.SceneManager.SceneAction
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SceneActionAttribute : Attribute
    {
        public SceneActionType Type;

        public SceneActionAttribute(SceneActionType type)
        {
            Type = type;
        }
    }
}
