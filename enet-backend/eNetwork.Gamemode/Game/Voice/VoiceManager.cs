using eNetwork.Framework;
using GTANetworkMethods;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Voice
{
    public class VoiceManager
    {
        private static readonly Logger Logger = new Logger("voice-manager");
        [CustomEvent("server.voice.addListener")]
        public void AddListener(ENetPlayer player, params object[] arguments)
        {
            try
            {
                if (player.GetCharacter() is null || arguments.Length == 0 || !(arguments[0] is ENetPlayer)) return;
                ENetPlayer target = (ENetPlayer)arguments[0];
                if (target.GetCharacter() is null) return;
                player.EnableVoiceTo(target);
            }
            catch (Exception e) { Logger.WriteError("AddListener", e); }
        }

        [CustomEvent("server.voice.removeListener")]
        public void RemoveListener(ENetPlayer player, params object[] arguments)
        {
            try
            {
                if (player.GetCharacter() is null || arguments.Length == 0 || !(arguments[0] is ENetPlayer)) return;
                ENetPlayer target = null;
                try { target = (ENetPlayer)arguments[0]; } catch { }

                if (target is null || target.GetCharacter() is null) return;
                player.DisableVoiceTo(target);
            }
            catch (Exception e) { Logger.WriteError("AddListener", e); }
        }
    }
}
