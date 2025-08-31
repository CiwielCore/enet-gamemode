using eNetwork.Framework;
using eNetwork.Game.Vehicles;
using eNetwork.Game.Weapons;
using eNetwork.Inv;
using eNetwork.Inv.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eNetwork.Game
{
    public class InvInterface
    {
        private static readonly Logger Logger = new Logger("inventory-interface");
        public static void Update(ENetPlayer player, string type, List<Item> InvItems, Item item)
        {
            try
            {
                ClientEvent.Event(player, "client.inventory.upadte", type, InvItems.IndexOf(item), JsonConvert.SerializeObject(GetItemData(item)));
            }
            catch (Exception ex) { Logger.WriteError("Update", ex); }
        }

        public static void SendItems(ENetPlayer player)
        {
            try
            {
                if (!Inventory.GetInventory(player.GetUUID(), out Storage storage)) return;

                //var list = new List<object>();
                //storage.Items.ForEach(item => { list.Add(GetItemData(item)); });
                var lst = JsonConvert.SerializeObject(storage.Items.Select(el => el.GetItemData()));
                ClientEvent.Event(player, "client.inventory.setItems", "main", lst);

                // Backpack
                var backpack = (Inv.Items.BackpackItem)storage.Items.FirstOrDefault(el=>el is Inv.Items.BackpackItem && el.Type == ItemId.Bag && el.IsActive && el.Slot == -((int)el.Type));
                if(backpack != null) InvBackpack.SendItems(player, backpack);
            }
            catch (Exception ex) { Logger.WriteError("SendInvItems", ex); }
        }

        public static void AddItem(ENetPlayer player, string type, Item item, int slot)
        {
            try
            {
                if (!Inventory.GetInventory(player.GetUUID(), out Storage storage)) return;

                item.Slot = slot;
                ClientEvent.Event(player, "client.inventory.addItem", type, JsonConvert.SerializeObject(GetItemData(item)));
            }
            catch (Exception e) { Logger.WriteError("AddItem", e); }
        }

        [CustomEvent("server.inventory.items.moveSlot")]
        public static void MoveItem(ENetPlayer player, string currentType, int itemId, string dropType, int dropSlot)
        {
            try
            {
                var charData = player.GetCharacter();
                if (!Inventory.GetInventory(charData.UUID, out Storage storage)) return;

                switch (currentType)
                {
                    case "main":
                        {
                            switch (dropType)
                            {
                                case "main":
                                    {
                                        if (!Inventory.MoveSlot(storage, itemId, dropSlot))
                                        {
                                            SendItems(player);
                                            Console.WriteLine("error move slot");
                                        }
                                        break;
                                    }
                                case "bag":
                                    {
                                        var backpack = (Inv.Items.BackpackItem)storage.Items.FirstOrDefault(el=>el is Inv.Items.BackpackItem && el.Type == ItemId.Bag && el.Slot == -((int)el.Type));
                                        if (backpack == null)
                                        {
                                            Console.WriteLine("InvInt err main > bag (null)");
                                            return;
                                        }
                                        if (InvBackpack.FindItemBySlot(backpack, dropSlot) is null)
                                        {
                                            var item = Inventory.FindItem(player, itemId);
                                            if (item == null) return;
                                            //item.Slot = dropSlot;
                                            //item.OwnerType = ItemOwnerType.Backpack;
                                            //item.OwnerId = backpack.Id;
                                            //item.Save();
                                            InvBackpack.AddItem(backpack, item, dropSlot);
                                            storage.Items.Remove(item);
                                            InvInterface.SendItems(player);
                                            //if (InvBackpack.TryAdd(player, item, dropSlot))
                                            //{
                                            //    Inventory.RemoveItem(player, item);
                                            //}
                                        }
                                        break;
                                    }
                                case "fastslots":
                                    {
                                        var item = Inventory.FindItem(player, itemId);
                                        if (item is null) return;

                                        if (!Inventory.MoveSlot(storage, itemId, dropSlot))
                                        {
                                            SendItems(player);
                                            return;
                                        }

                                        if (item is Inv.Items.WeaponItem weaponItem)
                                        {
                                            int fastSlot = (dropSlot + 100) * -1;
                                            WeaponAttachment.SetWeaponInFastSlot(player, item.Type, weaponItem.Components, fastSlot);
                                        }
                                        //if (item.ItemData.ItemType == ItemType.Weapon)
                                        //{
                                        //    int fastSlot = (dropSlot + 100) * -1;
                                        //    WeaponAttachment.SetWeaponInFastSlot(player, item.Type, JsonConvert.DeserializeObject<InvWeaponData>(item.Data).WeaponComponents, fastSlot);
                                        //}
                                        break;
                                    }
                                case "clothes":
                                    {
                                        var item = Inventory.FindItem(player, itemId);
                                        if (item is null) return;
                                        if(item is ClotheItem clotheItem)
                                        {
                                            InvUse.ItemUsing(player, currentType, itemId, dropSlot, false);
                                        }
                                        break;
                                    }
                                case "out":
                                    {
                                        var item = Inventory.FindItem(player, itemId);
                                        if (item is null) return;
                                        InvGround.Drop(player, item);
                                        break;
                                    }
                                default: return;
                            }
                        }
                        break;
                    case "fastslots":
                        if (dropType == "main")
                        {
                            var item = Inventory.FindItem(player, itemId);
                            if (item is null) return;

                            if (item.IsActive)
                            {
                                player.SendWarning("Сначала уберите предмет из рук");
                                return;
                            }

                            ItemType itemType = InvItems.GetType(item.Type);

                            int _dropSlot = dropSlot;
                            if (dropSlot == -999)
                            {
                                dropSlot = Inventory.TryGetSlot(storage, item, InventoryType.Player, -1);
                                if (dropSlot == -1)
                                {
                                    player.SendError("Недостаточно места в инвентаре");
                                    return;
                                }
                            }

                            if (!Inventory.MoveSlot(storage, itemId, dropSlot))
                            {
                                SendItems(player);
                                return;
                            }

                            if (itemType == ItemType.Weapon)
                            {
                                int fastSlot = (itemId + 100) * -1;
                                WeaponAttachment.RemoveWeaponFromFastSlot(player, item.Type, fastSlot);
                            }

                            if (_dropSlot == -999)
                                InvInterface.SendItems(player);
                        }
                        break;
                    case "clothes":
                        {
                            if (dropType == "main")
                            {
                                var item = Inventory.FindItem(player, itemId);
                                if (item is null) return;

                                Console.WriteLine($"currentType: {currentType}; currentSlot: {itemId}; dropSlot: {dropSlot}");
                                InvUse.ItemUsing(player, currentType, itemId, dropSlot, false);
                            }
                            else
                            {
                                player.SendError("Сначала снимите одежду");
                            }
                        }
                        break;
                    case "bag":
                        {
                            var backpack = (Inv.Items.BackpackItem)storage.Items.FirstOrDefault(el=>el is Inv.Items.BackpackItem && el.Type == ItemId.Bag && el.Slot == -((int)el.Type));
                            if (backpack == null)
                            {
                                Console.WriteLine("MoveItem err bag (null)");
                                return;
                            }
                            if (dropType == "bag")
                            {
                                if (!InvBackpack.MoveItem(backpack, itemId, dropSlot))
                                {
                                    SendItems(player);
                                    Console.WriteLine("error move bag slot");
                                }
                            }
                            if (dropType == "main")
                            {
                                if (Inventory.FindItemBySlot(storage, dropSlot) is null)
                                {
                                    var item = InvBackpack.FindItemById(backpack, itemId);
                                    if(item is null)
                                    {
                                        Console.WriteLine("MoveItem2 err bag > main - item (null)");
                                        return;
                                    }
                                    //тут предмет перекладывается из рюкзака в основной инвентарь
                                    //раньше: предмет удалялся и создавался новый
                                    //надо сделать: предмет просто меняет координату и тип принадлежности
                                    item.Slot = dropSlot;
                                    item.OwnerType = ItemOwnerType.Player;
                                    item.OwnerId = charData.UUID;
                                    item.Save();
                                    storage.Items.Add(item);
                                    InvInterface.SendItems(player);
                                    //if (Inventory.CreateNewItem(player, item, dropSlot))
                                    //{
                                    //    InvBackpack.RemoveItem(player, item);
                                    //}
                                }
                            }
                        }
                        break;
                    case "out":
                        {
                            switch (dropType)
                            {
                                case "main":
                                    {
                                        InvGround.TakeItem(player, itemId);
                                        break;
                                    }
                                default: return;
                            }
                            break;
                        }
                    default: return;
                }
            }
            catch (Exception e) { Logger.WriteError("MoveItem", e); }
        }

        [CustomEvent("server.inventory.items.swap")]
        public static void ItemSwap(ENetPlayer player, int itemId, string currentType, int dropSlot, string dropType)
        {
            try
            {
                if (!Inventory.GetInventory(player.GetUUID(), out Storage storage)) return;

                if (dropType == "out" || currentType == "out") return;

                if (currentType == "clothes")
                {
                    player.SendError("Сначала снимите одежду");
                    return;
                }

                var currentItem = Inventory.FindItem(player, itemId);
                var dropItem = Inventory.FindItem(player, dropSlot);
                if (dropType == "fastslots" && dropItem.IsActive)
                {
                    player.SendError("Сначала уберите предмет из рук");
                    return;
                }

                if (currentType == "bag" && dropType != "main")
                {
                    player.SendError("Сначала вытащите предмет из рюкзака");
                    return;
                }

                if (InvItems.GetType(currentItem.Type) == ItemType.Clothes && dropType == "clothes")
                {
                    InvUse.ItemUsing(player, dropType, dropItem, itemId, true);
                    InvUse.ItemUsing(player, currentType, currentItem, dropSlot, true);
                    return;
                }

                var dropItemType = InvItems.GetType(dropItem.Type);

                var currentItemType = InvItems.GetType(currentItem.Type);
                if (dropType == "fastslots" || currentType == "fastslots")
                {
                    int fastSlot = currentType == "fastslots" ? (currentItem.Slot + 100) * -1 : (dropSlot + 100) * -1;
                    if (dropItemType == ItemType.Weapon)
                    {
                        Console.WriteLine("Remove fs: " + fastSlot);
                        WeaponAttachment.RemoveWeaponFromFastSlot(player, dropItem.Type, fastSlot);
                    }
                    if (currentItemType == ItemType.Weapon)
                    {
                        if (currentItem is Inv.Items.WeaponItem currentWeaponItem)
                        {
                            Console.WriteLine("Set fs: " + fastSlot);
                            WeaponAttachment.SetWeaponInFastSlot(player, currentWeaponItem.Type, currentWeaponItem.Components, fastSlot);
                        }
                    }
                }

                dropItem.Slot = currentItem.Slot;
                currentItem.Slot = dropSlot;
            }
            catch (Exception ex) { Logger.WriteError("InvItemswap", ex); }
        }

        [CustomEvent("server.inventory.fastslots.use")]
        private static void UseFastslot(ENetPlayer player, int slot)
        {
            try
            {
                var item = Inventory.GetFastslotItem(player, slot);
                if (item is null) return;

                InvUse.ItemUsing(player, "main", item.Slot, -1, false);
            }
            catch (Exception ex) { Logger.WriteError("UseFastslot", ex); }
        }

        [CustomEvent("server.inventory.items.drop")]
        private static void DropItem(ENetPlayer player, int itemId)
        {
            try
            {
                var item = Inventory.FindItem(player, itemId);
                if (item is null) return;

                if (item.IsActive)
                {
                    player.SendError("Сначала уберите предмет из рук");
                    return;
                }

                InvGround.Drop(player, item);
            }
            catch (Exception ex) { Logger.WriteError("DropItem", ex); }
        }

        [CustomEvent("server.inventory.weapons.setComponent")]
        private static void SetComponent(ENetPlayer player, int weaponId, int componentSlot, int dropSlot)
        {
            try
            {
                if (!player.GetData("inventory.weapon.checked", out Item _checkedItem)) return;
                player.ResetData("inventory.weapon.checked");

                var item = Inventory.FindItem(player, weaponId);
                if (item is null || item != _checkedItem) return;

                if (item is Inv.Items.WeaponItem weaponItem)
                {
                    var componentItem = Inventory.FindItem(player, componentSlot);
                    if (componentItem is null) return;

                    var _componentItem = InvItems.Get(componentItem.Type);
                    if (_componentItem is null || _componentItem.ItemType != ItemType.WeaponComponent) return;

                    //InvWeaponData weaponItemData = JsonConvert.DeserializeObject<InvWeaponData>(weaponItem.Data);

                    switch (componentItem.Type)
                    {
                        case ItemId.ExtendedClip:
                            if (weaponItem.Components.Clip)
                            {
                                player.SendInfo($"На оружии уже стоит {_componentItem.Name}");
                                return;
                            }

                            weaponItem.Components.Clip = true;
                            break;
                        case ItemId.FlashlightComponent:
                            if (weaponItem.Components.Flashlight)
                            {
                                player.SendInfo($"На оружии уже стоит {_componentItem.Name}");
                                return;
                            }

                            weaponItem.Components.Flashlight = true;
                            break;
                        case ItemId.Grip:
                            if (weaponItem.Components.Grip)
                            {
                                player.SendInfo($"На оружии уже стоит {_componentItem.Name}");
                                return;
                            }

                            weaponItem.Components.Grip = true;
                            break;
                        case ItemId.HolographicSight:
                            if (weaponItem.Components.HolographicSight || weaponItem.Components.Scope)
                            {
                                player.SendInfo($"На оружии уже стоит прицел");
                                return;
                            }

                            weaponItem.Components.HolographicSight = true;
                            break;
                        case ItemId.Scope:
                            if (weaponItem.Components.HolographicSight || weaponItem.Components.Scope)
                            {
                                player.SendInfo($"На оружии уже стоит прицел");
                                return;
                            }

                            weaponItem.Components.Scope = true;
                            break;
                        case ItemId.Suppressor:
                            if (weaponItem.Components.Suppressor)
                            {
                                player.SendInfo($"На оружии уже стоит прицел");
                                return;
                            }

                            weaponItem.Components.Suppressor = true;
                            break;
                        default: return;
                    }

                    weaponItem.RefreshParams();
                    Inventory.RemoveItem(player, componentItem);
                    if (weaponItem.IsActive)
                        WeaponController.GiveWeapon(player, weaponItem.Type, weaponItem.Components, true);

                    SendItems(player);
                }
            }
            catch (Exception ex) { Logger.WriteError("SetComponent", ex); }
        }

        [CustomEvent("server.inventory.weapons.setComponent.try")]
        private static void SetComponentTry(ENetPlayer player, int weaponId, int componentSlot, int dropSlot)
        {
            try
            {
                var weaponItem = Inventory.FindItem(player, weaponId);
                if (weaponItem is null) return;

                var _weaponItemData = InvItems.Get(weaponItem.Type);
                if (_weaponItemData is null || _weaponItemData.ItemType != ItemType.Weapon) return;

                var weaponData = Configs.Weapons.GetWeaponData(weaponItem.Type);
                if (weaponData is null) return;

                var componentItem = Inventory.FindItem(player, componentSlot);
                if (componentItem is null) return;

                var _componentItem = InvItems.Get(componentItem.Type);
                if (_componentItem is null || _componentItem.ItemType != ItemType.WeaponComponent) return;

                string componentName = "";
                switch (componentItem.Type)
                {
                    case ItemId.Grip: componentName = "Grip"; break;
                    case ItemId.ExtendedClip: componentName = "Clip"; break;
                    case ItemId.Scope: componentName = "Scope"; break;
                    case ItemId.HolographicSight: componentName = "HolographicSight"; break;
                    case ItemId.Suppressor: componentName = "Suppressor"; break;
                    case ItemId.FlashlightComponent: componentName = "Flashlight"; break;
                    default: return;
                }

                player.SetData("inventory.weapon.checked", weaponItem);
                ClientEvent.Event(player, "client.inventory.weapons.setComponent.try", weaponData.Hash, componentName, weaponId, componentSlot, dropSlot);
            }
            catch (Exception ex) { Logger.WriteError("SetComponentTry", ex); }
        }

        [CustomEvent("server.inventory.weapons.takeComponents")]
        private static void TakeComponent(ENetPlayer player, int weaponId, int componentSlot, int dropSlot)
        {
            try
            {
                if (!Inventory.GetInventory(player.GetUUID(), out var storage)) return;

                var item = Inventory.FindItem(player, weaponId);
                if (item is null) return;

                if(item is Inv.Items.WeaponItem weaponItem)
                {
                    var _weaponItemData = InvItems.Get(weaponItem.Type);
                    if (_weaponItemData is null || _weaponItemData.ItemType != ItemType.Weapon) return;

                    if (Inventory.CheckCanAddItem(player, new Item(ItemId.ExtendedClip, 1), dropSlot))
                    {
                        player.SendError("Недостаточно места в инвентаре");
                        return;
                    }

                    ItemId componentId = ItemId.Debug;
                    switch (componentSlot)
                    {
                        case 1:
                            if (!weaponItem.Components.Clip) return;
                            weaponItem.Components.Clip = false;
                            componentId = ItemId.ExtendedClip;
                            break;
                        case 2:
                            if (!weaponItem.Components.Flashlight) return;
                            weaponItem.Components.Flashlight = false;
                            componentId = ItemId.FlashlightComponent;
                            break;
                        case 3:
                            if (!weaponItem.Components.Grip) return;
                            weaponItem.Components.Grip = false;
                            componentId = ItemId.Grip;
                            break;
                        case 4:
                            if (weaponItem.Components.HolographicSight)
                            {
                                weaponItem.Components.HolographicSight = false;
                                componentId = ItemId.HolographicSight;
                            }

                            else if (weaponItem.Components.Scope)
                            {
                                weaponItem.Components.Scope = false;
                                componentId = ItemId.Scope;
                            }

                            else return;

                            break;
                        case 5:
                            if (!weaponItem.Components.Suppressor) return;
                            weaponItem.Components.Suppressor = false;
                            componentId = ItemId.Suppressor;
                            break;
                        default: return;
                    }
                    if (componentId == ItemId.Debug) return;

                    weaponItem.RefreshParams();
                    _ = Inventory.CreateNewItem(player, TypedItems.Get(new Item(componentId, 1)), dropSlot);
                    if (weaponItem.IsActive)
                        WeaponController.GiveWeapon(player, weaponItem.Type, weaponItem.Components, true);

                    SendItems(player);
                }
            }
            catch (Exception ex) { Logger.WriteError("TakeComponent", ex); }
        }

        public static object GetItemData(Item item)
        {
            var data = InvItems.Get(item.Type);
            return new
            {
                Type = item.Type,
                Slot = item.Slot,
                Name = data != null ? data.Name : "undefined",
                Description = data != null ? data.Description : "undefined",
                Picture = data != null ? data.Picture : "null",
                Count = item.Count,
                Data = item.Data,
                ItemType = InvItems.GetType(item.Type).ToString(),
                IsActive = item.IsActive,
                Rarity = data.Rarity.ToString(),
                Weight = data.Weight,
            };
        }

        public static int GetMaxSlots(InventoryType type, params object[] args)
        {
            switch (type)
            {
                case InventoryType.Player: return 30;
                case InventoryType.Backpack:
                    {
                        if (args.Length != 1 || !(args[0] is ENetPlayer)) return 0;
                        return InvBackpack.GetMaxSlots((ENetPlayer)args[0]);
                    }
                case InventoryType.Vehicle:
                    {
                        // TODO: Vehicle Capacity
                        if (args.Length == 0 || !((args[0] is string) || (args[0] is uint))) return 0;
                        if (args[0] is string)
                            return VehcileCapacity.Get(Convert.ToString(args[0]));
                        else if (args[0] is uint)
                            return VehcileCapacity.Get((uint)args[0]);

                        return 0;
                    }
            }
            return 0;
        }

        public static void Close(ENetPlayer player) => ClientEvent.Event(player, "client.inventory.close");
    }
}
