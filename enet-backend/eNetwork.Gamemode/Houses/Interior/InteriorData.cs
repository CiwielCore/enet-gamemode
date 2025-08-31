using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Houses.Interior
{
    public class InteriorData
    {
        private static readonly Logger Logger = new Logger("interior-data");

        public string Name { get; set; }
        public Position Position { get; set; }
        public Vector3 Storage { get; set; }
        public int StorageWeight { get; set; }

        private House House { get; set; }
        private ENetColShape _colShape { get; set; }
        private Marker _marker { get; set; }

        public void GTAElements(House house)
        {
            House = house;

            _colShape = ENet.ColShape.CreateCylinderColShape(Position.GetVector3(), 1, 2, House.GetDimension(), ColShapeType.HouseInterior);
            _colShape.OnEntityEnterColShape += (s, e) => e.SetData("interior.house", House);
            _colShape.OnEntityExitColShape += (s, e) => e.ResetData("interior.house");
            _colShape.SetInteractionText("Выход из дома");

            _marker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, Position.GetVector3() - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), .8f, Helper.GTAColor, false, House.GetDimension());
        }

        [InteractionDeprecated(ColShapeType.HouseInterior)]
        public static void OnInteraction(ENetPlayer player)
        {
            try
            {
                if (!player.GetSessionData(out var sesionData) || !player.GetData<House>("interior.house", out var shapeHouse) || sesionData.EnteredHouse == -1) return;

                var house = HousesManager.GetHouse(sesionData.EnteredHouse);
                if (house != shapeHouse) return;

                house.RemovePlayer(player);
            }
            catch(Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }
    }
}
