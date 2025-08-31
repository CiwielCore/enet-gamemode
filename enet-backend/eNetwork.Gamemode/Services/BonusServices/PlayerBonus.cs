using System.Collections.Generic;

namespace eNetwork.Services.BonusServices
{
    public class PlayerBonus
    {
        public int UUID { get; set; }
        public List<bool[]> BonusDays { get; set; }
        public bool[] DailyBonus { get; set; }
        public bool[] CarBonus { get; set; }
        public List<BonusItem> Storage { get; set; }
        public PlayerBonus()
        {
            BonusDays = new List<bool[]>() { new bool[365], new bool[50] };
            DailyBonus = new bool[10];
            CarBonus = new bool[10];
            Storage = new List<BonusItem>();
        }
    }
}
