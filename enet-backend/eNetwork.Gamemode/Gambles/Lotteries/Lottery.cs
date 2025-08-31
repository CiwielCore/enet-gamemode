using System;
using System.Linq;
using eNetwork.Framework;
using eNetwork.Framework.Singleton;
using eNetwork.Framework.Utils;
using eNetwork.Gambles.Lotteries.LotteryModels;
using eNetwork.Inv;

namespace eNetwork.Gambles.Lotteries
{
    class Lottery : Singleton<Lottery>
    {
        private readonly Logger _logger;
        private readonly RandomValues _prizes;
        private readonly LotteryConfig _config;
        private readonly LotteryPrizeGiver _prizeGiver;
        private readonly LotteryTicketBank _ticketsBank;
        private string _updateTicketsTimerId;

        private Lottery()
        {
            _logger = new Logger(nameof(Lottery));
            _prizes = new RandomValues();
            _config = ConfigReader.Read<LotteryConfig>("lottery/config.json");
            _ticketsBank = new LotteryTicketBank(_config.TicketsForDay);
            _prizeGiver = new LotteryPrizeGiver();
        }

        public void OnResourceStart()
        {
            LoadPrizes();
            StartUpdateTicketsTimer();
        }

        public bool TakeTicket()
        {
            return _ticketsBank.TakeTicket();
        }

        public void UseTicket(ENetPlayer player, Item item)
        {
            string prizeValue = item.Data == null ? _prizes.GetRandomValue() : item.Data;
            LotteryPrize prize = _config.Prizes.FirstOrDefault(p => p.Value == prizeValue);
            LotteryPrizeDTO prizeDTO = new LotteryPrizeDTO() { Type = prize.Type.ToString("G"), Value = prize.Value };

            item.Data = prizeValue;
            ClientEvent.Event(player, "client.lottery.show_ticket", prizeDTO.ToString());
        }

        public void GivePrize(ENetPlayer player, Item item)
        {
            LotteryPrize prize = _config.Prizes.FirstOrDefault(p => p.Value == item.Data);
            if (prize == null)
            {
                player.SendError($"Ошибка выдачи приза: {item.Data}");
                return;
            }

            _prizeGiver.GivePrize(player, prize);
            Game.Inventory.RemoveItem(player, item, 1);
        }

        private void LoadPrizes()
        {
            foreach (LotteryPrize prize in _config.Prizes)
            {
                _logger.WriteDebug($"Prize: {prize.Type} {prize.Value} {prize.Chance}");
                if (!_prizes.TryAddValue(prize.Value, prize.Chance))
                    _logger.WriteError($"Prize ({prize.Value}) не загружно. Пожалуйста посмотрите конфиг.");
            }

            _logger.WriteInfo($"Загружено {_config.Prizes.Count} призов");
        }

        private void StartUpdateTicketsTimer()
        {
            TimeSpan nowTime = DateTime.Now.TimeOfDay;
            TimeSpan updateTime = new TimeSpan(_config.UpdateTicketsTime.Hours, _config.UpdateTicketsTime.Minutes, 0);
            double milliseconds = updateTime.Subtract(nowTime).TotalMilliseconds;
            _updateTicketsTimerId = Timers.StartOnce(Convert.ToInt32(milliseconds), UpdateTickets);
        }

        private void UpdateTickets()
        {
            _ticketsBank.Fill();
            StartUpdateTicketsTimer();
        }
    }
}