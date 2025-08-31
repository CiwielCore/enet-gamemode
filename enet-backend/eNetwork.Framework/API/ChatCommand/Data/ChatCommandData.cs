using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.ChatCommand.Data
{
    public class ChatCommandData
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public string Arguments { get; set; }
        public PlayerRank Access { get; set; }

        public ChatCommandData(string command, string description, string args, PlayerRank playerRank)
        {
            Command = command;
            Description = description;
            Arguments = args;
            Access = playerRank;
        }
    }
}
