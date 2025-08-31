using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Property.Parking
{
    public class Events
    {
        private static readonly Logger Logger = new Logger("parking-events");

        [CustomEvent("server.parking.elevator.action")]
        private void Elevator(ENetPlayer player, int floor)
        {
            try
            {
                if (!player.HasData("parking.col") && !player.HasData("parking.elevator") || !player.GetCharacter(out CharacterData characterData)) return;

                Parking parking = player.HasData("parking.col") ? player.GetData<Parking>("parking.col") : player.GetData<Parking>("parking.elevator");
                if (player.Dimension == parking.Dimension + (uint)floor) return;
                if (floor == 0)
                {
                    player.ResetData("parking");

                    player.SetDimension(0);
                    parking.EnterPosition.Set(player);
                    characterData.ExteriosPosition = null;
                    return;
                }

                int floors = ParkingManager.GetFloor(parking.Id);
                if (floor < 1 || floor > floors) return;

                var interiorData = Interior.GetInteriorData(parking.InteriorType);
                if (interiorData is null)
                {
                    player.SendError("Ошибка получения данных о вашей парковке");
                    return;
                }
                ClientEvent.Event(player, "client.parking.elevator.disable");

                interiorData.Enterpoint.Set(player);
                player.SetDimension(parking.Dimension + (uint)floor);

                player.SetData("parking", parking);

                characterData.ExteriosPosition = parking.EnterPosition.GetVector3();
            }
            catch (Exception ex) { Logger.WriteError("Elevator", ex); }
        }

        [CustomEvent("server.parking.buy")]
        private void Buy(ENetPlayer player, int placeId)
        {
            try
            {
                if (!player.IsTimeouted("parking.buy", 1) || !player.GetCharacter(out CharacterData characterData)) return;

                ParkingPlace place = ParkingManager.GetPlaceById(placeId);
                if (place is null) return;

                place.Buy(player);
            }
            catch(Exception ex) { Logger.WriteError("Buy", ex); }
        }
    }
}
