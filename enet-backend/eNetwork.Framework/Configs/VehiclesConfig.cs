using eNetwork.Clothes;
using eNetwork.Vehicles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace eNetwork.Framework.Configs
{
    public static class VehiclesConfig
    {
        private static Dictionary<string, VehicleModel> Pool = new Dictionary<string, VehicleModel>();
        public static void Initialize()
        {
            var table = ENet.Database.ExecuteRead("SELECT * FROM `vehicle_models`");
            foreach (DataRow row in table.Rows)
            {
                var vehicleModel = new VehicleModel(
                    id: Convert.ToInt32(row["id"]),
                    realname: row["real_name"].ToString(),
                    modelname: row["model_name"].ToString(),
                    type: (Vehicles.VehicleType)Convert.ToInt32(row["type"]),
                    vehicleEngineType: (VehicleEngineType)Convert.ToInt32(row["engine_type"]),
                    trunkWeight: Convert.ToInt32(row["trunk_weight"]),
                    trunkSlot: Convert.ToInt32(row["trunk_slot"]),
                    tank: Convert.ToInt32(row["tank"]),
                    imgUrl: row["img"].ToString()
                    );
                Pool.Add(vehicleModel.ModelName, vehicleModel);
            }
        }
        public static VehicleModel Get(string model)
        {
            if (Pool.ContainsKey(model)) return Pool[model];
            if (Pool.Count > 0) return Pool.First().Value;
            return null;
        }
    }
}
