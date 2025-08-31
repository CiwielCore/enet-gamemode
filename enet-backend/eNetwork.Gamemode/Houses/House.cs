using eNetwork.Framework;
using eNetwork.Houses.Garage;
using eNetwork.Houses.Interior;
using eNetwork.Houses.Storage;
using eNetwork.Inv;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Houses
{
    public class House
    {
        private static readonly Logger Logger = new Logger("house");

        public int Id { get; set; }
        public int Owner { get; set; } = -1;
        public double Price { get; set; }
        public Position Position { get; set; }
        public HouseInteriorType InteriorType { get; set; }
        public InteriorData InteriorData { get; set; }
        public double Tax { get; set; }
        public bool IsLocked { get; set; } = false;
        public string Street { get; set; }
        public List<Item> StorageItems { get; set; } = new List<Item>();

        public House(int id, double price, Position position, HouseInteriorType interiorType, string street = "")
        {
            Id = id;
            Price = price;
            Position = position;
            InteriorType = interiorType;

            InteriorData = HousesManager.GetInteriorData(InteriorType).Clone();
            Street = street;
        }

        public Position GaragePosition { get; set; }
        public GarageType GarageType { get; set; }
        public GarageData Garage { get; set; }

        public void SetGarage(Position position, GarageType garageType)
        {
            GaragePosition = position;
            GarageType = garageType;

            Garage = HousesManager.GetGarageData(GarageType).Clone();
        }

        public StorageData Storage { get; set; }
        public void CreateStorage()
        {
            Storage = new StorageData(this);
        }

        private ENetColShape _colShape { get; set; }
        private ENetBlip _blip { get; set; }
        private Marker _marker { get; set; }

        private ENetColShape _garageColShape { get; set; }
        public void GTAElements()
        {
            _colShape = ENet.ColShape.CreateCylinderColShape(Position.GetVector3(), 1f, 2, 0, ColShapeType.House);
            _colShape.OnEntityEnterColShape += (s, e) => e.SetData("house", this);
            _colShape.OnEntityExitColShape += (s, e) => { e.ResetData("house"); ClientEvent.Event(e as ENetPlayer, "client.house.close"); };
            _colShape.SetInteractionText($"Дом #{Id}");

            _marker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, Position.GetVector3() - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), .7f, Helper.GTAColor, false, 0);

            _garageColShape = ENet.ColShape.CreateCylinderColShape(GaragePosition.GetVector3(), 4f, 2, 0, ColShapeType.Garage);
            _garageColShape.AddPredicate((shape, player) =>
            {
                if (CanAccess(player.GetUUID())) return true; 
                return false;
            });
            _garageColShape.OnEntityEnterColShape += (s, e) => e.SetData("garage", Garage);
            _garageColShape.OnEntityExitColShape += (s, e) => e.ResetData("garage");
            _garageColShape.SetInteractionText("Гараж дома");

            InteriorData.GTAElements(this);
            Garage.GTAElements(this);
            CreateStorage();

            Update();
        }

        public void Update()
        {
            ENet.Task.SetMainTask(() =>
            {
                if (_blip is null)
                    _blip = ENet.Blip.CreateBlip(40, Position.GetVector3(), .6f, 2, "Дом", 255, 0, true, 0, 0);

                byte blipColor = 1;
                if (Owner == -1)
                    blipColor = 2;

                NAPI.Blip.SetBlipColor(_blip, blipColor);
            });
        }

        public uint GetDimension()
        {
            return 333333 + (uint)Id;
        }

        public List<ENetPlayer> Players = new List<ENetPlayer>();

        public void SendPlayer(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                if (!Players.Contains(player))
                    Players.Add(player);

                InteriorData.Position.Set(player);
                player.SetDimension(GetDimension());

                sessionData.EnteredHouse = Id;
                characterData.ExteriosPosition = Position.GetVector3();
            }
            catch(Exception ex) { Logger.WriteError("SendPlayer", ex); }
        }
        
        public void RemovePlayer(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                if (Players.Contains(player))
                    Players.Remove(player);

                Position.Set(player);
                player.SetDimension(0);

                sessionData.EnteredHouse = -1;
                characterData.ExteriosPosition = null;
            }
            catch(Exception ex) { Logger.WriteError("RemovePlayer", ex); }
        }

        public void TryBuy(ENetPlayer player)
        {
            try
            {
                if (HousesManager.GetPlayerHouse(player.GetUUID()) != null)
                {
                    player.SendError("У вас уже есть дом!");
                    return;
                }

                if (Owner != -1)
                {
                    player.SendError("Дом уже куплен!");
                    return;
                }

                if (!player.ChangeWallet(-Price)) return;

                Owner = player.GetUUID();
                SendPlayer(player);

                player.SendDone("Поздравляем с покупкой дома!");
                HousesManager.Load(player);

                Save().Wait();
                Update();
            }
            catch(Exception ex) { Logger.WriteError("TryBuy", ex); }
        }

        public bool CanAccess(int uuid)
        {
            return Owner == uuid;
        }

        public void Create()
        {
            ENet.Database.Execute($"INSERT INTO `houses` (`id`, `owner`, `type`, `price`, `position`, `tax`, `locked`, `garageType`, `garagePosition`, `storage`) " +
                $"VALUES({Id}, {Owner}, {(int)InteriorType}, {Price}, '{JsonConvert.SerializeObject(Position)}', {Tax}, {Convert.ToInt32(IsLocked)}, {(int)GarageType}, '{JsonConvert.SerializeObject(GaragePosition)}', '[]')");
        }

        public async Task Save()
        {
            await ENet.Database.ExecuteAsync($"UPDATE `houses` SET owner={Owner}, tax={Tax}, locked={Convert.ToInt32(IsLocked)}, storage='{JsonConvert.SerializeObject(StorageItems)}' WHERE id={Id}");
        }
    }
}
