using eNetwork;
using eNetwork.Inv;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Jobs.Fishing.Classes
{
    public class FishItemData
    {
        /// <summary>
        /// Тип предмета
        /// </summary>
        public ItemId ItemId { get; set; }

        /// <summary>
        /// Минимальный уровень, чтобы выловить эту рыбу
        /// </summary>
        public int MinLvl { get; set; }

        /// <summary>
        /// Шанс, с которым можно выловить рыбку
        /// </summary>
        public int Chance { get; set; }

        /// <summary>
        /// Минимальная цена продажи рыбы
        /// </summary>
        public int MinPrice { get; set; }
        
        /// <summary>
        /// Максимальная цена продажи рыбы
        /// </summary>
        public int MaxPrice { get; set; }

        /// <summary>
        /// Донатная рыба или нет? 
        /// 
        /// True - продается за донат
        /// False - продается за доллары
        /// </summary>
        public bool IsDonate { get; set; }

        /// <summary>
        /// Список удочек, которыми можно выловить рыбу
        /// null - всеми видами
        /// </summary>
        public List<ItemId> Rods { get; set; }

        public FishItemData(ItemId itemId, int minLvl, int chance, int minPrice, int maxPrice, List<ItemId> rods = null, bool isDonate = false)
        {
            ItemId = itemId;
            MinLvl = minLvl;
            Chance = chance;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            Rods = rods is null ? new List<ItemId>() { ItemId.Rod, ItemId.RodUpgraded, ItemId.RodMk2 } : rods;
            IsDonate = isDonate;
        }
    }
}
