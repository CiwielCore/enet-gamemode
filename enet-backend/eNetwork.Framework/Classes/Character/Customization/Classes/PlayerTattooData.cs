using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.Classes.Character.Customization
{
    public class PlayerTattooData
    {
        public string Collection { get; set; }
        public string Overlay { get; set; }

        public PlayerTattooData(string collection, string overlay)        {
            Collection = collection;
            Overlay = overlay;
        }
    }
}
