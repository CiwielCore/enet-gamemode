using eNetwork.Inv;
using eNetwork.Inv.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game
{
    /// <summary>
    /// типизация всех предметов для инвентаря
    /// </summary>
    public static class TypedItems
    {
        public static Item Get(Item item)
        {
            var typedItem = item;
            switch (item.Type)
            {
                //Под одиночные удобнее через case ItemId
                //case ItemId.Masks:
                //case ItemId.Shoes:
                //case ItemId.Accessories:
                //case ItemId.Undershirt:
                //case ItemId.BodyArmor:
                //case ItemId.Top:
                //case ItemId.Hat:
                //case ItemId.Glasses:
                //case ItemId.Ears:
                //case ItemId.Watches:
                //case ItemId.Bracelets:
                //case ItemId.Bag:
                //case ItemId.Gloves:
                //    {
                //        return new Inv.Items.ClotheItem(item);
                //    }
                //
                case ItemId.Bag:
                    {
                        typedItem = new BackpackItem(typedItem);
                        
                        return typedItem;
                    }
                //под большую выборку удобнее через default
                default:
                    {
                        if ((int)item.Type >= 1 && (int)item.Type <= 14) //is clothes
                        {
                            return new ClotheItem(typedItem);
                        }
                        if ((int)item.Type >= 15 && (int)item.Type <= 81) //is weapon
                        {
                            return new WeaponItem(typedItem);
                        }
                        break;
                    }
            }
            return typedItem;
        }
    }
}
