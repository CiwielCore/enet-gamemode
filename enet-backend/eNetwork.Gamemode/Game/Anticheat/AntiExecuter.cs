using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Anticheat
{
    internal class AntiExecuter
    {
        [CustomEvent("server.events.detected")]
        public static void Detected(ENetPlayer player, string data)
        {
            player.ExtKick($"Игрок {player.Name} был нахуй кикнут по причине: " + data);
        }
    }
}
