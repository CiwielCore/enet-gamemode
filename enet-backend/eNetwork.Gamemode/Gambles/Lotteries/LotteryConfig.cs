using System.Collections.Generic;

namespace eNetwork.Gambles.Lotteries
{
    class LotteryConfig
    {
        public uint TicketsForDay { get; set; }
        public LotteryTime UpdateTicketsTime { get; set; }
        public List<LotteryPrize> Prizes { get; set; }
    }
}