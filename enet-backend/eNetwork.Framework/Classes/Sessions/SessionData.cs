using eNetwork.Framework.Classes.Sessions.Classes;
using System.Collections.Generic;
using static eNetwork.Configs.AnimationsConfig;

namespace eNetwork
{
    public class SessionData
    {
        public WorkData WorkData { get; set; } = new WorkData();
        public AdminData AdminData { get; set; } = new AdminData();
        public int EnteredHouse { get; set; } = -1;
        public TimersData TimersData { get; set; } = new TimersData();
        public List<int> SmugglingPoints { get; set; } = new List<int>();

        public AnimationData AnimationData { get; set; } = new AnimationData();
    }
}
