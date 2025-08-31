using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using eNetwork.Factions;
using eNetwork.Framework;
using eNetwork.Modules.SafeActions.Classes;
using eNetwork.Services.VipServices;
using eNetwork.Services.VipServices.VipAddons;
using eNetwork.World.Hunting;
using GTANetworkAPI;

namespace eNetwork.Modules.SafeActions.Actions
{
    public class EveryMinute : SafeIntervalAction
    {
        private static readonly Logger Logger = new Logger("every-minute");

        public EveryMinute()
        {
            ThreadName = "EveryMinute";
            IntervalTime = 60;
        }

        public override void Action(object obj)
        {
            try
            {
                foreach (var player in ENet.Pools.GetAllRegisteredPlayers())
                {
                    var data = player.GetCharacter();
                    if (DateTime.Now.Date != data.Stats.LastTimeUpdate.Date)
                        data.Stats.TimePlaying = 0;

                    data.Stats.TimePlaying++;
                    data.Stats.AllTimePlaying++;

                    data.Indicators.Interval(player);

                    if (data.Stats.AllTimePlaying % 60 == 0)
                    {
                        // User PayDay
                        int paydayAmount = 100;
                        var rank = player.CharacterData.Faction.Ranks.FirstOrDefault(el=>el.Lvl == player.CharacterData.FactionRank);
                        if (rank != null)
                        {
                            paydayAmount += rank.PayDay;
                        }

                        Vip vip = VipService.Instance.GetVipOfPlayer(player);
                        if (vip is IHandlerOfUnemploymentBenefits vipPaydayHandler)
                        {
                            paydayAmount += vipPaydayHandler.AmountOfUnemploymentBenefits;
                        }

                        if (player.ChangeWallet(paydayAmount))
                        {
                            ChatHandler.SendMessage(player, $"Вы получили <span style=\"color: {Helper.DollarColor}\">{Helper.FormatPrice(paydayAmount)}$</span> за Pay Day", new ChatAddition(ChatType.System));
                        }

                        Game.Player.Leveling.Exp.Up(player);

                        Services.Promocodes.PromocodeService.Instance.TryAddPlayedHourForPlayerPromocodeinfo(player);
                        Services.BonusServices.DailyBonus.Instance.TriggerEveryMinuteEvent(player);
                    }

                    Demorgan.DemorganManager.Instance.UpdateEveryMinute(player);
                    Mute.MuteManager.UpdatingTheTimeOfMute(player);
                }

                if (DateTime.Now.Minute % 10 == 0)
                {
                    HuntingHandler.CheckAnimals();
                }

                Game.HiddingBox.HiddingBoxManager.Worker();

                if (DateTime.Now.Minute == 0) // Global PayDay
                {
                    ChatHandler.SendMessageToAll($"Текущее время {DateTime.Now.Hour}:00!");
                }
            }
            catch (Exception ex) { Logger.WriteError("Worker", ex); }
        }
    }
}