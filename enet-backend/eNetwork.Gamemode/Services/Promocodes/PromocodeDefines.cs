using System;
using System.Collections.Generic;
using eNetwork.Services.Rewards;

namespace eNetwork.Services.Promocodes
{
    class PromocodeConfig
    {
        public int AmountOfMoneyThatOwnerReceive { get; set; }
        public int AmountOfMoneyThatPlayerReceiveWhenActivatePromo { get; set; }
        public string VipThatPlayerReceiveWhenActivatePromo { get; set; }
        public int VipDaysThatPlayerReceiveWhenActivatePromo { get; set; }

        public int RequiredLevelNumberToCompletePromo { get; set; }
        public int AmountOfMoneyThatPlayerReceiveWhenCompletePromo { get; set; }
        public string VipThatPlayerReceiveWhenCompletePromo { get; set; }
        public int VipDaysThatPlayerReceiveWhenCompletePromo { get; set; }
    }

    class Promocode
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int OwnerId { get; set; }
        public RewardTypes RewardType { get; set; }
        public string RewardData { get; set; }
        public SortedDictionary<uint, PromocodeLevel> Levels { get; private set; } = new();

        public bool IsReferralCode => OwnerId != 0;

        public int ActivationsLimit { get; set; }
        public DateTime ValidityPeriod { get; set; }
    }

    class PromocodeLevel
    {
        public uint Index { get; set; }
        public int PromocodeId { get; set; }
        public uint HoursToPlay { get; set; }
        public RewardTypes RewardType { get; set; }
        public string RewardData { get; set; }
    }

    class PromocodeInfo
    {
        public int CharacterId { get; set; }
        public int PromocodeId { get; set; }
        public uint PromocodeLevelIndex { get; set; }
        public uint PlayedHours { get; set; }
        public bool IsComplete { get; set; }
    }
}
