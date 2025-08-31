using eNetwork.Factions.Classes;
using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Game;
using eNetwork.Inv;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Houses.Storage
{
    public class StorageData
    {
        private static readonly Logger Logger = new Logger("storage-data");
        private House House { get; set; }

        private ENetColShape _colShape { get; set; }
        private Marker _marker { get; set; }
        public StorageData(House house)
        {
            House = house;

            _colShape = ENet.ColShape.CreateCylinderColShape(House.InteriorData.Storage, 1f, 2f, House.GetDimension(), ColShapeType.HouseStorage);
            _colShape.OnEntityEnterColShape += (s, e) => e.SetData("house.storage", this);
            _colShape.OnEntityExitColShape += (s, e) => e.ResetData("house.storage");
            _colShape.SetInteractionText("Шкаф");

            _marker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, House.InteriorData.Storage, new Vector3(), new Vector3(), .8f, Helper.GTAColor, false, House.GetDimension());
        }

        public List<ENetPlayer> PlayersInStorage { get; set; } = new List<ENetPlayer>();

        public void Enter(ENetPlayer player)
        {
            try
            {
                if (!PlayersInStorage.Contains(player))
                    PlayersInStorage.Add(player);
            }
            catch (Exception ex) { Logger.WriteError("Enter", ex); }
        }

        public void Leave(ENetPlayer player)
        {
            try
            {
                if (PlayersInStorage.Contains(player))
                    PlayersInStorage.Remove(player);
            }
            catch (Exception ex) { Logger.WriteError("Leave", ex); }
        }

        public bool TryAdd(ENetPlayer player, Item item)
        {
            try
            {
                var add = AddItem(item);
                if (!add)
                {
                    player.SendError("Недостаточно места на складе");
                    return false;
                }

                return true;
            }
            catch (Exception ex) { Logger.WriteError("TryAdd", ex); return false; }
        }

        public bool AddItem(Item item)
        {
            try
            {
                var itemData = InvItems.Get(item.Type);
                if (itemData is null) return false;

                int weight = itemData.Weight * item.Count;

                if (GetWeight() + weight > House.InteriorData.StorageWeight) return false;

                item.IsActive = false;
                item.Slot = -1;
                House.StorageItems.Add(item);

                PlayersInStorage.ForEach((player) => InvOut.SendItems(player, House.StorageItems));
                return true;
            }
            catch (Exception ex) { Logger.WriteError("AddItem", ex); return false; }
        }

        public Item GetItem(int index)
        {
            try
            {
                if (index < 0 || index >= House.StorageItems.Count) return null;

                var item = House.StorageItems.ElementAt(index);
                if (item is null) return null;

                return item;
            }
            catch (Exception ex) { Logger.WriteError("GetItem", ex); return null; }
        }

        public bool RemoveItem(Item item, bool remove = true)
        {
            try
            {
                if (!House.StorageItems.Contains(item)) return false;

                if (remove)
                {
                    House.StorageItems.Remove(item);
                    PlayersInStorage.ForEach((player) => InvOut.SendItems(player, House.StorageItems));
                }

                return true;
            }
            catch (Exception ex) { Logger.WriteError("RemoveItem", ex); return false; }
        }

        public bool RemoveItem(int index, bool remove = true)
        {
            try
            {
                var item = GetItem(index);
                if (item is null) return false;

                return RemoveItem(item, remove);
            }
            catch (Exception ex) { Logger.WriteError("RemoveItem", ex); return false; }
        }

        public int GetWeight()
        {
            int weight = 0;
            House.StorageItems.ForEach(item =>
            {
                var itemData = InvItems.Get(item.Type);
                if (itemData != null)
                    weight += itemData.Weight * item.Count;
            });

            return weight;
        }

        [InteractionDeprecated(ColShapeType.HouseStorage)]
        public static void OnInteraction(ENetPlayer player)
        {
            try
            {
                if (!player.GetData("house.storage", out StorageData storage) || !storage.House.CanAccess(player.GetUUID())) return;

                if (storage.PlayersInStorage.Count > 0)
                {
                    player.SendError("Кто-то уже лазит в шкафу!");
                    return;
                }

                player.OpenOut(storage.House.StorageItems, InvOutType.HomeStorage, storage.House.Id, storage.House.InteriorData.StorageWeight);
            }
            catch(Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }
    }
}
