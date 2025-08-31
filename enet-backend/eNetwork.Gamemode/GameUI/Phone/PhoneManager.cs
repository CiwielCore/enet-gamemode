using eNetwork.Framework;
using eNetwork.Game;
using eNetwork.Inv;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.GameUI.Phone
{
    public class PhoneManager
    {
        private static readonly Logger Logger = new Logger("phone-manager");

        [CustomEvent("server.phone.open")]
        public static void OpenPhone(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData) || (player.HasSharedData("phone.opened") && player.GetSharedData<bool>("phone.opened") == true)) return;

                if (!IsHavePhone(player))
                {
                    player.SendError("У вас нет телефона, купите его в ближайшем магазине 24/7");
                    return;
                }

                if (player.GetItemHand() != ItemId.Debug)
                {
                    player.SendError("Сначала уберите предмет из рук!");
                    return;
                }

                player.SetItemHand(ItemId.Phone);
                ClientEvent.Event(player, "client.phone.open", 999);

                player.PlayScenario(ScenarioType.TakePhone);
                player.AddAttachment("phone");

                player.SetSharedData("phone.opened", true);
            }
            catch(Exception e) { Logger.WriteError("Event_Open", e); }
        }

        [CustomEvent("server.phone.close")]
        public static void ClosePhone(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData) || !player.HasSharedData("phone.opened")) return;

                player.RemoveItemHand();
                ClientEvent.Event(player, "client.phone.close");

                player.SmoothResetAnim();
                player.StopScenario();

                player.AddAttachment("phone", true);

                player.SetSharedData("phone.opened", false);
            }
            catch(Exception ex) { Logger.WriteError("Event_Close", ex); }
        }

        public static bool IsHavePhone(ENetPlayer player)
        {
            try
            {
                var phone = Inventory.FindItem(player, ItemId.Phone);
                if (phone is null) return false;
                
                return true;
            }
            catch(Exception ex) { Logger.WriteError("IsHavePhone", ex); return false; }
        }

        public static void MoveFinger(ENetPlayer player, FingerType fingerType)
        {
            try
            {
                ClientEvent.EventInRange(player.Position, 50f, "client.phone.moveFinger", (int)fingerType);
            }
            catch(Exception ex) { Logger.WriteError("MoveFinger", ex); }
        }

        public enum FingerType
        {
            SwipeUp = 1,
            SwipeDown = 2,
            SwipeLeft = 3,
            SwipeRight = 4,
            ButtonPress = 5
        }
    }
}
