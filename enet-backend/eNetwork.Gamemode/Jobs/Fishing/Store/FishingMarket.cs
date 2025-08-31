using eNetwork.Framework;
using eNetwork.Game;
using eNetwork.Inv;
using eNetwork.Jobs.Fishing.Store.Classes;
using GTANetworkAPI;                        
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Jobs.Fishing.Store
{
    public class FishingMarket
    {

        private static readonly Logger Logger = new Logger(nameof(FishingMarket));
        
        /// <summary>
        /// Название магазина
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Позиция магазина. По совместительству и позиция NPC
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Поворт NPC
        /// </summary>
        public float Heading { get; set; }  

        /// <summary>
        /// Спрайт блипа
        /// 0 - без блипа
        /// https://wiki.rage.mp/index.php?title=Blips
        /// </summary>
        public uint BlipSprite { get; set; }

        /// <summary>
        /// Цвет блипа
        /// https://wiki.rage.mp/index.php?title=Blips
        /// </summary>
        public byte BlipColor { get; set; }

        /// <summary>
        /// Хэш NPC 
        /// https://wiki.rage.mp/index.php?title=Peds
        /// </summary>
        public uint PedHash { get; set; }

        /// <summary>
        /// Список предметов для продажи
        /// </summary>
        public Dictionary<string, List<MarketItem>> Items { get; set; } = new Dictionary<string, List<MarketItem>>();

        public FishingMarket(string name, Vector3 position, float heading, uint blipSprite, byte blipColor, uint pedHash, Dictionary<string, List<MarketItem>> items)
        {
            Name = name;
            Position = position;
            Heading = heading;
            BlipSprite = blipSprite;
            BlipColor = blipColor;
            Items = items;
            PedHash = pedHash;
        }

        private Ped _ped { get; set; }
        private ENetColShape _colShape { get; set; }
        private Blip _blip { get; set; }
        public void GTAElements()
        {
            _ped = NAPI.Ped.CreatePed(PedHash, Position, Heading, false, true, true, false, 0);
            _colShape = ENet.ColShape.CreateCylinderColShape(Position, 2, 2, 0, ColShapeType.FishMarket);
            _colShape.OnEntityEnterColShape += (s, e) => e.SetData("fishing.store", this);
            _colShape.OnEntityExitColShape += (s, e) => e.ResetData("fishing.store");

            if (BlipSprite != 0)
                _blip = NAPI.Blip.CreateBlip(BlipSprite, Position, .8f, BlipColor, Name, 255, 0, true, 0, 0);
        }

        public void Update()
        {
            foreach(var item in Items)
            {
                item.Value.ForEach(storeItem =>
                {
                    if (storeItem.IsUpdated || storeItem.Price == 0)
                        storeItem.Price = ENet.Random.Next(storeItem.MinPrice, storeItem.MaxPrice);
                });
            }
        }

        public void Interaction(ENetPlayer player)
        {
            try
            {
                ClientEvent.Event(player, "client.fishing.store.open", JsonConvert.SerializeObject(Items));
            }
            catch(Exception ex) { Logger.WriteError("Interaction: " + ex.ToString()); }
        } 

        public void Action(ENetPlayer player, string category, int index, int value)
        {
            try
            {
                if (index < 0 || value <= 0 || !Items.TryGetValue(category, out var storeItems) || !Inventory.GetInventory(player.GetUUID(), out var items)) return;

                var item = storeItems.ElementAt(index);
                if (item is null) return;

                if (item.IsSell)
                {
                    if (!Inventory.RemoveItem(player, item.ItemId, value))
                    {
                        player.SendError("У вас нет столько предмета!");
                        return;
                    }

                    int totalPrice = item.Price * value;
                    if (!item.IsDonate)
                    {
                        player.ChangeWallet(totalPrice);
                    }
                    else
                    {
                        // TODO: Прибавить в донат
                    }
                        

                    player.SendDone($"Вы продали {item.Name} за {totalPrice}$", 3000);
                }
                else
                {
                    int totalPrice = item.Price * value; 
                    if (player.CharacterData.Cash < totalPrice)
                    {
                        player.SendError("Недостаточно средств", 3000);
                        return;
                    }

                    var inventoryItem = new Item(item.ItemId, 1);
                    if (!Inventory.CheckCanAddItem(player, inventoryItem))
                    {
                        player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                        return;
                    }

                    player.ChangeWallet(-totalPrice);
                    player.SendDone($"Вы купили {item.Name}", 3000);
                }
            }
            catch (Exception ex) { Logger.WriteError("Buy: " + ex.ToString()); }
        }
    }
}
