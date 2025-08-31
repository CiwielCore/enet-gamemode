using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Game.Characters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Businesses.Info
{
    public class InfoManager
    {
        private static readonly Logger Logger = new Logger(nameof(InfoManager));

        [ChatCommand("addinfobiz", Description = "Добавить позицию взаимодествия к бизнесу", Arguments = "[ID Бизнеса]", Access = PlayerRank.Owner)]
        private static void Command_AddInfoBiz(ENetPlayer player, int id)
        {
            try
            {
                var business = BusinessManager.GetBusiness(id);
                if (business is null)
                {
                    player.SendError($"Бизнес #{id} не найден");
                    return;
                }

                business.PositionInfo = player.Position;
                ENet.Database.Execute($"UPDATE `business` SET `posinfo`='{JsonConvert.SerializeObject(business.PositionInfo)}' WHERE `id`={business.ID}");

                business.CreateInfo();
            }
            catch (Exception ex) { Logger.WriteError("Command_AddPositionToBiz", ex); }
        }

        [InteractionDeprecated(ColShapeType.BusinessInfo)]
        public static void OnInteraction(ENetPlayer player)
        {
            try
            {
                if (!player.GetData<Business>("BUSINESS_INFO", out var business)) return;

                var owner = "Государство";
                if (business.Owner != -1)
                    owner = CharacterManager.GetCharacterData(business.Owner)?.Name;

                var data = new
                {
                    Id = business.ID,
                    Name = business.GetName(),
                    Owner = owner,
                    Price = business.Price,
                };

                ClientEvent.Event(player, "client.business.info.open", JsonConvert.SerializeObject(data));
            }
            catch(Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }

        [CustomEvent("server.business.buy")]
        public static void OnBuy(ENetPlayer player)
        {
            try
            {
                if (!player.GetData<Business>("BUSINESS_INFO", out var business)) return;

                if (business.Owner != -1)
                {
                    player.SendError("Бизнес уже куплен!");
                    return;
                }

                if (!player.ChangeWallet(-business.Price)) return;

                business.Owner = player.GetUUID();
                OnInteraction(player);

                player.SendDone($"Вы купили бизнес за {Helper.FormatPrice(business.Price)}$");

                business.Save().Wait();
            }
            catch(Exception ex) { Logger.WriteError("OnBuy", ex); }
        }
    }
}
