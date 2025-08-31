using GTANetworkAPI;
using System;

namespace eNetwork
{
    public class ENetColShape : ColShape
    {
        public ENetColShape(NetHandle handle) : base(handle) { }
        [Obsolete("This field is deprecated, please use Interaction instead")]
        public ColShapeType Interaction;

        [Obsolete("This field is deprecated, please use Interaction instead")]
        public string InteractionText;

        [Obsolete("This Method is deprecated, please use Interaction instead")]
        public void SetIntraction(ColShapeType interaction)
        {
            Interaction = interaction;
        }

        [Obsolete("This Method is deprecated, please use Interaction instead")]
        public void SetInteractionText(string text)
        {
            InteractionText = text;
        }

        [Obsolete("This field is deprecated, please use Interaction instead")]
        public Func<ENetColShape, ENetPlayer, bool> Predicate { get; set; } = null;

        [Obsolete("This Method is deprecated, please use Interaction instead")]
        public void AddPredicate(Func<ENetColShape, ENetPlayer, bool> predicate)
            => Predicate = predicate;
    }
}
