using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork
{
    public class AccountData
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string EMail { get; set; }
        public string IP { get; set; }
        public string HWID { get; set; }
        public string SocialClub { get; set; }
        public ulong SocialId { get; set; }
        public DateTime LastLogin { get; set; } = DateTime.Now;
        public List<int> Characters { get; set; } = new List<int> { -1, -1, -2 };
        public long DonatePoints { get; set; } = 0;
        public long BonusPoints { get; set; } = 0;
        public string Promocode { get; set; } = String.Empty;

        public bool IsLogined { get; set; } = false;
        public ENetPlayer Player { get; set; } = null;
    }
}
