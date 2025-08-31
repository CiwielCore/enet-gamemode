using eNetwork.Framework;
using eNetwork.Jobs.Fishing.Store;
using eNetwork;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.Framework.API.InteractionDepricated.Data;

namespace eNetwork.Jobs.Fishing
{
    public class Controller
    {
        private static readonly Logger Logger = new Logger("fishing-controller");

        [InteractionDeprecated(ColShapeType.FishMarket)]
        public static void OnInteraction(ENetPlayer player)
        {
            try
            {
                if (!player.GetData<FishingMarket>("fish.store", out var store)) return;
                store.Interaction(player);
            }
            catch (Exception ex) { Logger.WriteError("OnInteraction: " + ex.ToString()); }
        }

        [CustomEvent("server.fishing.change_state")]
        public void ChangeFishingState(ENetPlayer player, bool state)
        {
            try
            {
                if (!player.GetSessionData(out var sessionData) || sessionData.WorkData.FishingWorkData.CanDo == state) return;
                sessionData.WorkData.FishingWorkData.CanDo = state;
            }
            catch (Exception ex) { Logger.WriteError("ChangeFishingState", ex); }
        }

        [CustomEvent("server.fishing.minigame.end")]
        public void OnMinigameEnd(ENetPlayer player, bool state) 
            => FishingManager.EndMinigame(player, state);

        [CustomEvent("server.fishing.store.action")]
        public void OnStore(ENetPlayer player, string category, int index, int value)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetData<FishingMarket>("fish.store", out var store)) return;
                store.Action(player, category, index, value);
            }
            catch (Exception ex) { Logger.WriteError("OnStore", ex); }
        }
    }
}
