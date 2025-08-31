using eNetwork.Framework;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Property.Parking
{
    public class Parking
    {
        private static Logger Logger = new Logger("parking");
        public int Id { get; set; }
        public int Cost { get; set; }
        public Position Position { get; set; }
        public Position EnterPosition { get; set; }
        public uint Dimension { get; set; }
        public List<int> Places { get; set; }
        public InteriorType InteriorType { get; set; }

        [JsonIgnore]
        private ENetColShape PedColShape { get; set; }

        [JsonIgnore]
        private ENetBlip Blip { get; set; }

        [JsonIgnore]
        private ENetColShape EnterColShape { get; set; }

        [JsonIgnore]
        private Ped Ped { get; set; }

        [JsonIgnore]
        private Marker EnterMarker { get; set; }

        public Parking(int id, Position position, Position enterPosition, uint dim, int type, int cost)
        {
            Id = id;
            Position = position;
            EnterPosition = enterPosition;
            Dimension = dim;
            InteriorType = (InteriorType)type;
            Cost = cost;
        }

        public void CreateFloor()
        {
            try
            {
                var interiorData = Interior.GetInteriorData(InteriorType);
                if (interiorData is null)
                {
                    Logger.WriteError($"Не удалось получить данные об интерьере {InteriorType.ToString()} для парковки #{Id}");
                    return;
                }

                int floor = ParkingManager.GetFloor(Id) + 1;

                foreach (var pos in interiorData.Positions)
                {
                    var place = ParkingManager.CreateParkingPlace(Dimension + (uint)floor, interiorData.Positions.IndexOf(pos), Id, floor);
                    Places.Add(place.Id);
                }

                Save();
            }
            catch(Exception ex) { Logger.WriteError("CreateFloor", ex); }
        }

        public void SetPlaces(List<int> places)
        {
            try
            {
                if (places is null)
                    places = new List<int>();
                
                Places = places;
                GTAElements();
            }
            catch(Exception ex) { Logger.WriteError("SetPlaces", ex); }
        }

        public void GTAElements()
        {
            try
            {
                Blip = ENet.Blip.CreateBlip(50, Position.GetVector3(), .8f, 4, "Паркинг", 255, 0, true, 0, 0);
                PedColShape = ENet.ColShape.CreateCylinderColShape(Position.GetVector3(), 1f, 2f, 0, ColShapeType.ParkingPed);
                PedColShape.OnEntityEnterColShape += (s, e) => 
                {
                    e.SetData("parking.col", this);
                };
                PedColShape.OnEntityExitColShape += (s, e) =>
                {
                    e.ResetData("parking.col");
                };
                PedColShape.SetInteractionText($"Узнать информацию о паркинге #{Id}");

                Ped = NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("a_m_m_bevhills_02"), Position.GetVector3(), Position.GetHeading(), false, true, true, true, 0);

                CreateEnterPoint();
            }
            catch (Exception ex) { Logger.WriteError("GTAElements", ex); }
        }

        [JsonIgnore]
        private static List<Elevator> Elevators = new List<Elevator>();
        public void CreateElevators()
        {
            try
            {
                int floors = ParkingManager.GetFloor(Id);
                for(int i = 0; i < floors; i++)
                {
                    Elevators.Add(new Elevator(this, i+1));
                }
            }
            catch(Exception ex) { Logger.WriteError("CreateElevators", ex); }
        }   

        public void Save()
        {
            try
            {
                ENet.Database.Execute($"UPDATE `{ParkingManager.ParkingDB}` SET `enter`='{JsonConvert.SerializeObject(EnterPosition)}', `places`='{JsonConvert.SerializeObject(Places)}' WHERE `id`={Id}");
            }
            catch(Exception ex) { Logger.WriteError("Save", ex); }
        }

        public void CreateEnterPoint()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (EnterColShape != null)
                        EnterColShape.Delete();
                    if (EnterMarker != null)
                        EnterMarker.Delete();

                    EnterColShape = ENet.ColShape.CreateCylinderColShape(EnterPosition.GetVector3(), 3f, 4f, 0, ColShapeType.ParkingEnter);
                    EnterColShape.OnEntityEnterColShape += (s, e) =>
                    {
                        e.SetData("parking.col", this);  
                    };
                    EnterColShape.OnEntityExitColShape += (s, e) =>
                    {
                        e.ResetData("parking.col");
                        ClientEvent.Event((ENetPlayer)e, "client.parking.elevator.disable");
                    };
                    EnterColShape.SetInteractionText("Въезд в паркинг");

                    EnterMarker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, EnterPosition.GetVector3() - new Vector3(0, 0, 1), new Vector3(), new Vector3(), 4f, Helper.GTAColor, false, 0);
                }
                catch (Exception ex) { Logger.WriteError("CreateEnterPoint", ex); }
            });
        }

        public class Elevator
        {
            public ENetColShape ColShape { get; set; }
            public Marker Marker { get; set; }

            public Elevator(Parking parking, int floor)
            {
                var interiorData = Interior.GetInteriorData(parking.InteriorType);
                if (interiorData is null) return;
                Vector3 position = interiorData.Enterpoint.GetVector3() - new Vector3(0, 0, 1.12);

                ColShape = ENet.ColShape.CreateCylinderColShape(position, .7f, 2f, parking.Dimension + (uint)floor, ColShapeType.ParkingElevator);
                ColShape.OnEntityEnterColShape += (s, e) =>
                {
                    e.SetData("parking.elevator", parking);
                };
                ColShape.OnEntityExitColShape += (s, e) =>
                {
                    e.ResetData("parking.elevator");
                    ClientEvent.Event((ENetPlayer)e, "client.parking.elevator.disable");
                };
                ColShape.SetInteractionText("Лифт паркинга");

                Marker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, position, new Vector3(), new Vector3(), .7f, Helper.GTAColor, false, parking.Dimension + (uint)floor);
            }
        }
    }
}
