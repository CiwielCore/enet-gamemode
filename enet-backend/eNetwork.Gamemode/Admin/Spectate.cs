using eNetwork.Commands;
using eNetwork.Framework;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Admin
{
    public class Spectate
    {
        private static readonly Logger Logger = new Logger("spectate");

        [ChatCommand("sp")]
        public static void On(ENetPlayer player, int staticId)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByUUID(staticId);
                if (target is null)
                {
                    player.SendError($"Игрок #{staticId} не найден!");
                    return;
                }

                if (target == player || !player.GetSessionData(out var sessionData) || !target.GetSessionData(out var targetSessionData)) return;

                if (sessionData.AdminData.IsSpectating)
                {
                    player.SendError("Вы уже наблюдаете за кем-то");
                    return;
                }

                if (targetSessionData.AdminData.IsSpectating)
                {
                    player.SendError("Администратор наблюдает за кем-то");
                    return;
                }

                sessionData.AdminData.IsSpectating = true;
                sessionData.AdminData.SpectateDimension = player.Dimension;
                player.CharacterData.ExteriosPosition = player.Position;

                player.Transparency = 0;
                player.Position = new Vector3(target.Position.X, target.Position.Y, (target.Position.Z - 10));
                player.Dimension = target.Dimension;

                ClientEvent.Event(player, "client.spectate", target, true);
            }
            catch (Exception ex) { Logger.WriteError("On", ex); }
        }

        [CustomEvent("client.un_spectate")]
        [ChatCommand("spoff")]
        public static void UnSpectate(ENetPlayer player)
        {
            try
            {
                if (!player.GetSessionData(out var sessionData)) return;

                if (!sessionData.AdminData.IsSpectating)
                {
                    player.SendError("Вы не за кем не наблюдаете!");
                    return;
                }

                sessionData.AdminData.IsSpectating = false;

                player.Transparency = 255;
                player.Position = player.CharacterData.ExteriosPosition;
                player.Dimension = sessionData.AdminData.SpectateDimension;
                player.CharacterData.ExteriosPosition = null;

                sessionData.AdminData.SpectateDimension = 0;

                ClientEvent.Event(player, "client.spectate", null, false);
            }
            catch (Exception e) { Logger.WriteError("UnSpectate", e); }
        }
    }
}
