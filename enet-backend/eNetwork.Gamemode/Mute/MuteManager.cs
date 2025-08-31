using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.Framework;
using GTANetworkMethods;
using eNetwork.Mute.Models;
using eNetwork.Mute.Commands;

namespace eNetwork.Mute
{

    public class MuteManager
    {
        public static void UpdatingTheTimeOfMute(ENetPlayer player)
        {
            if (player.CharacterData is null)
                return;

            if (!MuteRepository.Instance.IsCharacterInMute(player.CharacterData.UUID)) 
                return;

            MuteInfo muteInfo = MuteRepository.Instance.GetMuteInfo(player.CharacterData.UUID);

            if (--muteInfo.MinutesLeft <= 0)
            {
               MuteCommands muteCommands = new MuteCommands();
                muteCommands.RemoveMuteThePlayer(player);
            }
        }
    }
}
