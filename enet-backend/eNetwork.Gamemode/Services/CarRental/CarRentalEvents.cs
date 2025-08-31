using eNetwork.Framework.API.InteractionDepricated.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Services.CarRental
{
    internal class CarRentalEvents
    {
        [InteractionDeprecated(ColShapeType.CarRental, InteractionType.Key)]
        private void OnInteractionPressKeyWithCarRentalPoint(ENetPlayer player)
        {
            if (!player.GetData(nameof(CarRentalPoint), out CarRentalPoint rentalPoint))
                return;

            rentalPoint.OpenDialogWithPed(player);
        }

        [CustomEvent("server.car_rental.buy_rent")]
        public static void BuyRentEventHandler(ENetPlayer player, string model, int hours, int colorIndex)
        {
            CarRentalModel rentalModel = CarRentalRepository.Instance.CarRentalModels.FirstOrDefault(m => m.ModelName == model);
            if (rentalModel == null)
            {
                player.SendError("Данной модели для аренды нет");
                return;
            }

            if (CarRentalService.Instance.Config.ServiceConfig.Colors.Count <= colorIndex || colorIndex < 0)
            {
                player.SendError("Выбран неверный цвет");
                return;
            }

            CarRentalService.Instance.RentACar(player, hours, rentalModel, CarRentalService.Instance.Config.ServiceConfig.Colors[colorIndex]);
        }
    }
}
