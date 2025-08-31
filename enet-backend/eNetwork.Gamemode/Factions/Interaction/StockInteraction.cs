using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Factions.Interaction
{
    internal class StockInteraction
    {
        public static void UnLoadingInStockFaction(ENetPlayer player, ColShape colShape)
        {
            if (player.IsInVehicle && player.Vehicle.DisplayName == "")
                return;

            ENetVehicle playerVehicle = ENet.Pools.GetVehicleById(player.Vehicle.Id);
            if (playerVehicle.VehicleData.Owner.OwnerVehicleType == OwnerVehicleEnum.Faction && playerVehicle.VehicleData.Owner.OwnerUUID == player.CharacterData.FactionId)
            {
                player.SendError("Данный транспорт не подойдет");
                return;
            }
        }
    }
}