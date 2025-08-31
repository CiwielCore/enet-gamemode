using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Game.HiddingBox.Classes;
using eNetwork.Game.HiddingBox.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.HiddingBox
{
    public class HiddingBoxManager
    {
        private static readonly Logger Logger = new Logger(nameof(HiddingBoxManager));
        public static ConcurrentDictionary<int, HiddenBox> Boxes = new ConcurrentDictionary<int, HiddenBox>();

        public static void Initialize()
        {
            foreach (var item in Config.HIDDEN_BOXES)
            {
                var box = item.Clone();
                box.Id = Boxes.Count + 1;

                Boxes.TryAdd(box.Id, box);

                box.GTAElements();
            }
        }

        public static void Worker()
        {
            foreach(var box in Boxes.Values)
            {
                box.Worker();
            }
        }

        [InteractionDeprecated(ColShapeType.HiddenBox)]
        public static void OnInteraction(ENetPlayer player)
        {
            try
            {
                if (!player.GetData<HiddenBox>("hiddenBox", out var hiddenBox)) return;
                hiddenBox.Interaction(player);
            }
            catch(Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }

        [ChatCommand("hiddenboxupdate", Description = "Обновить лут в тайнике", Access = PlayerRank.Owner, Arguments = "[id]")]
        public static void Command(ENetPlayer player, int id)
        {
            try
            {
                if (!Boxes.TryGetValue(id, out var box)) return;
                box.Cooldown = DateTime.Now;
                box.Worker();
            }
            catch(Exception ex) { Logger.WriteError("Command", ex); }
        }

        public static HiddenBox GetHiddenBox(int id)
        {
            Boxes.TryGetValue(id, out var box);
            return box;
        }
    }
}
