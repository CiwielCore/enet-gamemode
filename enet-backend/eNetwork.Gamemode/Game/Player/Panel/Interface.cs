using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Player.Panel
{
    public static class Interface
    {
        public static void SendData(ENetPlayer player, string path, string data)
        {
            ClientEvent.Event(player, "client.setStoreData", path, data);
        }
    }
}
