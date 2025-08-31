using eNetwork;
using eNetwork.Inv;
using eNetwork.Jobs.Fishing.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEvo.Jobs.Fishing.Classes
{
    public class RodData
    {
        /// <summary>
        /// Количество единиц, расходуемых при ловле
        /// </summary>
        public double Wear { get; set; }

        /// <summary>
        /// Минимальное время ожидания для улова
        /// </summary>
        public int MinTime { get; set; }

        /// <summary>
        /// Максимальное время ожидания для улова
        /// </summary>
        public int MaxTime { get; set; }   

        public RodData(double wear, int minTime, int maxTime)
        {
            Wear = wear;
            MinTime = minTime;
            MaxTime = maxTime;
        }

        public static bool Get(ItemId itemId, out RodData data)
            => Config.RODS_DATA.TryGetValue(itemId, out data);
    }
}
