using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Game.Characters;
using eNetwork.Houses.Garage;
using eNetwork.Houses.Interior;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Houses
{
    public class HousesController
    {
        private static readonly Logger Logger = new Logger("houses-controller");

        [ChatCommand("createhouse", Access = PlayerRank.Owner)]
        public static void Command_CreateHouse(ENetPlayer player, int price, int type)
        {
            try
            {
                if (!Enum.IsDefined(typeof(HouseInteriorType), type))
                {
                    player.SendError("Такого типа не существует!");
                    return;
                }

                var house = new House(HousesManager.GenerateId(), price, new Position(player.Position.X, player.Position.Y, player.Position.Z, player.Heading), (HouseInteriorType)type);
                HousesManager.Houses.TryAdd(house.Id, house);

                ENet.Chat.SendMessage(player, $"Id дома - #{house.Id}");
            }
            catch (Exception ex) { Logger.WriteError("Command_CreateHouse", ex); }
        }

        [ChatCommand("creategarage", Access = PlayerRank.Owner)]
        public static void Command_CreateGarage(ENetPlayer player, int houseId, int type)
        {
            try
            {
                if (!player.IsInVehicle || player.Vehicle is null)
                {
                    player.SendError("Вы должны находится в авто!");
                    return;
                }

                if (!Enum.IsDefined(typeof(HouseInteriorType), type))
                {
                    player.SendError("Такого типа не существует!");
                    return;
                }

                var house = HousesManager.GetHouse(houseId);
                if (house is null)
                {
                    player.SendError($"Дом #{houseId} - не найден!");
                    return;
                }

                house.SetGarage(new Position(player.Vehicle.Position.X, player.Vehicle.Position.Y, player.Vehicle.Position.Z, player.Vehicle.Heading), (GarageType)type);
                house.GTAElements();
                house.Create();
            }
            catch(Exception ex) { Logger.WriteError("Command_CreateGarage", ex); }
        }

        [InteractionDeprecated(ColShapeType.House)]
        public static void OnInteractionHouse(ENetPlayer player)
        {
            try
            {
                if (!player.GetSessionData(out var sessionData) || !player.GetData<House>("house", out var house) || sessionData.EnteredHouse != -1) return;

                ClientEvent.Event(player, "client.house.open", JsonConvert.SerializeObject(new
                {
                    Id = house.Id,
                    Owner = house.Owner == -1 ? "Государство" : CharacterManager.GetName(house.Owner),
                    Price = house.Price,
                    Interior = house.InteriorType,
                    Garage = house.Garage.Places.Count,
                    InteriorName = HousesManager.GetInteriorData(house.InteriorType).Name,
                    IsMy = house.CanAccess(player.GetUUID())
                }));
            }
            catch (Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }

        [InteractionDeprecated(ColShapeType.Garage)]
        public static void OnInteractionGarage(ENetPlayer player)
        {
            try
            {
                if (!player.GetSessionData(out var sessionData) || !player.GetData<GarageData>("garage", out var garage) || sessionData.EnteredHouse != -1) return;

                garage.Interaction(player);
            }
            catch (Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }

        [CustomEvent("server.house.action")]
        public static void OnAction(ENetPlayer player, string type)
        {
            try
            {
                if (!player.GetSessionData(out var sessionData) || !player.GetData<House>("house", out var house)) return;

                switch (type)
                {
                    case "buy":
                        house.TryBuy(player);
                        break;
                    case "join":
                        if (house.IsLocked)
                        {
                            player.SendError($"Дом закрыт!");
                            return;
                        }

                        house.SendPlayer(player);
                        break;
                }

                ClientEvent.Event(player, "client.house.close");
            }
            catch (Exception ex) { Logger.WriteError("OnAction", ex); }
        }
    }
}
