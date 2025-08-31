using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Admin
{
    public class AdminsManager
    {
        public static List<ENetPlayer> GetAdmins()
            => ENet.Pools.GetAllRegisteredPlayers().Where(x => x.CharacterData.Status > PlayerRank.Media).ToList();

        public static void SendNotify(string text, int time = 3000) =>
            GetAdmins().ForEach(admin => admin.SendInfo(text, time));

        public static void SendChat(string text, ChatType chatType = ChatType.Admin) =>
          GetAdmins().ForEach(admin => ENet.Chat.SendMessage(admin, text));
    }
}
