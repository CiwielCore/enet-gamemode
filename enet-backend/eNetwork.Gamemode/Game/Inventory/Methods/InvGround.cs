using eNetwork.Framework;
using eNetwork.Inv;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game
{
    public class InvGround
    {
        private static readonly Logger Logger = new Logger("inv-drop");
        //private static int LastID = 0;
        private static Dictionary<long, GTANetworkAPI.Object> _objects = new Dictionary<long, GTANetworkAPI.Object>();

        [CustomEvent("server.inventory.ground.take")]
        public static void TakeItem(ENetPlayer player, int itemId)
        {
            try
            {
                if (!_objects.TryGetValue(itemId, out var @object)) return;
                if (@object.Position.DistanceTo(player.Position) > 5)
                {
                    player.SendError("Вы слишком далеко");
                    return;
                }

                var itemData = @object.GetData<Item>("item.data"); 
                if (!Inventory.CheckCanAddItem(player, itemData))
                {
                    player.SendError("Недостаточно места");
                    return;
                }
                itemData = TypedItems.Get(itemData);

                Inventory.AddItem(player, itemData);

                player.PlayScenario(ScenarioType.TakeItem);
                // player.PlayTaskAnimation("pickup_object", "pickup_low", 47);
                RemoveFromGround(@object);
            }
            catch(Exception ex) { Logger.WriteError("TakeItem", ex); }
        }

        public static bool Drop(Vector3 position, Item item, uint dimension = 0)
        {
            try
            {
                //LastID++;

                GTANetworkAPI.Object @object = NAPI.Object.CreateObject(item.ItemData.Model, position, new Vector3(), 255, dimension);
                @object.SetSharedData("item.data", JsonConvert.SerializeObject(item.GetItemData()));
                @object.SetData("item.data", item);
                //@object.SetSharedData("item.dropId", LastID);
                @object.SetSharedData("isDynamic", true);
                @object.SetSharedData("isFrozen", false);
                @object.SetData("item.dropTimer", Timers.StartOnce(6 * 60000, () => RemoveFromGround(@object)));

                _objects.TryAdd(item.Id, @object);

                item.OwnerType = ItemOwnerType.None;
                item.OwnerId = -1;
                item.Save();
                return true;
            }
            catch(Exception ex) { Logger.WriteError("Drop", ex); return false; }
        }

        public static void RemoveFromGround(GTANetworkAPI.Object @object)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (@object is null || !@object.HasSharedData("item.data")) return;
                    var dropData = @object.GetSharedData<string>("item.data");

                    var itemData = JsonConvert.DeserializeObject<Item>(dropData);
                    var dropId = itemData.Id;
                    if (_objects.ContainsKey(dropId))
                        _objects.Remove(dropId);

                    if (@object.HasData("item.dropTimer"))
                        Timers.Stop(@object.GetData<string>("item.dropTimer"));

                    @object.Delete();
                }
                catch (Exception ex) { Logger.WriteError("RemoveFromGround", ex); }
            });
        }

        public static void Drop(ENetPlayer player, Item item, bool playAnim = true)
        {
            try
            {
                var drop = Drop(player.Position, item, player.Dimension);

                if (!drop)
                {
                    player.SendError("Не удалось выбросить предмет");
                    return;
                }

                if (playAnim)
                    player.PlayScenario(ScenarioType.DropItem);
                
                Inventory.RemoveItem(player, item.Slot);
            }
            catch(Exception ex) { Logger.WriteError("Drop", ex); }
        }
    }
}
