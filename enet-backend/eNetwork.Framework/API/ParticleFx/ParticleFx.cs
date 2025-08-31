using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eNetwork.Framework;
using GTANetworkAPI;

namespace eNetwork.API.ParticleFx
{
    public class ParticleFx
    {
        public static void PlayFXonPos(Vector3 pos, float range, float xPos, float yPos, float zPos, string fxLib, string fxName, int dellTime = 1000, ParticleFxData data = null)
        {
            ClientEvent.EventInRange(pos, range, "client.playFXonPos", xPos, yPos, zPos, fxLib, fxName, dellTime, data);
        }
        public static void PlayFXonPosOnce(Vector3 pos, float range, float xPos, float yPos, float zPos, string fxLib, string fxName, int dellTime = 1000, ParticleFxData data = null)
        {
            ClientEvent.EventInRange(pos, range, "client.playFXonPosOnce", xPos, yPos, zPos, fxLib, fxName, dellTime, data);
        }
        public static void PlayFXonEntity(Vector3 pos, float range, Entity entity, string fxLib, string fxName, int dellTime = 1000, ParticleFxData data = null)
        {
            ClientEvent.EventInRange(pos, range, "client.playFXonEntity", entity, fxLib, fxName, dellTime, data);
        }
        public static void PlayFXonEntityOnce(Vector3 pos, float range, Entity entity, string fxLib, string fxName, ParticleFxData data = null)
        {
            ClientEvent.EventInRange(pos, range, "client.playFXonEntityOnce", entity, fxLib, fxName, data);
        }
        public static void PlayFXonEntityBone(Vector3 pos, float range, Entity entity, int boneName, string fxLib, string fxName, int dellTime = 1000, ParticleFxData data = null)
        {
            ClientEvent.EventInRange(pos, range, "client.playFXonEntityBone", entity, boneName, fxLib, fxName, dellTime, data);
        }
    }
}
