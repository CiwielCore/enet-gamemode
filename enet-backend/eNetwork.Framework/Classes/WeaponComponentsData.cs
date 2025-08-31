using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork
{
    public class WeaponComponentsData
    {
        public int TintIndex { get; set; } = 0;
        public bool Clip { get; set; } = false;
        public bool Grip { get; set; } = false;
        public bool Scope { get; set; } = false ;
        public bool HolographicSight { get; set; } = false;
        public bool Suppressor { get; set; } = false;
        public bool Flashlight { get; set; } = false;
    }
}
