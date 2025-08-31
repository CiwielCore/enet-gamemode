using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eNetwork.Framework;
using GTANetworkAPI;

namespace eNetwork.Framework.API.Vehicles
{
    public class Offroad
    {
        private static readonly uint[] OffroadMaterials = new uint[]
        {
            2409420175,
            951832588,
            3008270349,
            3454750755,
            2128369009,
            2699818980,
            3833216577,
            1913209870,
            127813971,
            2253637325,
            581794674,
            2352068586,
            338165457,
            3594309083,
            510490462,
            2379541433,
            1333033863
        };
        private static readonly uint[] OffroadCars = new uint[]
        {
            NAPI.Util.GetHashKey("g63amg"),
            NAPI.Util.GetHashKey("rebel"),
        };
        public void Initialize(ENetPlayer player)
        {
            ClientEvent.Event(player, "client.offroad.setConfig", OffroadMaterials, OffroadCars);
        }
    }
}
