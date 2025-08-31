using eNetwork.Inv;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Clothes
{
    public class ClotheModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }
        public bool IsProp { get; set; }
        public int ComponentId { get; set; }
        public int DrawableId { get; set; }
        public int TextureId { get; set; }
        public int UndershirtId { get; set; }
        public int TorseId { get; set; }
        public int Price { get; set; }
        public ItemId ItemId { get; set; } = ItemId.Debug;
        public ClotheModel(int id, string name, int gender, int isProp, int componentId, int drawableId, int textureId, int undershirtId, int torseId, int price)
        {
            Id = id;
            Name = name;
            Gender = gender;
            IsProp = isProp == 1 ? true : false;
            ComponentId = componentId;
            DrawableId = drawableId;
            TextureId = textureId;
            UndershirtId = undershirtId;
            TorseId = torseId;
            Price = price;
            ItemId = getItemId();
        }
        private ItemId getItemId()
        {
            if (IsProp == false)
            {
                switch (ComponentId)
                {
                    case 1: return ItemId.Masks;
                    case 4: return ItemId.Pants;
                    case 5: return ItemId.Bag;
                    case 6: return ItemId.Shoes;
                    case 7: return ItemId.Accessories;
                    case 8: return ItemId.Undershirt;
                    case 9: return ItemId.BodyArmor;
                    case 11: return ItemId.Top;
                }
            } else
            {
                switch (ComponentId)
                {
                    case 0: return ItemId.Hat;
                    case 1: return ItemId.Glasses;
                    case 2: return ItemId.Ears;
                    case 6: return ItemId.Watches;
                    case 7: return ItemId.Bracelets;
                }
            }
            return ItemId.Debug;
        }
    }
}
