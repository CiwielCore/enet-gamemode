using eNetwork.Framework;
using eNetwork.Framework.Classes;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Reflection;
using System.Text;
using GTANetworkAPI;
using System.Threading.Tasks;
using eNetwork.Inv;
using Newtonsoft.Json;
using eNetwork.Inv;

namespace eNetwork.Game.Vehicles
{
    public class VehicleManager
    {
        private static readonly Logger Logger = new Logger("vehicle-manager");
        private static readonly string DBName = "vehicles";
        private static int LastID = 0;

        public static Dictionary<int, VehicleData> Vehicles = new Dictionary<int, VehicleData>();
        public static List<string> Numbers = new List<string>();

        public static void Initialize()
        {
            try
            {
                DataTable data = ENet.Database.ExecuteRead($"SELECT * from `{DBName}`");
                foreach (DataRow row in data.Rows)
                {
                    VehicleData vData = new VehicleData((int)row["id"],
                                                        JsonConvert.DeserializeObject<VehicleOwner>(row["owner"].ToString()),
                                                        Convert.ToString(row["number"]),
                                                        Convert.ToString("model"), (int)row["health"],
                                                        (float)row["fuel"], (float)row["mile"],
                                                        JsonConvert.DeserializeObject<VehicleCustomization>(row["components"].ToString()),
                                                        JsonConvert.DeserializeObject<List<Item>>(row["items"].ToString()),
                                                        JsonConvert.DeserializeObject<Position>(row["position"].ToString()),
                                                        (float)row["dirt"],
                                                        "");

                    if (vData.ID > LastID)
                        LastID = vData.ID;

                    Numbers.Add(vData.NumberPlate);
                    AddData(vData.ID, vData);
                }

                LastID++;
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static void Save(ENetVehicle vehicle)
        {
            NAPI.Task.Run(async () =>
            {
                try
                {
                    if (!vehicle.GetVehicleData(out VehicleData data)) return;

                    data.SetPosition(new Position(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z, vehicle.Heading));
                    data.Fuel = vehicle.GetPetrol();
                    data.Mile = vehicle.GetMile();

                    MySqlCommand sqlCommand = new MySqlCommand($"UPDATE `{DBName}` SET `owner`=@OWNER, `model`=@MODEL, `health`=@HEALTH, `fuel`=@FUEL, `mile`=@MILE, `dirt`=@DIRT, `components`=@COMPONENTS, " +
                        $"`items`=@ITEMS, `position`=@POSITION, `number`=@NUMBER WHERE id=@ID");

                    sqlCommand.Parameters.AddWithValue("@OWNER", JsonConvert.SerializeObject(data.Owner));
                    sqlCommand.Parameters.AddWithValue("@MODEL", data.Model);
                    sqlCommand.Parameters.AddWithValue("@HEALTH", data.Health);
                    sqlCommand.Parameters.AddWithValue("@FUEL", Convert.ToInt32(data.Fuel));
                    sqlCommand.Parameters.AddWithValue("@MILE", Convert.ToInt32(data.Mile));
                    sqlCommand.Parameters.AddWithValue("@DIRT", data.Dirt);
                    sqlCommand.Parameters.AddWithValue("@COMPONENTS", JsonConvert.SerializeObject(data.Components));
                    sqlCommand.Parameters.AddWithValue("@ITEMS", JsonConvert.SerializeObject(data.Items));
                    sqlCommand.Parameters.AddWithValue("@POSITION", JsonConvert.SerializeObject(data.Position));
                    sqlCommand.Parameters.AddWithValue("@NUMBER", data.NumberPlate);
                    sqlCommand.Parameters.AddWithValue("@ID", data.ID);

                    await ENet.Database.ExecuteAsync(sqlCommand);
                }
                catch (Exception ex) { Logger.WriteError("Save", ex); }
            });
        }

        public static bool AddData(int id, VehicleData data)
        {
            try
            {
                return Vehicles.TryAdd(id, data);
            }
            catch (Exception ex) { Logger.WriteError("AddData", ex); return false; }
        }

        public static async Task<int> CreateVehicle(VehicleOwner owner, string model, int primaryColor, int secondaryColor, bool createNumber = true)
        {
            try
            {
                string number = "";
                int fuel = 0;

                if (createNumber)
                    number = GenerateNumber();

                if (VehicleSync.GetVehicleConfig(model, out VehicleConfig vehicleConfig))
                {
                    fuel = vehicleConfig.MaxFuel;
                }

                VehicleData data = new VehicleData(LastID, owner, number, model, 1000, fuel, 0.0f, new VehicleCustomization(),
                                                   new List<Item>(), new Position(new Vector3(0, 0, 0), 0.0f), 0.0f, "");
                LastID++;
                AddData(data.ID, data);

                MySqlCommand sqlCommand = new MySqlCommand($"INSERT INTO `{DBName}` (`id`,`owner`,`model`,`number`,`health`,`fuel`,`mile`,`dirt`,`components`,`items`,`position`) " +
                    $"VALUES(@ID, @OWNER, @MODEL, @NUMBER, @HEALTH, @FUEL, @MILE, @DIRT, @COMPONENTS, @ITEMS, @POSITION)");
                sqlCommand.Parameters.AddWithValue("@ID", data.ID);
                sqlCommand.Parameters.AddWithValue("@OWNER", JsonConvert.SerializeObject(data.Owner));
                sqlCommand.Parameters.AddWithValue("@MODEL", data.Model);
                sqlCommand.Parameters.AddWithValue("@NUMBER", data.NumberPlate);
                sqlCommand.Parameters.AddWithValue("@HEALTH", data.Health);
                sqlCommand.Parameters.AddWithValue("@FUEL", data.Fuel);
                sqlCommand.Parameters.AddWithValue("@MILE", data.Mile);
                sqlCommand.Parameters.AddWithValue("@DIRT", data.Dirt);
                sqlCommand.Parameters.AddWithValue("@COMPONENTS", JsonConvert.SerializeObject(data.Components));
                sqlCommand.Parameters.AddWithValue("@ITEMS", JsonConvert.SerializeObject(data.Items));
                sqlCommand.Parameters.AddWithValue("@POSITION", JsonConvert.SerializeObject(data.Position));

                await ENet.Database.ExecuteAsync(sqlCommand);

                return data.ID;
            }
            catch (Exception ex) { Logger.WriteError("CreateVehicle", ex); return -1; }
        }

        public static ENetVehicle SpawnVehicle(int owner, int id, Position position, uint dim = 0)
        {
            try
            {
                if (!Vehicles.TryGetValue(id, out VehicleData data)) return null;

                ENetVehicle vehicle = ENet.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(data.Model), position.GetVector3(), (float)position.Heading, 0, 0, data.NumberPlate, 255, false, true, dim);
                vehicle.SetVehicleData(data);
                vehicle.SetType(VehicleType.Personal);
                vehicle.ApplyCustomization();

                vehicle.VehicleData.Position = position;

                vehicle.SetSharedData("model.name", data.Model);
                vehicle.SetSharedData("owner", owner);

                return vehicle;
            }
            catch (Exception ex) { Logger.WriteError("SpawnVehicle", ex); return null; }
        }

        public static IEnumerable<VehicleData> GetPlayerVehicles(int uuid)
        {
            try
            {
                return Vehicles.Values.ToList().Where(x => x.Owner.OwnerUUID == uuid);
            }
            catch (Exception ex) { Logger.WriteError("GetPlayerVehicles", ex); return null; }
        }

        public static VehicleData GetVehicleData(int vehicleId)
        {
            if (Vehicles.TryGetValue(vehicleId, out VehicleData data))
                return data;
            return null;
        }

        public static string GenerateNumber()
        {
            var whiteList = new char[] { 'A', 'B', 'E', 'K', 'M', 'H', 'O', 'P', 'C', 'T', 'Y', 'X' };

            string number = "";
            do
            {
                number = "";

                number += ENet.Random.Next(0, 9);
                number += ENet.Random.Next(0, 9);
                number += whiteList[ENet.Random.Next(0, whiteList.Length)];
                number += whiteList[ENet.Random.Next(0, whiteList.Length)];
                number += whiteList[ENet.Random.Next(0, whiteList.Length)];
                number += ENet.Random.Next(0, 9);
                number += ENet.Random.Next(0, 9);
                number += ENet.Random.Next(0, 9);
            }
            while (Numbers.Contains(number));

            return number;
        }
    }
}