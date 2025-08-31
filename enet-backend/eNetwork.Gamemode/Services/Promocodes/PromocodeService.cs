using eNetwork.Framework;
using eNetwork.Framework.Singleton;
using eNetwork.Game.Characters;
using eNetwork.Services.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace eNetwork.Services.Promocodes
{
    class PromocodeService : Singleton<PromocodeService>
    {
        private readonly Logger _logger;
        private readonly PromocodeRepository _repository;
        public readonly PromocodeConfig Config;

        private PromocodeService()
        {
            _logger = new Logger("promocode-service");
            _repository = new PromocodeRepository(_logger);
            Config = ConfigReader.Read<PromocodeConfig>("services/promocodes_config.json");
        }

        public void OnResourceStart()
        {
            _repository.OnResourceStart();
        }

        public Task CreatePromocode(ENetPlayer player, string promoText, int activationsLimit, int validityPeriodHours)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            if (_repository.GetPromocodeByText(promoText) is not null)
            {
                player.SendError("Промокод с таким названием уже существует");
                cts.Cancel();
                return Task.FromCanceled(cts.Token);
            }

            Promocode promocode = new Promocode()
            {
                Text = promoText,
                ActivationsLimit = activationsLimit,
                ValidityPeriod = DateTime.UtcNow.AddHours(validityPeriodHours)
            };

            return _repository.CreatePromocodeInDB(promocode);
        }

        public Task CreatePromocode(ENetPlayer player, string promoText, int ownerId, RewardTypes rewardType, string rewardData)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            if (_repository.GetPromocodeByText(promoText) is not null)
            {
                player.SendError("Промокод с таким названием уже существует");
                cts.Cancel();
                return Task.FromCanceled(cts.Token);
            }

            if (CharacterManager.GetCharacterData(ownerId) is null)
            {
                player.SendError($"Не существует персонажа с id: {ownerId}");
                cts.Cancel();
                return Task.FromCanceled(cts.Token);
            }

            Promocode promocode = new Promocode()
            {
                Text = promoText,
                OwnerId = ownerId,
                RewardType = rewardType,
                RewardData = rewardData
            };

            return _repository.CreatePromocodeInDB(promocode);
        }

        public Task CreatePromocodeLevel(ENetPlayer player, string promoText, uint hoursToPlay, RewardTypes rewardType, string rewardData)
        {
            Promocode promo = _repository.GetPromocodeByText(promoText);
            if (promo is null)
            {
                player.SendError("Промокод с таким названием не существует");
                return Task.CompletedTask;
            }

            uint nextLevelIndex = promo.Levels.Any() ? promo.Levels.Keys.Last() + 1 : 1;

            PromocodeLevel level = new()
            {
                Index = nextLevelIndex,
                PromocodeId = promo.Id,
                HoursToPlay = hoursToPlay,
                RewardType = rewardType,
                RewardData = rewardData
            };

            return _repository.CreatePromocodeLevelInDB(level).ContinueWith(
                (t) => promo.Levels.Add(nextLevelIndex, level));
        }

        public void ActivatePromocodeForPlayer(ENetPlayer player, string promocodeText)
        {
            Promocode promocode = _repository.GetPromocodeByText(promocodeText);
            if (promocode is null)
            {
                player.SendWarning("Такого промокода не существует");
                return;
            }

            if (promocode.IsReferralCode is false)
                return;

            int characterId = player.GetUUID();
            IReadOnlyCollection<PromocodeInfo> promoActivations = _repository.GetPromocodeInfos(characterId);
            if (promoActivations.Select(a => _repository.GetPromocodeById(a.PromocodeId)).Any(p => p.IsReferralCode))
            {
                player.SendError("Вы уже вводили один промокод");
                return;
            }

            if (promocode.OwnerId == characterId)
            {
                player.SendError("Вы не можете активировать свой же промокод");
                return;
            }

            PromocodeLevel firstLevel = promocode.Levels.Values.First();
            if (firstLevel is null)
            {
                player.SendWarning("Данный промокод сейчас недоступен");
                return;
            }

            PromocodeInfo promocodeInfo = new PromocodeInfo()
            {
                CharacterId = characterId,
                PromocodeId = promocode.Id,
                PromocodeLevelIndex = firstLevel.Index,
            };

            _repository.CreatePromocodeInfoInDB(promocodeInfo).ContinueWith(
                (t) =>
                {
                    player.SendInfo($"Вы активировали промокод {promocode.Text}");

                    if (firstLevel.HoursToPlay == 0)
                    {
                        CompletePromocodeLevel(player, promocode, promocodeInfo, firstLevel);
                    }
                });
        }

        public void ActivateBonusPromocodeForPlayer(ENetPlayer player, string promocodeText)
        {
            Promocode promocode = _repository.GetPromocodeByText(promocodeText);
            if (promocode is null || promocode.IsReferralCode)
            {
                player.SendWarning("Такого бонус-кода не существует");
                return;
            }

            int activations = _repository.GetPromocodeActivations(promocode.Id).Count();
            if (activations >= promocode.ActivationsLimit)
            {
                player.SendWarning("Количество активаций данного бонус-кода достигло лимита");
                return;
            }

            if (promocode.ValidityPeriod < DateTime.UtcNow)
            {
                player.SendWarning("Время активации данного бонус-кода вышло");
                return;
            }

            int characterId = player.GetUUID();
            if (_repository.GetPromocodeInfos(characterId).Any(p => p.PromocodeId == promocode.Id))
            {
                player.SendError("Вы уже вводили данный бонус-код");
                return;
            }

            PromocodeLevel firstLevel = promocode.Levels.Values.First();
            if (firstLevel is null)
            {
                player.SendWarning("Данный бонус-код сейчас недоступен");
                return;
            }

            PromocodeInfo promocodeInfo = new PromocodeInfo()
            {
                CharacterId = characterId,
                PromocodeId = promocode.Id,
                PromocodeLevelIndex = firstLevel.Index,
            };

            _repository.CreatePromocodeInfoInDB(promocodeInfo).ContinueWith(
                (t) =>
                {
                    player.SendInfo($"Вы активировали бонус-код {promocode.Text}");

                    if (firstLevel.HoursToPlay == 0)
                    {
                        CompletePromocodeLevel(player, promocode, promocodeInfo, firstLevel);
                    }
                });
        }

        public void TryAddPlayedHourForPlayerPromocodeinfo(ENetPlayer player)
        {
            IReadOnlyCollection<PromocodeInfo> promocodeInfos = _repository.GetPromocodeInfos(player.GetUUID());
            foreach (PromocodeInfo promoInfo in promocodeInfos)
            {
                Promocode promocode = _repository.GetPromocodeById(promoInfo.PromocodeId);
                if (promocode is null)
                    throw new KeyNotFoundException(
                        $"{nameof(PromocodeInfo)} of the player (Guid: {promoInfo.CharacterId}) is invalid. " +
                        "The associated promocode was not found in the list." +
                        $"The text of the desired promocode: {promoInfo.PromocodeId}");

                PromocodeLevel level = promocode.Levels.ContainsKey(promoInfo.PromocodeLevelIndex)
                    ? promocode.Levels[promoInfo.PromocodeLevelIndex]
                    : throw new KeyNotFoundException(
                        $"The {nameof(PromocodeLevel)} of the promocode by the index " +
                        $"from the {nameof(PromocodeInfo)} of the player (Guid: {promoInfo.CharacterId}) was not found." +
                        $"The promocode text of the desired {nameof(PromocodeLevel)}: {promoInfo.PromocodeId}");

                promoInfo.PlayedHours += 1;
                if (promoInfo.PlayedHours >= level.HoursToPlay)
                    CompletePromocodeLevel(player, promocode, promoInfo, level);

                _repository.UpdatePromocodeInfoInDB(promoInfo);
            }
        }

        private void CompletePromocodeLevel(ENetPlayer player, Promocode promo, PromocodeInfo promoInfo, PromocodeLevel level)
        {
            IReward reward = RewardService.Instance.GetRewardByType(level.RewardType, level.RewardData);
            reward.GiveReward(player);

            PromocodeLevel nextLevel = promo.Levels.GetValueOrDefault(level.Index + 1);
            if (nextLevel == null)
            {
                promoInfo.IsComplete = true;
                GiveOwnerBonusForActivatingHisPromocode(promo);
                return;
            }

            if (nextLevel.HoursToPlay < 1)
            {
                CompletePromocodeLevel(player, promo, promoInfo, nextLevel);
                return;
            }

            promoInfo.PlayedHours = 0;
            promoInfo.PromocodeLevelIndex = nextLevel.Index;
            player.SendInfo($"Вы на {nextLevel.Index} уровне промокода {promo.Text}. Нужно отыграть {nextLevel.HoursToPlay}ч.");
        }

        private void GiveOwnerBonusForActivatingHisPromocode(Promocode promo)
        {
            if (promo.OwnerId == 0)
                return;

            IReward reward = RewardService.Instance.GetRewardByType(promo.RewardType, promo.RewardData);
            ENetPlayer player = ENet.Pools.GetPlayerByUUID(promo.OwnerId);
            if (player is null)
            {
                reward.GiveRewardOffline((uint)promo.OwnerId);
                return;
            }

            player.SendInfo($"Кто-то из рефералов прошел все уровни вашего промокода ({promo.Text}).", 5000);
            reward.GiveReward(player);
        }
    }
}
