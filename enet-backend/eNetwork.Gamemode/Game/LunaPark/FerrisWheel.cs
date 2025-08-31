using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.Framework;
using eNetwork.GameUI;
using eNetwork.Modules;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace eNetwork.Game.LunaPark
{
    public class FerrisWheel : Script
    {
        private readonly static Logger Logger = new Logger("FerrisWheel");
        public static List<FerrisCab> ferrisCabs = new List<FerrisCab>();
        private static Vector3 _ferrisPosition = new Vector3(-1666.487, -1126.9152, 12.572337); 
        public static void Initialize()
        {
            try
            {
                for (int i = 0; i < 16; i++)
                    ferrisCabs.Add(new FerrisCab());

                NAPI.Blip.CreateBlip(266, _ferrisPosition, .9f, 4, "Колесо обозрения", 255, 0, true, 0, 0);
                NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, _ferrisPosition, new Vector3(), new Vector3(), .6f, Helper.GTAColor, false, 0);
            }
            catch(Exception e) { Logger.WriteError("Initialize", e); }
        }
        public static FerrisCab GetFerrisCab(int index)
        {
            if (ferrisCabs.Count <= index || index < 0) return null;
            return ferrisCabs[index];
        }
        public static void LoadFerrisWheel(ENetPlayer player)
        {
            try
            {
                List<int> players = new List<int>();
                foreach (FerrisCab cab in ferrisCabs)
                    players.Add(cab.PlayerID);

                ClientEvent.Event(player, "client.lunapark.ferris.init", JsonConvert.SerializeObject(players));
            }
            catch(Exception e) { Logger.WriteError("OnPlayerJoin", e); }
        }

        [CustomEvent("server.lunapark.ferris.enter")]
        private void EnterFerris(ENetPlayer player, params object[] args)
        {
            try
            {
                if (!player.IsTimeouted("FERRIS_ENTER", 1) || !Helper.IsInRangeOfPoint(player.Position, _ferrisPosition, 10)) return;

                int index = Convert.ToInt32(args[0]);
                FerrisCab cab = GetFerrisCab(index);

                if (cab is null) return;
                if (cab.IsOccupied)
                {
                    player.SendError(Language.GetText(TextType.FerrisCabTaken, index + 1));
                    return;
                }
                cab.IsOccupied = true;
                player.SetData("FERRIS_CABINE", index);

                Transition.Start(player, 600, 1000, 600);

                

                Timers.StartOnceTask(1000, () => {
                    try
                    {
                        if (player != null)
                        {
                            cab.PlayerID = player.Value;
                            ClientEvent.Event(player, "client.lunapark.ferris.seat", index);
                            ClientEvent.EventInRange(_ferrisPosition, Helper.DrawDistance, "client.lunapark.ferris.syncAtt", true, index, player.Value);
                        }
                    }
                    catch (Exception e) { Logger.WriteError("EtnerFerris.TaskRun", e); }
                });
            }
            catch(Exception ex) { Logger.WriteError("EnterFerris", ex); }
        }

        [CustomEvent("server.lunapark.ferris.leave")]
        private void LeaveFerris(ENetPlayer player)
        {
            try
            {
                if (!player.IsTimeouted("FERRIS_LEAVE", 1) || !Helper.IsInRangeOfPoint(player.Position, _ferrisPosition, 30) || !player.HasData("FERRIS_CABINE")) return;
                int cabIndex = player.GetData<int>("FERRIS_CABINE");
                FerrisCab cab = GetFerrisCab(cabIndex);
                
                if (cab is null || !cab.IsOccupied) return;

                cab.IsOccupied = false;
                cab.PlayerID = -1;
                ClientEvent.EventInRange(player.Position, Helper.DrawDistance, "client.lunapark.ferris.syncAtt", false, cabIndex, player.Value);
                ClientEvent.Event(player, "client.lunapark.ferris.deattach");
                NAPI.Entity.SetEntityPosition(player, _ferrisPosition);
            }
            catch (Exception e) { Logger.WriteError("LeaveFerris", e); }
        }
    }
}
