using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GTANetworkAPI;

namespace eNetwork
{
    public class ENetBlip : Blip
    {
        public ENetBlip(NetHandle handle) : base(handle)
        {

        }

        public void SetInformation(BlipInformation data)
        {
            SetSharedData("BLIP_INFO_DATA", JsonConvert.SerializeObject(data));
        }
    }
}
