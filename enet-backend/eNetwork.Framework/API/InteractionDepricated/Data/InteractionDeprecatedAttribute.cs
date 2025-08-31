using System;

namespace eNetwork.Framework.API.InteractionDepricated.Data
{
    [Obsolete("InteractionDeprecated is deprecated, please use Interaction instead")]
    [AttributeUsage(AttributeTargets.Method)]
    public class InteractionDeprecatedAttribute : Attribute
    {
        public ColShapeType Type;
        public InteractionType Interaction;
        public string Name;

        public InteractionDeprecatedAttribute(ColShapeType type, InteractionType interact = InteractionType.Key)
        {
            switch (interact)
            {
                case InteractionType.Key:
                    this.Name = type.ToString();
                    return;
                case InteractionType.Enter:
                    this.Name = "ENTER-" + type.ToString();
                    return;
                case InteractionType.Leave:
                    this.Name = "LEAVE-" + type.ToString();
                    return;
            }
        }
    }

    public enum InteractionType
    {
        Key, Enter, Leave
    }
}