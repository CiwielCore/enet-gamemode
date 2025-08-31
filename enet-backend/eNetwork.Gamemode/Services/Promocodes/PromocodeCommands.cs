using eNetwork.Framework;
using eNetwork.Services.Rewards;
using System.Threading.Tasks;

namespace eNetwork.Services.Promocodes
{
    class PromocodeCommands
    {
        [ChatCommand("createpromo", "Создать реферальный код для игрока", Access = PlayerRank.Admin, Arguments = "[название] [uuid]")]
        public static async Task CreateReferralCodeCommandHandler(ENetPlayer player, string promo, int ownerId)
        {
            if (ownerId < 1)
            {
                player.SendWarning("Укажите владельца реферального кода");
                return;
            }

            Task task = PromocodeService.Instance.CreatePromocode(player, promo, ownerId, RewardTypes.Money, 
                PromocodeService.Instance.Config.AmountOfMoneyThatOwnerReceive.ToString());

            await task.ContinueWith(async (t) =>
            {
                if (t.IsCanceled)
                    return;

                await PromocodeService.Instance.CreatePromocodeLevel(player, promo, 0, RewardTypes.Money,
                    PromocodeService.Instance.Config.AmountOfMoneyThatPlayerReceiveWhenActivatePromo.ToString());

                await PromocodeService.Instance.CreatePromocodeLevel(player, promo, 0, RewardTypes.Vip,
                    string.Join("_", PromocodeService.Instance.Config.VipThatPlayerReceiveWhenActivatePromo,
                        PromocodeService.Instance.Config.VipDaysThatPlayerReceiveWhenActivatePromo));

                await PromocodeService.Instance.CreatePromocodeLevel(player, promo, 15, RewardTypes.Money,
                    PromocodeService.Instance.Config.AmountOfMoneyThatPlayerReceiveWhenCompletePromo.ToString());

                await PromocodeService.Instance.CreatePromocodeLevel(player, promo, 0, RewardTypes.Vip,
                    string.Join("_", PromocodeService.Instance.Config.VipThatPlayerReceiveWhenCompletePromo,
                        PromocodeService.Instance.Config.VipDaysThatPlayerReceiveWhenCompletePromo));

                player.SendInfo($"Вы успешно создали и добавили промокод {promo} для {ownerId}");
            });
        }

        [ChatCommand("createbonus", "Создать бонус код", Access = PlayerRank.Admin,
            Arguments = "[название промокода] [кол-во активаций] [время действия] [деньги] [донат] [транспорт]")]
        public static async Task CreatePromocodeCommandHandler(ENetPlayer player, string promo, int activationsLimit, int validityHours,
            int money = 0, int donatePoints = 0, string modelName = "")
        {
            if (money < 1 && donatePoints < 1 && string.IsNullOrWhiteSpace(modelName))
            {
                player.SendError("Укажите хотя бы одну награду для бонус-кода");
                return;
            }

            Task task = PromocodeService.Instance.CreatePromocode(player, promo, activationsLimit, validityHours);

            await task.ContinueWith(async (t) =>
            {
                if (t.IsCanceled)
                    return;

                if (money > 0)
                    await PromocodeService.Instance.CreatePromocodeLevel(player, promo, 0, RewardTypes.Money, money.ToString());

                if (donatePoints > 0)
                    await PromocodeService.Instance.CreatePromocodeLevel(player, promo, 0, RewardTypes.DonatePoints, donatePoints.ToString());

                if (!string.IsNullOrWhiteSpace(modelName))
                    await PromocodeService.Instance.CreatePromocodeLevel(player, promo, 0, RewardTypes.Vehicle, modelName);

                player.SendInfo($"Вы успешно создали и добавили бонус-код: {promo}");
            });
        }

        [ChatCommand("promocode", "Активировать промокод", Arguments = "[промокод]")]
        public static void PromocodeActivateCommandHandler(ENetPlayer player, string promo)
        {
            PromocodeService.Instance.ActivatePromocodeForPlayer(player, promo);
        }

        [ChatCommand("bonus", "Активировать бонус-код", Arguments = "[бонус-код]")]
        public static void BonusActivateCommandHandler(ENetPlayer player, string promo)
        {
            PromocodeService.Instance.ActivateBonusPromocodeForPlayer(player, promo);
        }
    }
}
