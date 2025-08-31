using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;

namespace eNetwork.Framework.API.Blip
{
    public class BlipManager
    {
        public BlipManager() { }
        public ENetBlip CreateBlip(uint sprite, Vector3 position, float scale, byte color, string name = "", byte alpha = 255, float drawDistance = 0, bool shortRange = false, short rotation = 0, uint dimension = uint.MaxValue)
        {
            return (ENetBlip)NAPI.Blip.CreateBlip(sprite, position, scale, color, name, alpha, drawDistance, shortRange, rotation, dimension);
        }

        public BlipInformation GenerateInformation(string name, string description, string picture = "default", BlipInfoType type = BlipInfoType.Default, dynamic extra = null)
        {
            return new BlipInformation()
            {
                Name = name,
                Description = description,
                Picture = picture,
                Type = type.ToString(),
                ExtraData = extra
            };
        }
    }
}
