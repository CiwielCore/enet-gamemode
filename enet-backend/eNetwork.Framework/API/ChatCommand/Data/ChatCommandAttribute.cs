using System;
using eNetwork.Framework;

namespace eNetwork
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ChatCommandAttribute : Attribute
    {
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string Description { get; set; }
        public bool GreedyArg { get; set; } = false;
        public PlayerRank Access { get; set; } = PlayerRank.Player;
        /// <summary>
        /// Создать команду на сервер
        /// </summary>
        /// <param name="command">Название команды</param>
        /// <param name="description">Описание команды</param>
        /// <param name="access">Доступ для использования команды</param>
        public ChatCommandAttribute(string command, string description = "", PlayerRank access = PlayerRank.Player)
        {
            Command = command; 
            Description = description; 
            Access = access;
        }
    }
}
