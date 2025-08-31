using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Vehicles
{
    public class VehicleModel
    {
        public int Id { get; set; }
        public string RealName { get; set; }
        public string ModelName { get; set; }
        public VehicleType Type { get; set; }
        public VehicleEngineType EngineType { get; set; }
        public int TrunkWeight { get; set; }
        public int TrunkSlot { get; set; }
        public int Tank { get; set; }
        public string ImgUrl { get; set; }
        public VehicleModel(int id, string realname, string modelname, VehicleType type, VehicleEngineType vehicleEngineType, int trunkWeight, int trunkSlot, int tank, string imgUrl)
        {
            Id = id;
            RealName = realname;
            ModelName = modelname;
            Type = type;
            EngineType = vehicleEngineType;
            TrunkWeight = trunkWeight;
            TrunkSlot = trunkSlot;
            Tank = tank;
            ImgUrl = imgUrl;
        }
    }
}
