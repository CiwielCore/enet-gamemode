using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Framework.API.SceneManager.SceneAction;
using eNetwork.Game.Casino.Classes;
using GTANetworkAPI;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Game.Casino
{
    public static class CasinoManager
    {
        private static readonly Logger Logger = new Logger("casino-manager");
        private static ConcurrentDictionary<int, CasinoPlayerData> _playersData = new ConcurrentDictionary<int, CasinoPlayerData>();

        public static readonly uint CasinoDimension = 7777777;
        public static readonly Position СasinoExteriorPosition = new Position(924.0211, 46.933903, 81.20635, 60);
        public static readonly Position СasinoInteriorPosition = new Position(1089.695, 206.015, -49, -24);

        public static void Initialize()
        {
            try
            {
                DataTable data = ENet.Database.ExecuteRead("SELECT * FROM `casino_players`");
                if (data != null && data.Rows.Count != 0)
                {
                    foreach(DataRow row in data.Rows)
                    {
                        var playerData = new CasinoPlayerData();
                        playerData.Uuid = Convert.ToInt32(row["uuid"]);
                        playerData.Chips = Convert.ToInt64(row["chips"]);
                        playerData.Roulette = JsonConvert.DeserializeObject<CasinoStats>(row["roulette"].ToString());
                        playerData.BlackJack = JsonConvert.DeserializeObject<CasinoStats>(row["blackjack"].ToString());
                        playerData.Horse = JsonConvert.DeserializeObject<CasinoStats>(row["horse"].ToString());
                        playerData.Slots = JsonConvert.DeserializeObject<CasinoStats>(row["slots"].ToString());
                        playerData.Poker = JsonConvert.DeserializeObject<CasinoStats>(row["poker"].ToString());
                        playerData.LuckyWheel = (DateTime)row["luckywheel"];

                        _playersData.TryAdd(playerData.Uuid, playerData);
                    }
                }
                GTAElements();

                Logger.WriteInfo($"Загружено {_playersData.Count} данных о игроках в казино");
                LuckyWheel.LuckyWheelManager.Inititlaize();
            }
            catch(Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static void GTAElements()
        {
            try
            {
                var colShape = ENet.ColShape.CreateCylinderColShape(СasinoExteriorPosition.GetVector3(), 1.5f, 2, 0, ColShapeType.CasinoHall);
                colShape.InteractionText = "Нажмите чтобы войти в здание Казино";

                colShape = ENet.ColShape.CreateCylinderColShape(СasinoInteriorPosition.GetVector3(), 1.5f, 2, CasinoDimension, ColShapeType.CasinoHall);
                colShape.InteractionText = "Нажмите чтобы покинуть здание";

                NAPI.Marker.CreateMarker(1, СasinoInteriorPosition.GetVector3() - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), 0.5f, Helper.GTAColor, false, CasinoDimension);
                NAPI.Marker.CreateMarker(1, СasinoExteriorPosition.GetVector3() - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), 0.5f, Helper.GTAColor, false, 0);

                var blip = ENet.Blip.CreateBlip(679, СasinoExteriorPosition.GetVector3(), 0.9f, 4, "Казино Diamond", 255, 0, true, 0, 0);
            }
            catch(Exception ex) { Logger.WriteError("GTAElements", ex); }
        }

        [InteractionDeprecated(ColShapeType.CasinoHall)]
        public static void OnInteraction(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;
                if (player.Position.DistanceTo(СasinoExteriorPosition.GetVector3()) < 10)  // Enter caisno
                {
                    ENet.SceneManager.Start(player, "casino_enter");
                }
                else
                {
                    ClientEvent.Event(player, "client.casino.leaveInterior");
                }
            }
            catch(Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }

        [CustomEvent("server.casino.leaveInterior")]
        public static void EndCasinoLeaveScene(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;
                if (player.Position.DistanceTo(СasinoInteriorPosition.GetVector3()) > 10) return;

                СasinoExteriorPosition.Set(player);
                player.SetDimension(0);

                characterData.ExteriosPosition = null;
            }
            catch(Exception ex) { Logger.WriteError("EndCasinoLeaveScene", ex); }
        }

        [SceneAction(SceneActionType.CasinoEnter)]
        public static void EndCasinoEnterScene(ENetPlayer player, string sceneName)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;
                if (player.Position.DistanceTo(СasinoExteriorPosition.GetVector3()) > 20) return;

                СasinoInteriorPosition.Set(player);
                player.SetDimension(CasinoDimension);

                characterData.ExteriosPosition = СasinoExteriorPosition.GetVector3();
                ClientEvent.Event(player, "client.casino.enterInterior");
            }
            catch(Exception ex) { Logger.WriteError("EndCasinoEnterScene", ex); }
        }

        public static bool GetCasinoData(this ENetPlayer player, out CasinoPlayerData casinoPlayerData)
        {
            return _playersData.TryGetValue(player.GetUUID(), out casinoPlayerData);
        }

        public static bool ChangeChips(this ENetPlayer player, long chips)
        {
            if (!player.GetCasinoData(out var casinoPlayerData)) return false;
            if (casinoPlayerData.Chips + chips < 0) return false;

            casinoPlayerData.Chips += chips;
            player.SetSharedData("player.casino.chips", casinoPlayerData.Chips);
            return true;
        }

        public static void LoadCasinoData(this ENetPlayer player)
        {
            try
            {
                if (!player.GetCasinoData(out var playerData))
                {
                    playerData = new CasinoPlayerData();
                    playerData.Uuid = player.GetUUID();

                    ENet.Database.Execute($"INSERT INTO `casino_players` (`uuid`,`chips`,`roulette`,`blackjack`,`horse`,`slots`,`poker`,`luckywheel`) " +
                        $"VALUES ({playerData.Uuid}, {playerData.Chips}, '{JsonConvert.SerializeObject(playerData.Roulette)}', '{JsonConvert.SerializeObject(playerData.BlackJack)}', '{JsonConvert.SerializeObject(playerData.Horse)}', " +
                        $"'{JsonConvert.SerializeObject(playerData.Slots)}', '{JsonConvert.SerializeObject(playerData.Poker)}', '{Helper.ConvertTime(playerData.LuckyWheel)}')");
                    _playersData.TryAdd(playerData.Uuid, playerData);
                }

                player.SetSharedData("player.casino.chips", playerData.Chips);
            }
            catch(Exception ex) { Logger.WriteError("LoadCasinoData", ex); }
        }

        public static async Task Save(ENetPlayer player)
        {
            try
            {
                if (!player.GetCasinoData(out var casinoPlayerData)) return;

                MySqlCommand sqlCommand = new MySqlCommand("UPDATE `casino_players` SET `chips`=@CHIPS, `roulette`=@ROULETTE, `blackjack`=@BLACKJACK, `horse`=@HORSE, `slots`=@SLOTS, `poker`=@POKER, `luckywheel`=@WHEEL WHERE `uuid`=@UUID");
                sqlCommand.Parameters.AddWithValue("@UUID", casinoPlayerData.Uuid);
                sqlCommand.Parameters.AddWithValue("@CHIPS", casinoPlayerData.Chips);
                sqlCommand.Parameters.AddWithValue("@ROULETTE", JsonConvert.SerializeObject(casinoPlayerData.Roulette));
                sqlCommand.Parameters.AddWithValue("@BLACKJACK", JsonConvert.SerializeObject(casinoPlayerData.BlackJack));
                sqlCommand.Parameters.AddWithValue("@HORSE", JsonConvert.SerializeObject(casinoPlayerData.Horse));
                sqlCommand.Parameters.AddWithValue("@SLOTS", JsonConvert.SerializeObject(casinoPlayerData.Slots));
                sqlCommand.Parameters.AddWithValue("@POKER", JsonConvert.SerializeObject(casinoPlayerData.Poker));
                sqlCommand.Parameters.AddWithValue("@WHEEL", Helper.ConvertTime(casinoPlayerData.LuckyWheel));

                await ENet.Database.ExecuteAsync(sqlCommand);
            }
            catch(Exception ex) { Logger.WriteError("SaveCasinoData", ex); }
        }

        public static void SetWallType(string type)
        {
            try
            {
                ClientEvent.EventInRange(new Vector3(1111.052, 229.8579, -49.133), 100f, "client.casino.wall.set", type);
                if (type == "winner")
                    Timers.StartOnce(8500, () => SetWallType("diamonds"));
            }
            catch(Exception ex) { Logger.WriteError("SetWallType", ex); }
        }
    }
}
