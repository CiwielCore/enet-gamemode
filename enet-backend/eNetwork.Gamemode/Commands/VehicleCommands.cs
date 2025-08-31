using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using eNetwork.Framework;
using eNetwork.Game;
using System.Linq;

namespace eNetwork.Commands
{
    public class VehicleCommands
    {
        private static readonly Logger Logger = new Logger("VehicleCommands");

        [ChatCommand("veh", Arguments = "[модель] [цвет1?] [цвет2?] [номер?]", Description = "Создание транспорта", Access = PlayerRank.Admin)]
        public void CMD_CreateVehicle(ENetPlayer player, string model, int firstColor = 0, int secondColor = 0, string number = "ELISION")
        {
            try
            {
                ENetVehicle vehicle = ENet.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(model), player.Position, player.Rotation.Z, firstColor, secondColor, number, 255, false, true, player.Dimension);
                vehicle.SetType(VehicleType.Server);
                vehicle.SetData("CREATED_BY", player.Name);
                vehicle.SetSharedData("model.name", model);

                vehicle.SetColors(firstColor, secondColor);

                player.SetIntoVehicle(vehicle, 0);
            }
            catch (Exception ex) { Logger.WriteError("CMD_CreateVehicle", ex); }
        }

        [ChatCommand("afuel", Description = "Установить автомобилю количество топлива", Access = PlayerRank.Helper, Arguments = "[ID] [Количество]")]
        public static void Command_OGuns(ENetPlayer player, int id, int value)
        {
            try
            {
                ENetVehicle vehicle = ENet.Pools.GetVehicleById(id);

                if (vehicle is null)
                {
                    ENet.Chat.SendMessage(player, "Транспорт не найден");
                    return;
                }

                vehicle.VehicleData.Fuel = value;
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("repair", Arguments = "[ID транспорта]", Description = "Починить транспорт", Access = PlayerRank.JuniorAdmin)]
        public static void CMD_RepairVehcile(ENetPlayer player, int VehicleID = -101)
        {
            try
            {
                if (VehicleID == -101)
                    if (player.IsInVehicle)
                        player.Vehicle.Repair();
                    else
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                            if (VehicleID == v.Value)
                                v.Repair();
            }
            catch (Exception ex) { Logger.WriteError("CMD_RepairVehcile", ex); }
        }

        [ChatCommand("ghv", Arguments = "[ID транспорта]", Description = "Телепортировать авто к себе", Access = PlayerRank.Helper)]
        public static void CMD_GetVehicle(ENetPlayer player, int vehicleid)
        {
            try
            {
                foreach (var vehicle in NAPI.Pools.GetAllVehicles())
                {
                    if (vehicleid == vehicle.Value)
                    {
                        NAPI.Entity.SetEntityPosition(vehicle, player.Position);
                        NAPI.Entity.SetEntityRotation(vehicle, player.Rotation);
                        NAPI.Entity.SetEntityDimension(vehicle, player.Dimension);
                    }
                }
            }
            catch(Exception ex) { Logger.WriteError("CMD_GetVehicle", ex); }
        }

        [ChatCommand("delallveh", Description = "Удалить весь созданный транспорт", Access = PlayerRank.SeniorAdmin)]
        public static void CMD_DeleteAllVehicles(ENetPlayer player)
        {
            try
            {
                foreach (ENetVehicle vehicle in ENet.Pools.GetAllVehicles())
                {
                    if (vehicle.VehicleType == VehicleType.Server)
                        vehicle.Delete();
                }
            }
            catch (Exception ex) { Logger.WriteError("CMD_DeleteAllVehicles: ", ex); }
        }

        [ChatCommand("delveh", Description = "Используйте: /delveh", Access = PlayerRank.JuniorAdmin)]
        public static void CMD_DeleteNearVehicle(ENetPlayer player)
        {
            try
            {
                ENetVehicle veh = Game.Vehicles.VehicleSync.GetNearestVehicle(player, 10);
                if (veh is null) return;

                if (player.Position.DistanceTo(veh.Position) < 10)
                    veh.Delete();
            }
            catch (Exception ex) { Logger.WriteError("CMD_DeleteNearVehicle" + ex.Message); }
        }

        [ChatCommand("delmycars", Description = "Используйте: /delmycars", Access = PlayerRank.JuniorAdmin)]
        public static void CMD_DeleteMyCars(ENetPlayer player)
        {
            try
            {
                List<ENetVehicle> vehicles = ENet.Pools.GetAllVehicles();

                foreach (ENetVehicle vehicleSort in vehicles)
                {
                    if (vehicleSort.GetData<string>("CREATED_BY") == player.Name)
                    {
                        vehicleSort.Delete();
                    }
                }
            }
            catch (Exception ex) { Logger.WriteError("CMD_DeleteNearVehicle" + ex.Message); }
        }
    }
}
