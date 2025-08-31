using System;

namespace eNetwork.Services.VipServices
{
    public class VipStatus
    {
        public int CharacterId { get; set; }
        public string VipName { get; set; }
        public DateTime DateOfIssue { get; set; }
        public DateTime DateOfEnd { get; set; }
    }
}
