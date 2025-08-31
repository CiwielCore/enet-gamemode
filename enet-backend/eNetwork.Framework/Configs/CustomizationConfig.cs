using eNetwork.Framework;
using eNetwork.Inv;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Configs
{
    public class CustomizationConfig
    {
        public static IReadOnlyDictionary<Gender, Dictionary<int, Dictionary<int, Undershirt>>> Undershirts = new Dictionary<Gender, Dictionary<int, Dictionary<int, Undershirt>>>()
        {
            { Gender.Male, new Dictionary<int, Dictionary<int, Undershirt>>() {
                    // undershirt / top
                    { 0, new Dictionary<int, Undershirt>()
                        {
                            { 0, new Undershirt(top: 0, cropped: 2) },
                        }
                    },
                    { 1, new Dictionary<int, Undershirt>()
                        {
                            { 1, new Undershirt(top: 1, cropped: 14) },
                        }
                    },
                    { 4, new Dictionary<int, Undershirt>()
                        {
                            { 72, new Undershirt(top: 111, cropped: 71) },
                        }
                    },
                    { 5, new Dictionary<int, Undershirt>()
                        {
                            
                        }
                    },
                    { 14, new Dictionary<int, Undershirt>() {
                            { 0, new Undershirt(top: 0, cropped: 2) },
                            { 1, new Undershirt(top: 1, cropped: 14) },
                            { 72, new Undershirt(top: 111, cropped: 71) },
                        }
                    },
                    { 15, new Dictionary<int, Undershirt>() {
                            { 0, new Undershirt(top: 0, cropped: 2) },
                            { 1, new Undershirt(top: 1, cropped: 14) },
                            { 72, new Undershirt(top: 111, cropped: 71) },
                        } 
                    }
                }
            }
        };

        public static IReadOnlyDictionary<Gender, Dictionary<int, Top>> Tops = new Dictionary<Gender, Dictionary<int, Top>>()
        {
            { Gender.Male, new Dictionary<int, Top>()
                {
                    { 0, new Top(variation: 0, torso: 0, rarity: ItemRarity.Common) },  // undershirt
                    { 1, new Top(variation: 1, torso: 0, rarity: ItemRarity.Common) },  // undershirt
                    { 3, new Top(variation: 0, torso: 14, rarity: ItemRarity.Common) },
                    { 4, new Top(variation: 4, torso: 14, rarity: ItemRarity.Common) },
                    { 5, new Top(variation: 5, torso: 5, rarity: ItemRarity.Common) },
                    { 6, new Top(variation: 6, torso: 14, isUnderCropped: true, rarity: ItemRarity.Common) },
                    { 35, new Top(variation: 35, torso: 14, torsoWithUnder: 4, rarity: ItemRarity.Common) },
                    { 111, new Top(variation: 111, torso: 4, rarity: ItemRarity.Common) },
                }
            }
        };

        public static readonly Dictionary<Gender, Dictionary<ItemId, int[]>> EmptySlots = new Dictionary<Gender, Dictionary<ItemId, int[]>>()
        {
             { Gender.Male, new Dictionary<ItemId, int[]>() {
                { ItemId.Masks, new int[] { 0, 0 } },
                { ItemId.Pants, new int[] { 21, 0 } },
                { ItemId.Shoes, new int[] { 34, 0 } },
                { ItemId.Accessories, new int[] { 0, 0 } },
                { ItemId.Undershirt, new int[] { 15, 0 } },
                { ItemId.BodyArmor, new int[] { 0, 0 } },
                { ItemId.Top, new int[] { 15, 0 } },
                { ItemId.Hat, new int[] { 0, 0 } },
                { ItemId.Glasses, new int[] { 0, 0 } },
                { ItemId.Ears, new int[] { 0, 0 } },
                { ItemId.Watches, new int[] { 0, 0 } },
                { ItemId.Bracelets, new int[] { 0, 0 } },
                { ItemId.Bag, new int[] { 0, 0 } },
                { ItemId.Gloves, new int[] { 0, 0 } },
            }},
            { Gender.Female, new Dictionary<ItemId, int[]>() {
                { ItemId.Masks, new int[] { 0, 0 } },
                { ItemId.Pants, new int[] { 15, 0 } },
                { ItemId.Shoes, new int[] { 35, 0 } },
                { ItemId.Accessories, new int[] { 0, 0 } },
                { ItemId.Undershirt, new int[] { 6, 0 } },
                { ItemId.BodyArmor, new int[] { 0, 0 } },
                { ItemId.Top, new int[] { 15, 0 } },
                { ItemId.Hat, new int[] { 0, 0 } },
                { ItemId.Glasses, new int[] { 0, 0 } },
                { ItemId.Ears, new int[] { 0, 0 } },
                { ItemId.Watches, new int[] { 0, 0 } },
                { ItemId.Bracelets, new int[] { 0, 0 } },
                { ItemId.Bag, new int[] { 0, 0 } },
                { ItemId.Gloves, new int[] { 0, 0 } },
            }}
        };

        public class Top
        {
            public int Variation;
            public int Torso;
            public int TorsoWithUnder;
            public bool IsUnderCropped;
            public ItemRarity Rarity;

            public Top(int variation, int torso, ItemRarity rarity, bool isUnderCropped = false, int torsoWithUnder = -1)
            {
                Variation = variation;
                Torso = torso;
                Rarity = rarity;
                IsUnderCropped = isUnderCropped;
                TorsoWithUnder = torsoWithUnder == -1 ? torso : torsoWithUnder;
            }
        }
        public class Undershirt
        {
            public int Top;
            public int Cropped;
            public Undershirt(int top, int cropped = -1)
            {
                Top = top;
                Cropped = cropped;
            }
        }
    }
}
