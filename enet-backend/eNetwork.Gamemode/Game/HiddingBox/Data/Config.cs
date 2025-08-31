using eNetwork.Game.HiddingBox.Classes;
using eNetwork.Inv;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.HiddingBox.Data
{
    public class Config
    {
        /// <summary>
        /// Вес тайника
        /// </summary>
        public static readonly int BOX_WEIGHT = 100000;

        /// <summary>
        /// Время разблокировки тайника
        /// </summary>
        public static readonly int TIME_TO_UNLOCK_BOX = 5;

        /// <summary>
        /// Минимальное время для рефреша
        /// </summary>
        public static readonly int MIN_TIME_TO_REFRESH = 50;

        /// <summary>
        /// Максимальное время для рефреша
        /// </summary>
        public static readonly int MAX_TIME_TO_REFRESH = 80;

        /// <summary>
        /// Список лута в тайниках
        /// </summary>
        public static readonly Dictionary<HiddenBoxType, HiddenBoxSettings> HIDDEN_BOX_SETTINGS = new Dictionary<HiddenBoxType, HiddenBoxSettings>()
        {
            { HiddenBoxType.Medicine, new HiddenBoxSettings() 
                {
                    Name = "Медицинский ящик",
                    Model = NAPI.Util.GetHashKey("prop_box_wood02a"),
                    Items = new List<ItemId>()
                    {
                        ItemId.Burger,
                        ItemId.Bandages,
                        ItemId.FirstAidKit
                    }
                }
            },
        };

        public static readonly List<HiddenBox> HIDDEN_BOXES = new List<HiddenBox>()
        {
            new HiddenBox(HiddenBoxType.Medicine, new Position(462.20685, -3228.6704, 4.949564, heading: -170)),
        };
    }
}
