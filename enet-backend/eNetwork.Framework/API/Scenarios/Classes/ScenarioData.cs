using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.Scenarios.Classes
{
    public class ScenarioData
    {
        public ScenarioType Type { get; set; }
        public string Dictionary { get; set; }
        public string Name { get; set; }
        public int Flag { get; set; }
        public bool IsLooped { get; set; }
        public bool IsClientSync { get; set; }  
        public bool StopOnEnd { get; set; }
        public double StopDuration { get; set; } = .95;

        public ScenarioData(string dictionary, string name, int flag, bool isLooped = false, bool isClientSync = false, bool stopOnEnd = true, double stopDuration = .95)
        {
            Dictionary = dictionary;
            Name = name;
            Flag = flag;
            IsLooped = isLooped;
            IsClientSync = isClientSync;
            StopOnEnd = stopOnEnd;
            StopDuration = stopDuration;
        }
    }
}
