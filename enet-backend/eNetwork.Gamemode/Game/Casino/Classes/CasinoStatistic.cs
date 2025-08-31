using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Casino.Classes
{
    public class CasinoStats
    {
        public int TotalGames { get; set; }
        public int Wins { get; set; } = 0;
        public int Earn { get; set; } = 0;
        public int Spent { get; set; } = 0;
    }
}
