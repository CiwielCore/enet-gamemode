using GTANetworkAPI;

namespace eNetwork.Framework
{
    public class ServerConfig
    {
        public string Authors { get; set; } = "Edison";
        public string ServerName { get; set; }
        public ServerMode Mode { get; set; } = ServerMode.Dev;
        public int ServerNumber { get; set; }
        public string UIUrl { get; set; }
        public string DiscordToken { get; set; }
        public long DisocrdGuildID { get; set; }
        public string TelegramToken { get; set; }
        public bool IsWhitelist { get; set; }
        public long TelegramChatID { get; set; }
        public Vector3[] SpawnPositions = { 
            new Vector3(-1042.7578, -2746.206, 21.35938),
        }; 

        public enum ServerMode
        {
            Dev, Game
        }
    }
}
