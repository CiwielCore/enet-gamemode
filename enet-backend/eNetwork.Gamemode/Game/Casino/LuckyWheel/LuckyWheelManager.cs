using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.GameUI;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static eNetwork.Game.Casino.LuckyWheel.LuckyWheelManager;

namespace eNetwork.Game.Casino.LuckyWheel
{
    public class LuckyWheelManager
    {
        private static readonly Logger Logger = new Logger("luckywheel-mngr");
        private static bool _isWheelSpinning = false;

        private static List<LuckyWheelItem> _prizes = new List<LuckyWheelItem>()
        {
            // Car
            new LuckyWheelItem(LuckyWheelItemType.Vehicle, 18,  chance: 0, "Транспорт", "comet6"),

            // Money
            new LuckyWheelItem(LuckyWheelItemType.Money, 2,     chance: 60, "Деньги", "1000_10000"),
            new LuckyWheelItem(LuckyWheelItemType.Money, 6,     chance: 70, "Деньги", "1000_10000"),
            new LuckyWheelItem(LuckyWheelItemType.Money, 14,    chance: 80, "Деньги", "1000_10000"),
            new LuckyWheelItem(LuckyWheelItemType.Money, 19,    chance: 90, "Деньги", "1000_10000"),

            // Clothes
            new LuckyWheelItem(LuckyWheelItemType.Clothes, 0,   chance: 20, "экслюзивную одежду Diamond Casino", "0"),
            new LuckyWheelItem(LuckyWheelItemType.Clothes, 8,   chance: 20, "экслюзивную одежду Diamond Casino", "0"),
            new LuckyWheelItem(LuckyWheelItemType.Clothes, 12,  chance: 20, "экслюзивную одежду Diamond Casino", "0"),
            new LuckyWheelItem(LuckyWheelItemType.Clothes, 16,  chance: 20, "экслюзивную одежду Diamond Casino", "0"),

            // Reroll
            new LuckyWheelItem(LuckyWheelItemType.Reroll, 1,    chance: 25, "еще один Шанс прокрутить колесо в Колесе удачи", "1"),
            new LuckyWheelItem(LuckyWheelItemType.Reroll, 5,    chance: 25, "еще один Шанс прокрутить колесо в Колесе удачи", "1"),
            new LuckyWheelItem(LuckyWheelItemType.Reroll, 9,    chance: 25, "еще один Шанс прокрутить колесо в Колесе удачи", "1"),
            new LuckyWheelItem(LuckyWheelItemType.Reroll, 13,   chance: 25, "еще один Шанс прокрутить колесо в Колесе удачи", "1"),
            new LuckyWheelItem(LuckyWheelItemType.Reroll, 17,   chance: 30, "еще один Шанс прокрутить колесо в Колесе удачи", "1"),

            // Casino Chips
            new LuckyWheelItem(LuckyWheelItemType.Chips, 3,     chance: 10, "Игральные фишки", "75_150"),
            new LuckyWheelItem(LuckyWheelItemType.Chips, 7,     chance: 7, "Игральные фишки", "125_300"),
            new LuckyWheelItem(LuckyWheelItemType.Chips, 10,    chance: 4, "Игральные фишки", "250_400"),
            new LuckyWheelItem(LuckyWheelItemType.Chips, 15,    chance: 2, "Игральные фишки", "500_1000"),

            // Donate
            new LuckyWheelItem(LuckyWheelItemType.Donate, 4, chance : 2, "Донатная валюта", "100_700"),

            // Mystical Item
            new LuckyWheelItem(LuckyWheelItemType.Mystical, 11, chance : 2, "Мистический кейс", "1"),
        };

        public static void Inititlaize()
        {
            try
            {
                var colShape = ENet.ColShape.CreateCylinderColShape(new Vector3(1110.8710, 228.8737, -49.6358), 1.5f, 2, CasinoManager.CasinoDimension, ColShapeType.CasinoLuckyWheel);
                colShape.SetInteractionText("Нажмите чтобы узнать информацию о колесе удачи");

                _prizes = _prizes.OrderBy(x => x.Chance).Reverse().ToList();
            }
            catch(Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        [InteractionDeprecated(ColShapeType.CasinoLuckyWheel)]
        public static void OnInteraction(ENetPlayer player)
        {
            try
            {
                if (!player.GetCasinoData(out var casinoPlayerData)) return;
                Event_BuySpins(player, "Free");
            }
            catch(Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }

        [CustomEvent("server.casino.luckyWheel.buy")]
        public static void Event_BuySpins(ENetPlayer player, string type)
        {
            try
            {
                if (!player.GetAccountData(out var accountData) || !player.GetCasinoData(out var casinoPlayerData)) return;

                if (_isWheelSpinning)
                {
                    player.SendInfo("Кто-то уже крутит колесо, подождите!");
                    return;
                }

                switch(type)
                {
                    case "Free":
                        if (casinoPlayerData.LuckyWheel > DateTime.Now)
                        {
                            TimeSpan difference = casinoPlayerData.LuckyWheel - DateTime.Now; ;
                            string timeLeft = difference.Hours >= 1 ? $"{difference.Hours} час" + (difference.Hours == 2 || difference.Hours == 3 || difference.Hours == 4 ? "a" : "ов") : $"{difference.Minutes} минут";
                            player.SendError($"Вы сможете прокрутить колесо через {timeLeft}");
                            return;
                        }
                        break;
                    case "Buy":

                        break;
                }

                int prizeIndex = GetPrizeIndex();
                player.SetData("luckyWheel.prize", prizeIndex);

                _isWheelSpinning = true;

                player.SmoothResetAnim();
                ClientEvent.Event(player, "client.casino.luckyWheel.comeTo", prizeIndex);
            }
            catch(Exception ex) { Logger.WriteError("Event_BuySpins", ex); }
        }

        [CustomEvent("server.casino.luckyWheel.spin")]
        public static void Event_StartSpin(ENetPlayer player)
        {
            try
            {
                if (!player.IsTimeouted("luckyWheel.spin", 1) || !player.GetData("luckyWheel.prize", out int prizeIndex) || !player.GetCharacter(out var characterData) || !player.GetCasinoData(out var casinoPlayerData)) return;
                ClientEvent.EventInRange(player.Position, 100, "client.casino.luckyWheel.spin", prizeIndex);
            }
            catch(Exception ex) { Logger.WriteError("Event_StartSpin", ex); }
        }

        public static int GetPrizeIndex()
        {
            int pool = 0;
            for (int i = 0; i < _prizes.Count; i++)
            {
                pool += _prizes[i].Chance;
            }

            int random = ENet.Random.Next(pool);

            int accumulatedProbability = 0;
            for (int i = 0; i < _prizes.Count; i++)
            {
                accumulatedProbability += _prizes[i].Chance;
                if (random <= accumulatedProbability)
                    return _prizes[i].Index;
            }   
            return 19;
        }

        [CustomEvent("server.casino.luckyWheel.finish")]
        public static void Event_EndSpin(ENetPlayer player)
        {
            try
            {
                if (!_isWheelSpinning || !player.IsTimeouted("luckyWheel.finish", 1) || !player.GetData("luckyWheel.prize", out int prizeIndex) || !player.GetCharacter(out var characterData) || !player.GetCasinoData(out var casinoPlayerData)) return;

                var prizeData = _prizes.Find(x => x.Index == prizeIndex);
                if (prizeData is null)
                {
                    player.SendError($"Вы ничего не выиграли");
                    return;
                }
                string prizeName = prizeData.Name;

                switch (prizeData.Type)
                {
                    case LuckyWheelItemType.Donate:
                        string[] splitted = prizeData.Data.Split("_");
                        int randomDonate = ENet.Random.Next(Convert.ToInt32(splitted[0]), Convert.ToInt32(splitted[1]));

                        prizeName += $" в размере {Helper.FormatPrice(randomDonate)}EP";
                        CasinoManager.SetWallType("winner");
                        break;
                    case LuckyWheelItemType.Money:
                        splitted = prizeData.Data.Split("_");
                        int randomCash = ENet.Random.Next(Convert.ToInt32(splitted[0]), Convert.ToInt32(splitted[1]));
                        player.ChangeWallet(randomCash);

                        prizeName += $" в размере {Helper.FormatPrice(randomCash)}$";
                        break;
                    case LuckyWheelItemType.Mystical:
                        // vip
                        CasinoManager.SetWallType("winner");
                        break;
                    case LuckyWheelItemType.Chips:
                        splitted = prizeData.Data.Split("_");
                        int randomChips = ENet.Random.Next(Convert.ToInt32(splitted[0]), Convert.ToInt32(splitted[1]));

                        player.ChangeChips(randomChips);
                        prizeName += $" в размере {Helper.FormatPrice(randomChips)}";
                        break;
                    case LuckyWheelItemType.Clothes:
                        
                        break;
                    case LuckyWheelItemType.Reroll:
                        casinoPlayerData.LuckyWheel = DateTime.Now;
                        break;
                }

                _isWheelSpinning = false;
                player.SendDone($"Вы выиграли {prizeName}, поздравляем!");
                player.ResetData("luckyWheel.prize");
            }
            catch(Exception ex) { Logger.WriteError("Event_EndSpin", ex); } 
        }

        public class LuckyWheelItem
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public int Chance { get; set; }
            public string Data { get; set; }
            public LuckyWheelItemType Type { get; set; }
            
            public LuckyWheelItem(LuckyWheelItemType type, int index, int chance, string name, string data)
            {
                Type = type;
                Index = index;
                Name = name;
                Chance = chance;
                Data = data;
            }
        }

        public enum LuckyWheelItemType
        {
            Vehicle,
            Mystical,
            Clothes,
            Money,
            Donate,
            Chips,
            Reroll
        }
    }
}
