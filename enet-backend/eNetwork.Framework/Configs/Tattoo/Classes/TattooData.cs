using eNetwork.Framework.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.Configs.Tattoo.Classes
{
    public class TattooData
    {
        public string Name { get; set; }
        public string Collection { get; set; }
        public string OverlayMale { get; set; }
        public string OverlayFemale { get; set; }
        public int Price { get; set; }
        public bool IsDonate { get; set; }

        public TattooData(string name, string collection, string overlayMale, string overlayFemale, int price, bool isDonate)
        {
            Name = name;
            Collection = collection;
            OverlayMale = overlayMale;
            OverlayFemale = overlayFemale;
            Price = price;
            IsDonate = isDonate;
        }
    }
}
