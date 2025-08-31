using System;
using System.Collections.Generic;
using eNetwork.Game.Vehicles;

namespace eNetwork.Gambles.Lotteries
{
    internal class LotteryPrizeGiver
    {
        private readonly Dictionary<LotteryPrizeType, Action<ENetPlayer, LotteryPrize>> _givers;

        public LotteryPrizeGiver()
        {
            _givers = new Dictionary<LotteryPrizeType, Action<ENetPlayer, LotteryPrize>>
            {
                { LotteryPrizeType.Money, GiveMoneyPrize },
                { LotteryPrizeType.Vehicle, GiveVehiclePrize }
            };
        }

        public void GivePrize(ENetPlayer player, LotteryPrize prize)
        {
            if (prize.Type == LotteryPrizeType.None)
            {
                return;
            }

            if (_givers.ContainsKey(prize.Type))
                _givers[prize.Type].DynamicInvoke(player, prize);
        }

        private void GiveMoneyPrize(ENetPlayer player, LotteryPrize prize)
        {
            int money = Convert.ToInt32(prize.Value);
            player.ChangeWallet(money);
            player.SendInfo($"Вы выиграли с лотерейного билета ${money}");
        }

        private void GiveVehiclePrize(ENetPlayer player, LotteryPrize prize)
        {
            string modelName = prize.Value;
            int vehicleId = VehicleManager.CreateVehicle(new VehicleOwner(OwnerVehicleEnum.Player, player.GetUUID()), modelName, 0, 0, true).GetAwaiter().GetResult();
            VehicleData vehicleData = VehicleManager.GetVehicleData(vehicleId);
            player.SendInfo($"Вы выиграли с лотерейного билета {vehicleData.Model}. Вам выдан номер {vehicleData.NumberPlate}");
        }
    }
}