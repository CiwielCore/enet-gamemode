using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Player.Leveling
{
    public static class Exp
    {
        public static void Up(ENetPlayer player)
        {
            var data = player.CharacterData;
            var forNextLevel = expForNextLevel(data.Lvl);
            data.Exp++;
            if(data.Exp >= forNextLevel)
            {
                data.Exp = 0;
                data.Lvl++;
            }
            Panel.Init.UpdatePart(player, new Dictionary<string, object>()
            {
                { "lvl", data.Lvl },
                {"exp", data.Exp }
            });
        }
        private static int expForNextLevel(int currentLevel)
        {
            return currentLevel * 4;
        }
    }
}
