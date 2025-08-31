using eNetwork.Framework;
using eNetwork.Game.Vehicles;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using eNetwork.Property.Parking;

namespace eNetwork.GameUI.Phone.Apps
{
    public class MyRide
    {
        private static readonly Logger Logger = new Logger("phoneApp.my-ride");
        [CustomEvent("server.phone.transport.open")]
        public static void Open(ENetPlayer player)
        {
            try
            {
                var playerVehicles = VehicleManager.GetPlayerVehicles(player.GetUUID());
                if (playerVehicles is null)
                {
                    PhoneNotification.Send(player, PhoneNotificationType.MyRide, "Не удалось получить список транспорта.");
                    return;
                }

                var data = new List<object>();
                playerVehicles.ToList().ForEach(x => data.Add(new { Name = Configs.VehicleNames.Get(x.Model), Number = String.IsNullOrEmpty(x.NumberPlate) ? "Без номеров" : x.NumberPlate, Model = x.Model, ID = x.ID }));

                ClientEvent.Event(player, "client.phone.openApp", "transport", JsonConvert.SerializeObject(data));
                PhoneManager.MoveFinger(player, PhoneManager.FingerType.ButtonPress);
            }
            catch (Exception ex) { Logger.WriteError("Open", ex); }
        }

        [CustomEvent("server.phone.transport.gps")]
        public static void Gps(ENetPlayer player, int vehicleId)
        {
            try
            {
                if (!player.IsTimeouted("myride.action", 1) || !player.GetCharacter(out CharacterData characterData)) return;

                bool isAlive = false;
                Vector3 position = new Vector3();
                foreach(var vehicle in ENet.Pools.GetAllVehicles())
                {
                    if (!vehicle.HasData("in.parking") && vehicle.VehicleData != null && vehicle.VehicleData.ID == vehicleId)
                    {
                        isAlive = true;
                        position = vehicle.Position;
                        break;
                    }
                }

                if (isAlive)
                {
                    player.SendInfo("Местоположение транспорта найдено, метка установлена на вашем GPS");
                    player.SetWaypoint(position.X, position.Y);
                }
                else
                {
                    var parking = ParkingManager.GetParkingByPlace(vehicleId);
                    if (parking is null)
                    {
                        if (player.GetData("parking", out Parking currentPlayerParking))
                        {
                            if (ParkingManager.HaveFreePlaceInCurrentParking(characterData.UUID, currentPlayerParking.Id, out ParkingPlace place) && place != null)
                            {
                                place.VehicleId = vehicleId;
                                place.SpawnVehicle();
                                place.Save();

                                player.SendInfo("Вы припарковали транспортное средство на данной парковке! Совсем скоро она появится на месте");
                            }
                            else
                                player.SendWarning("Вам нужно купить парковочное место, чтобы пользоваться транспортом");
                        }
                        else 
                            player.SendWarning("Вам нужно купить парковочное место, чтобы пользоваться транспортом");
                    }
                    else
                    {
                        player.SendInfo("Транспорт стоит на парковке, метка установлена на вашем GPS");
                        player.SetWaypoint(parking.Position.X, parking.Position.Y);
                    }
                }

                PhoneManager.MoveFinger(player, PhoneManager.FingerType.ButtonPress);
            }
            catch(Exception ex) { Logger.WriteError("Gps", ex); }
        }
    }
}
