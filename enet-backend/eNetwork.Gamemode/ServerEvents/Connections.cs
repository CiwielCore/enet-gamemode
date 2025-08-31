using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using GTANetworkAPI;
using eNetwork.Framework;
using eNetwork.Modules;
using eNetwork.World;
using eNetwork.Game.LunaPark;
using eNetwork.Game;

namespace eNetwork.ServerEvents
{
    public class Connections : Script
    {
        public static Logger Logger = new Logger("connections");

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player rPlayer)
        {
            ENetPlayer player = rPlayer as ENetPlayer;
            if (player == null)
                return;

            try
            {
                player.SetSharedData("user.socialclub", player.SocialClubName);

                player.HardwareID = player.Serial;
                player.IP = player.Address;
                player.UserName = player.SocialClubName;
                player.UserId = player.SocialClubId;

                VehicleRadioList.Init(player);

                // Проверка на белый список!
                if (!Game.Core.Whitelist.Manager.CanJoin(player))
                {
                    Logger.WriteWarning($"Игрок с Socialclub \n{player.UserName}\n попытался зайти на сервер, но был кикнут!");
                    player.ExtKick("Доступ запрещен!");
                    return;
                };

                Logger.WriteInfo($"Игрок | {player.Address} | {player.Name} | {player.SocialClubName} | зашел на сервер");

                uint dim = Game.DimensionHandler.RequestPrivateDimension(player);
                player.Dimension = dim;

                ClientEvent.Event(player, "client.start", ENet.CustomEvent.GetSecretCode(), ENet.CustomEvent.GetCallbackCode(), Engine.Config.UIUrl);       
            }
            catch(Exception ex) { Logger.WriteError("connect", ex); }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player rPlayer, DisconnectionType type, string reason)
        {
            ENetPlayer player = rPlayer as ENetPlayer;
            if (player == null)
                return;

            try
            {
                player.SavePosition(true);

                ENet.Task.SetTask(async () =>
                {
                    try
                    {
                        // Account saving
                        await Game.Accounts.AccountManager.Save(player);

                        // Character saving
                        await Game.Characters.CharacterManager.Save(player, true);
                        await Game.Inventory.Save(player.GetUUID());
                        await Services.BonusServices.DailyBonus.Instance.Save(player.GetUUID());
                    }
                    catch(Exception e) { Logger.WriteError("disconnect.task", e); }
                });

                Businesses.Showroom.EndTestdrive(player, false);

                Logger.WriteInfo($"Игрок {Helper.FormatName(player.Name)} вышел с сервера");

                foreach (var data in NAPI.Data.GetAllEntityData(player))
                    player.ResetData(data);

                Services.VipServices.VipService.Instance.OnPlayerDisconnected(player);
                Services.DrivingLicensing.DrivingLicenseService.Instance.OnPlayerDisconnected(player);

                ENet.Pools.OnDisconnect(player);
            }
            catch(Exception ex) { Logger.WriteError("disconnect", ex); }
        }
    }
}
