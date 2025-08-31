using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using GTANetworkAPI;
using MySqlConnector;
using eNetwork.Framework;
using eNetwork.Framework.Singleton;

namespace eNetwork.Services.CarRental
{
    internal class CarRentalRepository : Singleton<CarRentalRepository>
    {
        private readonly Logger _logger = new Logger(nameof(CarRentalRepository));

        private readonly List<CarRentalPoint> _rentPoints;

        public IReadOnlyList<CarRentalModel> CarRentalModels => _rentCarModels;
        private readonly List<CarRentalModel> _rentCarModels;

        private CarRentalRepository()
        {
            _rentPoints = new List<CarRentalPoint>();
            _rentCarModels = new List<CarRentalModel>();
        }

        public void OnResourceStart()
        {
            LoadCarRentalPoints();
            LoadCarRentalModels();
        }

        private void LoadCarRentalPoints()
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM `car_rental_points_car_spawns`;");
            DataTable result = ENet.Database.ExecuteRead(command);

            if (result is null || result.Rows.Count == 0)
            {
                _logger.WriteWarning("Car rental points table is empty");
                return;
            }

            List<KeyValuePair<uint, Position>> spawns = new List<KeyValuePair<uint, Position>>();
            foreach (DataRow row in result.Rows)
            {
                uint pointId = Convert.ToUInt32(row["point_id"]);
                float spawnX = Convert.ToSingle(row["spawn_x"]);
                float spawnY = Convert.ToSingle(row["spawn_y"]);
                float spawnZ = Convert.ToSingle(row["spawn_z"]);
                float rotation = Convert.ToSingle(row["rotation"]);

                spawns.Add(new KeyValuePair<uint, Position>(pointId, new Position(spawnX, spawnY, spawnZ, rotation)));
            }

            command = new MySqlCommand("SELECT * FROM `car_rental_points`;");
            result = ENet.Database.ExecuteRead(command);

            if (result is null || result.Rows.Count == 0)
                return;

            foreach (DataRow row in result.Rows)
            {
                uint id = Convert.ToUInt32(row["id"]);
                float locationX = Convert.ToSingle(row["location_x"]);
                float locationY = Convert.ToSingle(row["location_y"]);
                float locationZ = Convert.ToSingle(row["location_z"]);
                float pedHeading = Convert.ToSingle(row["ped_heading"]);

                if (spawns.Any(s => s.Key == id) == false)
                {
                    _logger.WriteWarning($"CarRentalPoint with id ({id}) no has car spawn points");
                    continue;
                }

                Vector3 location = new Vector3(locationX, locationY, locationZ);
                IEnumerable<Position> tempSpawns = spawns.Where(s => s.Key == id).Select(s => s.Value);
                _rentPoints.Add(new CarRentalPoint(id, pedHeading, location, tempSpawns));
            }

            spawns.Clear();
            spawns.Capacity = 0;
        }

        private void LoadCarRentalModels()
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM `car_rental_models`;");
            DataTable result = ENet.Database.ExecuteRead(command);

            if (result is null || result.Rows.Count == 0)
            {
                _logger.WriteWarning("Car rental models table is empty");
                return;
            }

            foreach (DataRow row in result.Rows)
            {
                string modelName = Convert.ToString(row["model_name"]);
                uint costPerHour = Convert.ToUInt32(row["cost_per_hour"]);
                string type = Convert.ToString(row["type"]);
                string img = Convert.ToString(row["img"]);

                _rentCarModels.Add(new CarRentalModel(modelName, costPerHour, type, img));
            }
        }
    }
}
