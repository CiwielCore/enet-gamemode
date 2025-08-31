using System;
using System.Collections.Generic;
using System.Linq;
using eNetwork.Game;
using eNetwork.Inv;
using GTANetworkAPI;

namespace eNetwork.Gambles.Lotteries
{
    public class LotteryEvents : Script
    {
        [CustomEvent("server.lottery.take_prize")]
        public static void TakePrizeEventHandler(ENetPlayer player, string prizeType, string prizeValue)
        {
            if (player.CharacterData == null)
                return;

            if (!Enum.TryParse(prizeType, out LotteryPrizeType _))
                return;
            
            if (!player.GetInventory(out Storage storage))
                return;

            Item prizeItem = storage.Items.FirstOrDefault(i => i.Type == ItemId.LotteryTicket && i.Data == prizeValue);
            if (prizeItem == null)
            {
                player.SendError($"У вас нет лотерейного билета с призом {prizeValue}");
                return;
            }

            Lottery.Instance.GivePrize(player, prizeItem);
        }
    }
}
