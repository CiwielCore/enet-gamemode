using System;
using GTANetworkAPI;

namespace eNetwork.Gambles.Lotteries.LotteryModels
{
    public class LotteryPrizeDTO
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return NAPI.Util.ToJson(this);
        }
    }
}
