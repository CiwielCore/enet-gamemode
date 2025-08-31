using System;

namespace eNetwork.Framework.Classes.Faction.Ranks
{
    public class RanksData(string rankName, int payDay, int lvl, RanksPermission permission)
    {
        public int Lvl { get; set; } = lvl;

        public int PayDay { get; set; } = payDay;

        public RanksPermission Permission { get; set; } = permission;
        public string RankName { get; set; } = rankName;
    }

    public class RanksPermission(bool addRank, bool changeFactionNews, bool changePlayerRank, bool changeStockKits,
                                bool changeStockStatus, bool changePlayerTag, bool invite, bool removeRank, bool renameRank, bool unInvite, bool cuff)
    {
        public bool AddRank { get; set; } = addRank;
        public bool ChangeFactionNews { get; set; } = changeFactionNews;
        public bool ChangePlayerRank { get; set; } = changePlayerRank;
        public bool ChangePlayerTag { get; set; } = changePlayerTag;
        public bool ChangeStockKits { get; set; } = changeStockKits;
        public bool ChangeStockStatus { get; set; } = changeStockStatus;
        public bool Cuff { get; set; } = cuff;
        public bool Invite { get; set; } = invite;
        public bool RemoveRank { get; set; } = removeRank;
        public bool RenameRank { get; set; } = renameRank;
        public bool UnInvite { get; set; } = unInvite;

        public bool GetPropertyValue(string propertyName)
        {
            return Convert.ToBoolean(this.GetType().GetProperty(propertyName).GetValue(this, null));
        }
    }
}