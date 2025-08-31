using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using eNetwork.Game;
using eNetwork.GameUI;
using eNetwork.Modules;
using eNetwork.Framework;
using Newtonsoft.Json;
using eNetwork.Services.Logging.LogMessages;
using eNetwork.Services.Logging;

namespace eNetwork.Commands
{
    public class PlayerCommands
    {
        private static readonly Logger Logger = new Logger("PlayerCommands");

        [ChatCommand("tpc", Arguments = "[x] [y] [z]", Description = "Телепортироваться по координатам", Access = PlayerRank.Admin)]
        public static void CMD_TeleportToCoords(ENetPlayer player, double x, double y, double z)
        {
            try
            {
                NAPI.Entity.SetEntityPosition(player, new Vector3(x, y, z));
            }
            catch (Exception ex) { Logger.WriteError("CMD_TeleportToCoords", ex); }
        }

        [ChatCommand("tp", Arguments = "[ID игрока]", Description = "Телепортироваться к игроку", Access = PlayerRank.Helper)]
        public void CMD_TeleportToPlayer(ENetPlayer player, int dynamicId)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(dynamicId);
                if (target == null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistDynamic, dynamicId));
                    return;
                }

                NAPI.Entity.SetEntityDimension(player, target.Dimension);
                NAPI.Entity.SetEntityPosition(player, target.Position);
                LoggingService.Instance.Create(new AdminCommandUsageLogMessage(player.GetUUID(), player.GetName(), target.GetUUID(), target.GetName(), "tp"));
            }
            catch (Exception ex) { Logger.WriteError("CMD_TeleportToPlayer", ex); }
        }

        [ChatCommand("play", Arguments = "[группа] [название] [флаг]", Description = "Проиграть анимацию", Access = PlayerRank.Owner)]
        public void CMD_PlayAnim(ENetPlayer player, string dict, string name, int flag)
        {
            player.PlayAnimation(dict, name, flag);
        }

        [ChatCommand("scenario", Arguments = "[название]", Description = "Запустить сценарий", Access = PlayerRank.Owner)]
        public void Command_Scenario(ENetPlayer player, string scenario)
        {
            try
            {
                NAPI.Player.PlayPlayerScenario(player, scenario);
            }
            catch(Exception ex) { Logger.WriteError("Command_Scenario", ex); }
        }

        [ChatCommand("rescue", Arguments = "[ID игрока]", Description = "Возродить игрока по динамическому айди", Access = PlayerRank.Admin)]
        public void CMD_Rescue(ENetPlayer player, int dynamicId)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(dynamicId);
                if (target == null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistDynamic, dynamicId));
                    return;
                }

                NAPI.Player.SpawnPlayer(target, target.Position + new Vector3(0, 0, 0.5));
                target.StopAnimation();
                target.Health = 100;
                LoggingService.Instance.Create(new AdminCommandUsageLogMessage(player.GetUUID(), player.GetName(), target.GetUUID(), target.GetName(), "rescue"));
            }
            catch(Exception ex) { Logger.WriteError("CMD_Rescue", ex); }
        }

        [ChatCommand("getdim", Description = "Узнать номер виртуального мира игрока", Arguments = "[динамик]", Access = PlayerRank.Helper)]
        public static void Command_GetDimension(ENetPlayer player, int dynamicId)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(dynamicId);
                if (target == null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistDynamic, dynamicId));
                    return;
                }

                ChatHandler.SendMessage(player, $"Игрок ({dynamicId}) находится в {target.Dimension} измерении!", new ChatAddition(ChatType.System));
            }
            catch (Exception ex) { Logger.WriteError("Command_GetDimension", ex); }
        }

        [ChatCommand("givecash", Arguments = "[ID игрока] [количество]", Description = "Выдать деньги игроку по динамическому айди", Access = PlayerRank.CheifAdmin)]
        public void Command_GiveCash(ENetPlayer player, int dynamicId, int cash)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(dynamicId);
                if (target == null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistDynamic, dynamicId));
                    return;
                }

                if (!target.ChangeWallet(cash))
                {
                    player.SendError($"Не удалось выдать деньги игроку {dynamicId}");
                    return;
                }
                else
                {
                    player.SendDone($"Вы выдали деньги игроку {target.GetName()} (#{target.GetUUID()})");
                    LoggingService.Instance.Create(new AdminCommandUsageLogMessage(player.GetUUID(), player.GetName(), target.GetUUID(), target.GetName(), "givecash"));
                }
            }
            catch(Exception ex) { Logger.WriteError("Command_GiveCash", ex); }
        }
    }
}
