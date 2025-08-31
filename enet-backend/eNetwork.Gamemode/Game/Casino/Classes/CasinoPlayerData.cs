using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Casino.Classes
{
    public class CasinoPlayerData
    {
        public int Uuid { get; set; } = -1;
        public long Chips { get; set; } = 0;
        public CasinoStats Roulette { get; set; } = new CasinoStats();
        public CasinoStats BlackJack { get; set; } = new CasinoStats();
        public CasinoStats Horse { get; set; } = new CasinoStats();
        public CasinoStats Slots { get; set; } = new CasinoStats();
        public CasinoStats Poker { get; set; } = new CasinoStats();
        public DateTime LuckyWheel { get; set; } = DateTime.Now;
    }
}
