using eNetwork.Framework;
using eNetwork.Framework.Singleton;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.GameUI
{
    class Documents : Singleton<Documents>
    {
        private Documents()
        {

        }

        public void ShowIdCard(ENetPlayer toWhom, ENetPlayer whose)
        {
            object idCard = (object)new
            {
                name = whose.GetName(),
                serNum = whose.GetUUID(),
                gender = whose.Gender.ToString("G"),
                britDate = whose.GetCharacter().BirthDay.ToShortDateString()
            };

            ClientEvent.Event(toWhom, "client.documents.show_idCard", NAPI.Util.ToJson(idCard));
        }
    }
}
