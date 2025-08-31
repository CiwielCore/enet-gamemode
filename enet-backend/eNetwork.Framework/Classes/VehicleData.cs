using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using eNetwork.Framework;
using eNetwork.Inv;
using eNetwork.Vehicles;

namespace eNetwork
{
    public class VehicleData
    {
        public VehicleData(int iD, VehicleOwner owner, string numberPlate, string model, int health, float fuel, float mile, VehicleCustomization components, List<Item> items, Position position, float dirt, string parkedPlace)
        {
            ID = iD;
            Owner = owner;
            NumberPlate = numberPlate;
            Model = model;
            Health = health;
            Fuel = fuel;
            Mile = mile;
            Components = components;
            Items = items;
            Position = position;
            Dirt = dirt;
            ParkedPlace = parkedPlace;
        }

        public int ID { get; private set; }
        public VehicleOwner Owner { get; private set; }
        public string NumberPlate { get; set; }
        public string Model { get; set; }
        public VehicleModel StockModel { get; set; }
        public int Health { get; set; } = 1000;
        public float Fuel { get; set; } = 0;
        public float Mile { get; set; } = 0;
        public VehicleCustomization Components { get; set; } = new VehicleCustomization();
        public List<Item> Items { get; set; } = new List<Item>();
        public Position Position { get; set; }
        public float Dirt { get; set; } = 0;

        public string ParkedPlace { get; set; }

        public void SetPosition(Position newPosition)
        {
            Position = newPosition;
        }

        public string GetVehicleDataToJson(string vehnumber = null)
        {
            var data = new
            {
                Model = Model,
                Fuel = Fuel,
                Mile = Mile,
            };

            return JsonConvert.SerializeObject(data);
        }
    }

    public class VehicleOwner()
    {
        public OwnerVehicleEnum OwnerVehicleType { get; private set; }

        public int OwnerUUID { get; private set; }

        public VehicleOwner(OwnerVehicleEnum ownerVehicleType, int ownerUUID) : this()
        {
            OwnerVehicleType = ownerVehicleType;
            OwnerUUID = ownerUUID;
        }

        public void ChangeOwner(ENetPlayer player, int newOwnerUUID, OwnerVehicleEnum ownerVehicleType)
        {
            if (newOwnerUUID < 0)
            {
                ENet.Chat.SendMessage(player, "Недопустимое значение UUID");
                return;
            }

            OwnerVehicleType = ownerVehicleType;
            OwnerUUID = newOwnerUUID;
        }
    }

    public enum OwnerVehicleEnum
    {
        Player = 0,
        Faction = 1
    }

    public class VehicleCustomization
    {
        public Color NeonColor = new Color(0, 0, 0, 0);

        public int PrimModColor = 0;
        public int SecModColor = 0;

        public int Muffler = -1;
        public int SideSkirt = -1;
        public int Hood = -1;
        public int Spoiler = -1;
        public int Lattice = -1;
        public int Wings = -1;
        public int Roof = -1;
        public int Vinyls = -1;
        public int FrontBumper = -1;
        public int RearBumper = -1;

        public int Engine = -1;
        public int Turbo = -1;
        public int Horn = -1;
        public int Transmission = -1;
        public int WindowTint = 0;
        public int Suspension = -1;
        public int Brakes = -1;
        public int Headlights = -1;

        public int NumberPlate = 0;

        public int Wheels = -1;
        public int WheelsType = 0;
        public int WheelsColor = 0;

        public int Armor = -1;
    }
}