using eNetwork.Framework;
using eNetwork.Gambles.Lotteries;
using eNetwork.Game.Weapons;
using eNetwork.GameUI.Phone;
using eNetwork.Inv;
using eNetwork.Inv.Items;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static eNetwork.Configs.Weapons;

namespace eNetwork.Game
{
    public class InvUse
    {
        private static readonly Logger Logger = new Logger("inventory-use");

        [CustomEvent("server.inventory.items.use")]
        public static void ItemUsing(ENetPlayer player, string currentType, int itemId, int dropSlot, bool isSwapping)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData character) || (currentType != "main" && currentType != "clothes") || !Inventory.GetInventory(player.GetUUID(), out var items)) return;
                var item = Inventory.FindItem(player, itemId);

                ItemUsing(player, currentType, item, dropSlot, isSwapping);
            }
            catch (Exception ex) { Logger.WriteError("ItemUsing", ex); }
        }
        
        public static void ItemUsing(ENetPlayer player, string currentType, Item item, int dropSlot, bool isSwapping)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData character) || (currentType != "main" && currentType != "clothes") || !Inventory.GetInventory(player.GetUUID(), out var storage)) return;

                if (item is null)
                {
                    player.SendError("Ошибка использования предмета");
                    return;
                }

                if (!isSwapping && dropSlot != -1)
                {
                    var dropItem = Inventory.FindItemBySlot(storage, dropSlot);
                    if (dropItem != null) return;
                }

                int newSlot = 0;
                //ItemData dataItem = InvItems.Get(item.Type);
                //if (dataItem is null)
                //{
                //    player.SendError("Ошибка получения информации о предмете");
                //    return;
                //}
                //ItemType ItemType = InvItems.GetType(item.Type);
                switch (item.ItemData.ItemType)
                {
                    case ItemType.Ammo: return;
                    case ItemType.Melee:
                    case ItemType.Weapon:
                        if(item is Inv.Items.WeaponItem weaponItem)
                        {
                            if (weaponItem.Wear <= 1)
                            {
                                player.SendWarning("Оружие сломалось");
                                Inventory.RemoveItem(player, item, delete: true);
                                return;
                            }
                            if (item.IsActive)
                            {
                                player.RemoveItemHand();
                                item.IsActive = false;

                                WeaponController.RemoveWeapon(player);
                                ChatHandler.SendRPMessage(player, $"<span style=\"color: #B773BC\">[ME]</span> <span style=\"color: #F796FF\">{player.GetName()} {Helper.GenderString("убрал", player.Gender)} из рук {weaponItem.ItemData.Name}</span>");

                                if (item.Slot < 0)
                                    WeaponAttachment.SetWeaponInFastSlot(player, item.Type, weaponItem.Components, (item.Slot + 100) * -1);
                            }
                            else
                            {
                                var currentWeapon = WeaponController.GetCurrentWeapon(storage);
                                if (currentWeapon != null)
                                {
                                    if(currentWeapon is Inv.Items.WeaponItem currentWeaponItem)
                                    {
                                        player.RemoveItemHand();
                                        currentWeapon.IsActive = false;
                                        ChatHandler.SendRPMessage(player, $"<span style=\"color: #B773BC\">[ME]</span> <span style=\"color: #F796FF\">{player.GetName()} {Helper.GenderString("убрал", player.Gender)} из рук {currentWeaponItem.ItemData.Name}</span>");
                                        WeaponAttachment.SetWeaponInFastSlot(player, currentWeaponItem.Type, currentWeaponItem.Components, (currentWeapon.Slot + 100) * -1);
                                    }
                                    //ItemData currentWeaponDataItem = InvItems.Get(currentWeapon.Type);
                                    //player.RemoveItemHand();
                                    //currentWeapon.IsActive = false;
                                    //ChatHandler.SendRPMessage(player, $"<span style=\"color: #B773BC\">[ME]</span> <span style=\"color: #F796FF\">{player.GetName()} {Helper.GenderString("убрал", player.Gender)} из рук {currentWeaponDataItem.Name}</span>");
                                    //WeaponAttachment.SetWeaponInFastSlot(player, currentWeapon.Type, JsonConvert.DeserializeObject<InvWeaponData>(currentWeapon.Data).WeaponComponents, (currentWeapon.Slot + 100) * -1);
                                }

                                if (player.GetItemHand() != ItemId.Debug)
                                {
                                    player.SendError("Сначала уберите предмет из рук!");
                                    return;
                                }

                                WeaponController.GiveWeapon(player, weaponItem.Type, weaponItem.Components);
                                WeaponAttachment.RemoveWeaponFromFastSlot(player, weaponItem.Type, (weaponItem.Slot + 100) * -1);

                                player.SetItemHand(weaponItem.Type);
                                item.IsActive = true;

                                ChatHandler.SendRPMessage(player, $"<span style=\"color: #B773BC\">[ME]</span> <span style=\"color: #F796FF\">{player.GetName()} {Helper.GenderString("взял", player.Gender)} в руки {weaponItem.ItemData.Name}</span>");
                            }
                        }
                        break;
                    case ItemType.Clothes:
                        {
                            if(item is ClotheItem clotheItem)
                            {
                                if (item.IsActive && !isSwapping)
                                {
                                    if (dropSlot == -1)
                                    {
                                        newSlot = Inventory.GetFreeSlot(storage, InventoryType.Player);
                                        if (newSlot == -1)
                                        {
                                            player.SendError("Нет свободных слотов в инвентаре");
                                            return;
                                        }
                                    }
                                }

                                //InvClothesData clothesData = JsonConvert.DeserializeObject<InvClothesData>(item.Data);
                                
                                Gender gender = clotheItem.ClotheModel.Gender == 1 ? Gender.Male : Gender.Female;
                                if (gender != character.CustomizationData.Gender && clotheItem.ClotheModel.Gender != 2)
                                {
                                    player.SendError("Данная одежда предназначена не для вашего пола");
                                    InvInterface.SendItems(player);
                                    return;
                                }

                                if (!isSwapping)
                                {
                                    var tempClothesItem = Inventory.FindItem(player, -(int)item.Type);
                                    if (!item.IsActive && tempClothesItem != null)
                                    {
                                        InvInterface.ItemSwap(player, item.Slot, currentType, tempClothesItem.Slot, "clothes");
                                        return;
                                    }
                                }

                                switch (item.Type)
                                {
                                    case ItemId.Top:
                                        bool undershirtIsTop = false;
                                        if (item.IsActive)
                                        {
                                            var emptySlots = Configs.CustomizationConfig.EmptySlots[gender][item.Type];
                                            character.CustomizationData.Clothes.Top = new ComponentData(emptySlots[0], emptySlots[1]);
                                            character.CustomizationData.Clothes.Torso = new ComponentData(15, 0);

                                            int torsoUndershirt = 15;
                                            if (Configs.CustomizationConfig.Undershirts[gender][15].TryGetValue(character.CustomizationData.Clothes.Undershit.DrawableId, out Configs.CustomizationConfig.Undershirt topVariation))
                                            {
                                                player.SetClothes(11, topVariation.Top, character.CustomizationData.Clothes.Undershit.TextureId);
                                                player.SetClothes(3, Configs.CustomizationConfig.EmptySlots[gender][ItemId.Undershirt][0], 0);

                                                undershirtIsTop = true;

                                                if (Configs.CustomizationConfig.Tops[gender].TryGetValue(topVariation.Top, out Configs.CustomizationConfig.Top topData))
                                                    torsoUndershirt = topData.Torso;
                                            }
                                            else
                                            {
                                                player.SetClothes(8, character.CustomizationData.Clothes.Undershit.DrawableId, character.CustomizationData.Clothes.Undershit.TextureId);
                                            }

                                            character.CustomizationData.Clothes.Torso = new ComponentData(torsoUndershirt, 0);
                                            player.SetClothes(3, torsoUndershirt, 0);

                                            item.IsActive = false;
                                        }
                                        else
                                        {
                                            int torsoVariation = 15;
                                            if (Configs.CustomizationConfig.Tops[gender].TryGetValue(clotheItem.ClotheModel.DrawableId, out Configs.CustomizationConfig.Top topData))
                                                torsoVariation = topData.Torso;

                                            var emptySlotsUndershirt = Configs.CustomizationConfig.EmptySlots[gender][ItemId.Undershirt];
                                            int undershirtVariation = character.CustomizationData.Clothes.Undershit.DrawableId;
                                            if (character.CustomizationData.Clothes.Undershit.DrawableId != emptySlotsUndershirt[0])
                                            {
                                                if (!Configs.CustomizationConfig.Undershirts[gender][torsoVariation].ContainsKey(character.CustomizationData.Clothes.Undershit.DrawableId))
                                                {
                                                    player.SendError("Невозможно надеть эту одежду поверх другой");
                                                    InvInterface.SendItems(player);
                                                    return;
                                                }

                                                if (topData != null)
                                                    torsoVariation = topData.TorsoWithUnder;

                                                if (topData != null && topData.IsUnderCropped == true)
                                                {
                                                    if (Configs.CustomizationConfig.Undershirts[gender][torsoVariation].TryGetValue(character.CustomizationData.Clothes.Undershit.DrawableId, out Configs.CustomizationConfig.Undershirt undershirtData) && undershirtData.Cropped != -1)
                                                    {
                                                        undershirtVariation = undershirtData.Cropped;
                                                    }
                                                    else
                                                    {
                                                        player.SendError("Невозможно надеть эту одежду поверх другой 2");
                                                        InvInterface.SendItems(player);
                                                        return;
                                                    }
                                                }
                                            }


                                            player.SetClothes(8, undershirtVariation, character.CustomizationData.Clothes.Undershit.TextureId);

                                            character.CustomizationData.Clothes.Top = new ComponentData(clotheItem.ClotheModel.DrawableId, clotheItem.ClotheModel.TextureId);
                                            character.CustomizationData.Clothes.Torso = new ComponentData(torsoVariation, 0);

                                            player.SetClothes(3, character.CustomizationData.Clothes.Torso.DrawableId, character.CustomizationData.Clothes.Torso.TextureId);

                                            item.IsActive = true;
                                            item.Slot = -(int)item.Type;
                                        }

                                        if (!undershirtIsTop)
                                        {
                                            player.SetClothes(11, character.CustomizationData.Clothes.Top.DrawableId, character.CustomizationData.Clothes.Top.TextureId);
                                            player.SetClothes(3, character.CustomizationData.Clothes.Torso.DrawableId, character.CustomizationData.Clothes.Torso.TextureId);
                                        }

                                        break;
                                    case ItemId.Undershirt:
                                        var emptySlotsTop = Configs.CustomizationConfig.EmptySlots[gender][ItemId.Top];

                                        if (character.CustomizationData.Clothes.Top.DrawableId == emptySlotsTop[0])
                                        {
                                            if (!item.IsActive)
                                            {
                                                character.CustomizationData.Clothes.Undershit = new ComponentData(clotheItem.ClotheModel.DrawableId, clotheItem.ClotheModel.TextureId);

                                                int torsoUndershirt = 15;
                                                if (Configs.CustomizationConfig.Undershirts[gender][15].TryGetValue(clotheItem.ClotheModel.DrawableId, out Configs.CustomizationConfig.Undershirt undershirtData))
                                                {
                                                    player.SetClothes(11, undershirtData.Top, clotheItem.ClotheModel.TextureId);
                                                    if (Configs.CustomizationConfig.Tops[gender].TryGetValue(undershirtData.Top, out Configs.CustomizationConfig.Top topData))
                                                        torsoUndershirt = topData.Torso;
                                                }
                                                else
                                                {
                                                    player.SetClothes(8, character.CustomizationData.Clothes.Undershit.DrawableId, character.CustomizationData.Clothes.Undershit.TextureId);
                                                }

                                                player.SetClothes(3, torsoUndershirt, 0);
                                                character.CustomizationData.Clothes.Torso = new ComponentData(torsoUndershirt, 0);

                                                item.IsActive = true;
                                                item.Slot = -(int)item.Type;
                                            }
                                            else
                                            {
                                                var emptySlots = Configs.CustomizationConfig.EmptySlots[gender][item.Type];
                                                character.CustomizationData.Clothes.Undershit = new ComponentData(emptySlots[0], emptySlots[1]);
                                                character.CustomizationData.Clothes.Torso = new ComponentData(15, 0);

                                                item.IsActive = false;
                                                player.SetClothes(3, 15, 0);
                                                player.SetClothes(8, character.CustomizationData.Clothes.Undershit.DrawableId, character.CustomizationData.Clothes.Undershit.TextureId);
                                                player.SetClothes(11, emptySlotsTop[0], emptySlotsTop[1]);
                                            }
                                        }
                                        else
                                        {
                                            player.SendError("Сначала снимите верхнюю одежду!");
                                            InvInterface.SendItems(player);
                                            return;
                                        }
                                        break;
                                    case ItemId.Pants:
                                        if (item.IsActive)
                                        {
                                            var emptySlots = Configs.CustomizationConfig.EmptySlots[gender][item.Type];
                                            character.CustomizationData.Clothes.Leg = new ComponentData(emptySlots[0], emptySlots[1]);

                                            item.IsActive = false;
                                        }
                                        else
                                        {
                                            character.CustomizationData.Clothes.Leg = new ComponentData(clotheItem.ClotheModel.DrawableId, clotheItem.ClotheModel.TextureId);

                                            item.IsActive = true;
                                            item.Slot = -(int)item.Type;
                                        }

                                        player.SetClothes(4, character.CustomizationData.Clothes.Leg.DrawableId, character.CustomizationData.Clothes.Leg.TextureId);
                                        break;
                                    case ItemId.Shoes:
                                        if (item.IsActive)
                                        {
                                            var emptySlots = Configs.CustomizationConfig.EmptySlots[gender][item.Type];
                                            character.CustomizationData.Clothes.Feet = new ComponentData(emptySlots[0], emptySlots[1]);

                                            item.IsActive = false;
                                        }
                                        else
                                        {
                                            character.CustomizationData.Clothes.Feet = new ComponentData(clotheItem.ClotheModel.DrawableId, clotheItem.ClotheModel.TextureId);

                                            item.IsActive = true;
                                            item.Slot = -(int)item.Type;
                                        }

                                        player.SetClothes(6, character.CustomizationData.Clothes.Feet.DrawableId, character.CustomizationData.Clothes.Feet.TextureId);
                                        break;
                                    case ItemId.Bag:
                                        if (item is BackpackItem backpack)
                                        {
                                            _ = Task.Run(async () =>
                                            {
                                                if (item.IsActive)
                                                {
                                                    var freeSlot = Inventory.GetFreeSlot(storage, InventoryType.Player);
                                                    if(freeSlot == -1)
                                                    {
                                                        player.SendError("Недостаточно места в инвентаре");
                                                        return;
                                                    }
                                                    var emptySlots = Configs.CustomizationConfig.EmptySlots[gender][item.Type];
                                                    character.CustomizationData.Clothes.Bag = new ComponentData(emptySlots[0], emptySlots[1]);

                                                    item.IsActive = false;
                                                    item.Slot = freeSlot;
                                                    InvInterface.SendItems(player);
                                                }
                                                else
                                                {
                                                    character.CustomizationData.Clothes.Bag = new ComponentData(backpack.ClotheModel.DrawableId, backpack.ClotheModel.TextureId);

                                                    item.IsActive = true;
                                                    item.Slot = -(int)item.Type;
                                                    
                                                    //загрузка данных из БД (возможно стоит поднимать кэш)
                                                    await InvBackpack.LoadItems(backpack);
                                                    InvInterface.SendItems(player);
                                                    //InvBackpack.SendItems(player, backpack);
                                                }
                                                NAPI.Task.Run(() =>
                                                {
                                                    player.SetClothes(5, character.CustomizationData.Clothes.Bag.DrawableId, character.CustomizationData.Clothes.Bag.TextureId);
                                                });
                                                
                                            });
                                        }
                                        break;
                                    case ItemId.Accessories:
                                        if (item.IsActive)
                                        {
                                            var emptySlots = Configs.CustomizationConfig.EmptySlots[gender][item.Type];
                                            character.CustomizationData.Clothes.Accessory = new ComponentData(emptySlots[0], emptySlots[1]);

                                            item.IsActive = false;
                                        }
                                        else
                                        {
                                            character.CustomizationData.Clothes.Accessory = new ComponentData(clotheItem.ClotheModel.DrawableId, clotheItem.ClotheModel.TextureId);

                                            item.IsActive = true;
                                            item.Slot = -(int)item.Type;
                                        }

                                        player.SetClothes(7, character.CustomizationData.Clothes.Accessory.DrawableId, character.CustomizationData.Clothes.Accessory.TextureId);
                                        break;
                                    case ItemId.BodyArmor:
                                        if (item.IsActive)
                                        {
                                            var emptySlots = Configs.CustomizationConfig.EmptySlots[gender][item.Type];
                                            character.CustomizationData.Clothes.Bodyarmor = new ComponentData(emptySlots[0], emptySlots[1]);

                                            item.IsActive = false;
                                        }
                                        else
                                        {
                                            character.CustomizationData.Clothes.Bodyarmor = new ComponentData(clotheItem.ClotheModel.DrawableId, clotheItem.ClotheModel.TextureId);

                                            item.IsActive = true;
                                            item.Slot = -(int)item.Type;
                                        }

                                        player.SetClothes(9, character.CustomizationData.Clothes.Bodyarmor.DrawableId, character.CustomizationData.Clothes.Bodyarmor.TextureId);
                                        break;
                                }

                                if (!item.IsActive)
                                    item.Slot = newSlot;

                            }
                        }
                        break;

                    case ItemType.Food:
                        switch (item.Type)
                        {
                            case ItemId.Burger:

                                break;
                        }
                        break;

                    case ItemType.Default:
                        switch (item.Type)
                        {
                            case ItemId.IdCard:
                                GameUI.Documents.Instance.ShowIdCard(player, player);
                                break;

                            case ItemId.LotteryTicket:
                                // TODO: Lottery Ticket Use
                                Lottery.Instance.UseTicket(player, item);
                                break;
                            //case ItemId.Programmer:
                            //    Factions.Tasks.CarTheft.CarTheftManager.UseProgrammer(player, item);
                            //    return;

                            case ItemId.Rod:
                            case ItemId.RodUpgraded:
                            case ItemId.RodMk2:
                                Jobs.Fishing.FishingManager.UseRod(player, item);
                                return;

                            case ItemId.Phone:
                                InvInterface.Close(player);

                                if (player.HasSharedData("phone.opened") && player.GetSharedData<bool>("phone.opened"))
                                    PhoneManager.ClosePhone(player);
                                else
                                    PhoneManager.OpenPhone(player);
                                break;

                            case ItemId.FirstAidKit:
                                if (player.GetItemHand() != ItemId.Debug)
                                {
                                    player.SendError("Сначала уберите предмет из рук!");
                                    return;
                                }

                                if (player.HasData("item.health.data"))
                                {
                                    player.SendWarning("Вы уже используете аптечку");
                                    return;
                                }

                                if (!player.IsTimeouted("health.timeout", 10))
                                {
                                    player.SendWarning("Не так часто!");
                                    return;
                                }

                                if (player.Health >= 100)
                                {
                                    player.SendWarning("У вас полное здоровье!");
                                    return;
                                }

                                player.PlayScenario(ScenarioType.HelthAidKit);
                                player.AddAttachment("health_pack");

                                player.SetItemHand(item.Type);

                                int _healthCount = 65;
                                player.SetData("item.health.data", _healthCount);

                                Inventory.RemoveItem(player, item, 1);
                                break;
                        }
                        break;

                    case ItemType.Tools:
                        {
                            switch (item.Type)
                            {
                                case ItemId.PepperSpray:
                                    if (!item.IsActive)
                                    {
                                        player.AddAttachment("pepper_spray");
                                        player.SetSharedData("isPepper.active", true);
                                        item.IsActive = true;
                                    }
                                    else
                                    {
                                        player.AddAttachment("pepper_spray", true);
                                        player.SetSharedData("isPepper.active", false);
                                        item.IsActive = false;
                                    }
                                    break;
                            }
                        }
                        break;
                }

                if (dropSlot != -1)
                {
                    item.Slot = dropSlot;
                }

                //InvInterface.Update(player, "main", items, item);
                InvInterface.SendItems(player);
            }
            catch (Exception e) { Logger.WriteError("ItemUsing", e); }
        }
    }
}