using Newtonsoft.Json;

namespace eNetwork.Inv
{
    public class ItemData
    {
        public ItemId Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public uint Model { get; set; }
        public string Picture { get; set; }
        public int Stack { get; set; }
        public int Weight { get; set; }
        public ItemRarity Rarity { get; set; }
        public ItemType ItemType { get; set; } = ItemType.Default;
        public ItemData(ItemId type, string name, string description, uint model, string picture, int weight, int stack, ItemRarity rarity, ItemType itemType)
        {
            Type = type;
            Name = name;
            Description = description;
            Model = model;
            Picture = picture;
            Weight = weight;
            Stack = stack;
            Rarity = rarity;
            ItemType = itemType;
        }

    }
}
