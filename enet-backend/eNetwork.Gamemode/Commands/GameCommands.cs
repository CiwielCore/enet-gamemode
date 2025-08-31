using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using GTANetworkAPI;
using eNetwork.Game;
using eNetwork.World;
using eNetwork.Framework;

namespace eNetwork.Commands
{
    class GameCommands : Script
    {
        [ChatCommand("settime", Arguments = "[время]", Description = "Установить фиксированное игровое время на сервере", Access = PlayerRank.SeniorAdmin)]
        public void SetTime(ENetPlayer player, int hour)
        {
            string result = WeatherHandler.SetCustomHour(hour);
            ChatHandler.SendMessage(player, result);
        }

        [ChatCommand("ts", Access = PlayerRank.Owner)]
        public void TestSound(ENetPlayer player, string name, string dict)
        {
            ClientEvent.Event(player, "soundtest", name, dict);
        }

        [ChatCommand("gun")]
        public void GiveGun(ENetPlayer player)
        {
            NAPI.Player.GivePlayerWeapon(player, WeaponHash.Assaultrifle, 1000);
        }

        [ChatCommand("gw", Access = PlayerRank.SeniorAdmin, Arguments = "[оружие] [патроны]", Description = "Выдать оружие себе")]
        public void GiveGun(Player player, string Name, int ammo)
        {
            NAPI.Player.GivePlayerWeapon(player, WeaponHash.Carbinerifle, ammo);
        }

        [ChatCommand("ct", Access = PlayerRank.Owner)]
        public void CreateTrailer(ENetPlayer player)
        {
            var truck = NAPI.Vehicle.CreateVehicle(VehicleHash.Phantom, player.Position, 0, 0, 0, "TRUCK");
            truck.SetData("VTYPE", "SERVER");
            truck.SetData("CREATED_BY", player.Name);
            player.SetIntoVehicle(truck, 0);

            NAPI.Task.Run(() =>
            truck.SetSharedData(Game.Vehicles.TrailerSync.SyncData, JsonConvert.SerializeObject(Game.Vehicles.TrailerSync.CreateTrailer("trailers3"))), 1000);
        }
    }
}
