using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Mute.Models
{
    /// <summary>
    /// Класс определяющий содеражение системы Mute
    /// </summary>
    /// <param name="PlayerUUID"> Статический ID игрока имеющий мут. </param>
    /// <param name="AdminUUID"> Статический ID администратора выдавшего мут. </param>
    /// <param name="Reason"> Причина мута </param>
    /// <param name="MinutesLeft"> Время до окончания </param>
    public class MuteInfo
    {
        public int PlayerUUID { get; set; }
        public int AdminUUID { get; set; }
        public string Reason { get; set; }
        public uint MinutesLeft { get; set; }
    }
}
