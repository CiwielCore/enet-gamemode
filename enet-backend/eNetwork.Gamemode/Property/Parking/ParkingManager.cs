using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Framework.API.SceneManager.SceneAction;
using eNetwork.Game;
using eNetwork.Game.Characters;
using eNetwork.GameUI;
using GTANetworkAPI;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eNetwork.Property.Parking
{
    public class ParkingManager
    {
        private static readonly Logger Logger = new Logger("parking-manager");

        public static Dictionary<int, Parking> Parkings = new Dictionary<int, Parking>();
        public static readonly string ParkingDB = "parkings";
        private static int _parkingLastId = 0;

        public static Dictionary<int, ParkingPlace> Places = new Dictionary<int, ParkingPlace>();
        public static readonly string PlacesDB = "parking_places";
        private static int _placesLastId = 0;

        public static void Initialize()
        {
            try
            {
                DataTable data = ENet.Database.ExecuteRead($"SELECT * FROM `{ParkingDB}`");
                if (data != null && data.Rows.Count != 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        int id = Convert.ToInt32(row["id"]);
                        int cost = Convert.ToInt32(row["cost"]);
                        Position pos = JsonConvert.DeserializeObject<Position>(row["pos"].ToString());
                        Position enter = JsonConvert.DeserializeObject<Position>(row["enter"].ToString());
                        uint dim = Convert.ToUInt32(row["dim"]);
                        List<int> places = JsonConvert.DeserializeObject<List<int>>(row["places"].ToString());
                        int type = Convert.ToInt32(row["type"]);

                        if (_parkingLastId < id)
                            _parkingLastId = id;

                        Parking parking = new Parking(id, pos, enter, dim, type, cost);
                        parking.SetPlaces(places);

                        Parkings.TryAdd(id, parking);
                    }
                }
                Logger.WriteInfo($"Загружено {Parkings.Count} парковок");

                data = ENet.Database.ExecuteRead($"SELECT * FROM `{PlacesDB}`");
                if (data != null && data.Rows.Count != 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        int id = Convert.ToInt32(row["id"]);
                        int owner = Convert.ToInt32(row["owner"]);
                        uint dim = Convert.ToUInt32(row["dim"]);
                        int vehId = Convert.ToInt32(row["vehid"]);
                        int posIndex = Convert.ToInt32(row["posindex"]);
                        DateTime paid = Convert.ToDateTime(row["paid"]);
                        int parkingId = Convert.ToInt32(row["parkingid"]);
                        int floor = Convert.ToInt32(row["floor"]);

                        if (_placesLastId < id)
                            _placesLastId = id;

                        ParkingPlace plcae = new ParkingPlace(id, owner, dim, vehId, posIndex, paid, parkingId, floor);
                        Places.TryAdd(id, plcae);
                    }
                }
                Parkings.Values.ToList().ForEach((x) => x.CreateElevators());

                Logger.WriteInfo($"Загружено {Places.Count} парковочных мест");
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static ParkingPlace CreateParkingPlace(uint dimension, int positionIndex, int parkingId, int floor)
        {
            try
            {
                _placesLastId++;
                var place = new ParkingPlace(_placesLastId, -1, dimension, -1, positionIndex, DateTime.Now, parkingId, floor);
                Places.TryAdd(place.Id, place);

                ENet.Database.Execute($"INSERT INTO `{PlacesDB}` (`id`,`owner`,`dim`,`vehid`,`posindex`,`paid`,`parkingid`,`floor`) " +
                    $"VALUES({place.Id}, {place.Owner}, {place.Dimension}, {place.VehicleId}, {place.PositionIndex}, '{Helper.ConvertTime(place.PaidDate)}', {place.ParkingId}, {place.Floor})");

                return place;
            }
            catch (Exception ex) { Logger.WriteError("CreateParkingPlace", ex); return null; }
        }

        public static int GetFloor(int parkingId)
        {
            try
            {
                var parkingPlaces = Places.Where(x => x.Value.ParkingId == parkingId).ToList();
                int lastFloor = 0;
                parkingPlaces.ForEach((x) =>
                {
                    if (x.Value.Floor > lastFloor)
                        lastFloor = x.Value.Floor;
                });

                return lastFloor;
            }
            catch (Exception ex) { Logger.WriteError("GetFloor", ex); return 0; }
        }

        public static Parking GetParkingByPlace(int vehId)
        {
            var place = GetPlace(vehId);
            if (place == null) return null;

            return Parkings.Values.ToList().Find(x => x.Places.Contains(place.Id));
        }

        public static ParkingPlace GetPlace(int vehId)
        {
            if (vehId == -1) return null;
            return Places.Values.ToList().Find(x => x.VehicleId == vehId);
        }

        public static ParkingPlace GetPlaceById(int parkingId)
        {
            return Places.Values.ToList().Find(x => x.Id == parkingId);
        }

        public static IEnumerable<ParkingPlace> GetPlaces(int owner)
        {
            return Places.Values.ToList().Where(x => x.Owner == owner);
        }

        public static bool HaveFreePlaceInCurrentParking(int owner, int parkingId, out ParkingPlace parkingPlace)
        {
            parkingPlace = null;

            Parking parking = GetParking(parkingId);
            if (parking is null) return false;

            var places = GetPlaces(owner).Where(x => x.VehicleId == -1 && x.ParkingId == parkingId);
            if (places.Count() == 0) return false;

            parkingPlace = places.ElementAt(0);
            return true;
        }

        public static Parking GetParking(int parkingId)
        {
            if (Parkings.TryGetValue(parkingId, out Parking data)) return data;
            return null;
        }

        [InteractionDeprecated(ColShapeType.ParkingPed, InteractionType.Key)]
        private static void InteractionParking(ENetPlayer player)
        {
            try
            {
                if (!player.HasData("parking.col")) return;
                if (player.IsInVehicle)
                {
                    player.SendError("Сначала выйдите с транспортного средства");
                    return;
                }
                Parking parking = player.GetData<Parking>("parking.col");

                var places = Places.Values.ToList().Where(x => x.ParkingId == parking.Id);

                Dictionary<int, List<object>> data = new Dictionary<int, List<object>>();
                foreach (var place in places)
                {
                    if (!data.ContainsKey(place.Floor))
                        data.Add(place.Floor, new List<object>());

                    data[place.Floor].Add(new
                    {
                        Id = place.Id,
                        Owner = place.Owner == -1 ? "Государство" : CharacterManager.GetName(place.Owner),
                        UUID = place.Owner,
                    });
                }

                ClientEvent.Event(player, "client.parking.open", JsonConvert.SerializeObject(data), parking.Cost);
            }
            catch (Exception ex) { Logger.WriteError("InteractionParking", ex); }
        }

        [InteractionDeprecated(ColShapeType.ParkingEnter, InteractionType.Key)]
        private static async void InteractionEnter(ENetPlayer player)
        {
            try
            {
                if (!player.HasData("parking.col") || !player.GetCharacter(out CharacterData characterData)) return;
                Parking currentParking = player.GetData<Parking>("parking.col");

                if (!player.IsInVehicle)
                {
                    int floors = GetFloor(currentParking.Id);
                    ClientEvent.Event(player, "client.parking.elevator.show", floors, false);
                    return;
                }

                ENetVehicle vehicle = (ENetVehicle)player.Vehicle;
                if (!vehicle.GetVehicleData(out VehicleData data)) return;

                ParkingPlace place = GetPlace(data.ID);
                Parking parking = null;

                if (place != null)
                    Parkings.TryGetValue(place.ParkingId, out parking);

                if (place is null || parking is null || parking != currentParking)
                {
                    player.SendError("У вас нет купленного свободного места на этой парковке");
                    return;
                }

                if (HaveFreePlaceInCurrentParking(characterData.UUID, currentParking.Id, out ParkingPlace myParkingPlace))
                {
                    if (place != null)
                    {
                        place.VehicleId = -1;
                        place.Save();

                        data.ParkedPlace = String.Empty;
                    }

                    myParkingPlace.VehicleId = data.ID;
                    myParkingPlace.SpawnVehicle();

                    myParkingPlace.Save();
                    player.SendDone($"Вы припарковали свое транспортное средство. Ваше парковочное место: #{myParkingPlace.Id}");
                    parking = currentParking;
                    place = myParkingPlace;
                }
                else
                {
                    player.SendError("У вас нет купленного свободного места на этой парковке");
                    return;
                }

                Interior interiorData = Interior.GetInteriorData(parking.InteriorType);
                if (interiorData is null)
                {
                    player.SendError("Ошибка получения данных о парковочном комплексе");
                    return;
                }

                Position position = interiorData.Positions.ElementAt(place.PositionIndex);
                if (position is null) return;

                Transition.Open(player, "Заезжаем в паркинг");

                await Task.Delay(500);

                characterData.ExteriosPosition = parking.EnterPosition.GetVector3();

                place.SpawnVehicle();
                interiorData.Enterpoint.Set(player);

                player.SetDimension(place.Dimension);

                player.SetData("parking", parking);

                Transition.Close(player);
            }
            catch (Exception ex) { Logger.WriteError("InteractionEnter", ex); }
        }

        [InteractionDeprecated(ColShapeType.ParkingElevator, InteractionType.Key)]
        private static void InteractionElevator(ENetPlayer player)
        {
            try
            {
                if (!player.HasData("parking")) return;
                Parking parking = player.GetData<Parking>("parking");

                int floors = GetFloor(parking.Id);
                ClientEvent.Event(player, "client.parking.elevator.show", floors, true);
            }
            catch (Exception ex) { Logger.WriteError("InteractionElevator", ex); }
        }

        #region Commands

        [ChatCommand("createparking", "Создать парковочный комплекс", Arguments = "[тип] [цена за 1 день] [дименшн] [этажи]", Access = PlayerRank.Owner)]
        public void Command_CreateParking(ENetPlayer player, int type, int cost, uint dimension, int floors)
        {
            try
            {
                _parkingLastId++;

                Position position = new Position(player.Position.X, player.Position.Y, player.Position.Z, player.Heading);
                var parking = new Parking(_parkingLastId, position, new Position(0, 0, 0, 0), dimension, type, cost);
                parking.SetPlaces(null);

                Parkings.TryAdd(_parkingLastId, parking);

                for (int i = 0; i < floors; i++)
                    parking.CreateFloor();

                parking.CreateElevators();

                MySqlCommand sqlCommand = new MySqlCommand($"INSERT INTO `{ParkingDB}` (`id`,`cost`,`pos`,`enter`,`dim`,`places`,`type`) VALUES(@ID, @COST, @POS, @ENTER, @DIM, @PLACES, @TYPE)");
                sqlCommand.Parameters.AddWithValue("@ID", parking.Id);
                sqlCommand.Parameters.AddWithValue("@COST", parking.Cost);
                sqlCommand.Parameters.AddWithValue("@POS", JsonConvert.SerializeObject(parking.Position));
                sqlCommand.Parameters.AddWithValue("@ENTER", JsonConvert.SerializeObject(parking.EnterPosition));
                sqlCommand.Parameters.AddWithValue("@DIM", parking.Dimension);
                sqlCommand.Parameters.AddWithValue("@PLACES", JsonConvert.SerializeObject(parking.Places));
                sqlCommand.Parameters.AddWithValue("@TYPE", (int)parking.InteriorType);

                ENet.Database.Execute(sqlCommand);
            }
            catch (Exception ex) { Logger.WriteError("Command_CreateParking", ex); }
        }

        [ChatCommand("parkingaddenter", Description = "Добавить позицию въезда в паркинг", Access = PlayerRank.Owner)]
        public void Command_ParkingAddEnter(ENetPlayer player, int parkingId)
        {
            try
            {
                if (!player.IsInVehicle)
                {
                    player.SendError("Вы должны находится в транспорте");
                    return;
                }
                if (!Parkings.TryGetValue(parkingId, out Parking parking))
                {
                    player.SendError($"Парковки #{parkingId} не существует!");
                    return;
                }

                parking.EnterPosition = new Position(player.Position.X, player.Position.Y, player.Position.Z, player.Heading);
                parking.CreateEnterPoint();
                parking.Save();
            }
            catch (Exception ex) { Logger.WriteError("Command_ParkingAddEnter", ex); }
        }

        #endregion Commands
    }
}