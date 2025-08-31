using eNetwork.Framework;
using eNetwork.Game.Characters;
using eNetwork.Houses.Data;
using eNetwork.Houses.Garage;
using eNetwork.Houses.Interior;
using eNetwork.Inv;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace eNetwork.Houses
{
    public class HousesManager
    {
        private static readonly Logger Logger = new Logger("houses-manager");
        public static ConcurrentDictionary<int, House> Houses = new ConcurrentDictionary<int, House>();

        public static void Initialize()
        {
            try
            {
                DataTable data = ENet.Database.ExecuteRead("SELECT * FROM `houses`");
                if (data != null && data.Rows.Count > 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        int id = Convert.ToInt32(row["id"]);
                        int owner = Convert.ToInt32(row["owner"]);
                        HouseInteriorType houseInteriorType = (HouseInteriorType)Convert.ToInt32(row["type"]);
                        double price = Convert.ToDouble(row["price"]);
                        double tax = Convert.ToDouble(row["tax"]);
                        Position position = JsonConvert.DeserializeObject<Position>(row["position"].ToString());
                        bool isLocked = Convert.ToInt32(row["locked"]) == 1;

                        GarageType garageType = (GarageType)Convert.ToInt32(row["garageType"]);
                        Position garagePosition = JsonConvert.DeserializeObject<Position>(row["garagePosition"].ToString());
                        var street = row["street"].ToString();

                        //исправить на подгрузку по новой системе
                        //List<Item> storage = JsonConvert.DeserializeObject<List<Item>>(row["storage"].ToString());

                        var house = new House(id, price, position, houseInteriorType, street);
                        house.Owner = owner;
                        house.Tax = tax;
                        house.IsLocked = isLocked;
                        //house.StorageItems = storage;

                        house.SetGarage(garagePosition, garageType);
                        house.GTAElements();

                        Houses.TryAdd(id, house);
                    }
                }

                Logger.WriteInfo($"Загружено {Houses.Count} домов!");
            }
            catch(Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static void Load(ENetPlayer player)
        {
            try
            {
                var house = GetPlayerHouse(player.GetUUID());
                if (house is null) return;

                ClientEvent.Event(player, "client.checkpoints.marker.create", "garage", 36, 1, house.GaragePosition.X, house.GaragePosition.Y, house.GaragePosition.Z + .5, Helper.GTAColor.Red, Helper.GTAColor.Green, Helper.GTAColor.Blue, Helper.GTAColor.Alpha);
            }
            catch(Exception ex) { Logger.WriteError("Load", ex); }
        }

        public static void UnLoad(ENetPlayer player)
        {
            try
            {
                ClientEvent.Event(player, "client.checkpoints.marker.remove", "garage");
            }
            catch(Exception ex) { Logger.WriteError("UnLoad", ex); }
        }

        public static House GetPlayerHouse(int uuid)
        {
            return Houses.Values.ToList().Find(x => x.CanAccess(uuid));
        }
        
        public static House GetHouse(int id)
        {
            Houses.TryGetValue(id, out House house);
            return house;
        }

        public static GaragePositionPlace GetVehicleInGarage(int houseOwner, int vehicleId)
        {
            var house = GetPlayerHouse(houseOwner);
            if (house is null) return null;

            return house.Garage.Places.Find(x => x.VehicleData != null && x.VehicleData.ID == vehicleId);
        }

        public static InteriorData GetInteriorData(HouseInteriorType houseInteriorType)
        {
            Config.HOUSE_INTERIORS.TryGetValue(houseInteriorType, out var interiorData); 
            return interiorData;
        }

        public static GarageData GetGarageData(GarageType garageType)
        {
            Config.HOUSE_GARAGES.TryGetValue(garageType, out var garageData);
            return garageData;
        }

        public static int GenerateId()
        {
            return Houses.Count == 0 ? 1 : Houses.Max(x => x.Key) + 1;
        }
    }
}
