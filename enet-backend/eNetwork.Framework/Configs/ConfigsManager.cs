using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.Configs
{
    public class ConfigsManager
    {
        public static void Initialize()
        {
            Tattoo.TattooConfig.Initialize();
        }
    }
}
