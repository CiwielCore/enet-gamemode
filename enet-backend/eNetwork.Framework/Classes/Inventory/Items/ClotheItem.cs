using eNetwork.Clothes;
using eNetwork.Configs;
using eNetwork.Framework.Configs;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Inv.Items
{
    public class ClotheItem : Item
    {
        private int clotheId { get; set; } = 0;
        public int ClotheId
        {
            get
            {
                return clotheId;
            }
            set
            {
                clotheId = value;
                this.data = JsonConvert.SerializeObject(new { ClotheId });
                this.ClotheModel = ClothesConfig.Get(clotheId);
                if (ClotheModel == null)
                {
                    //logger нужен
                }
            }
        }
        public ClotheModel ClotheModel { get; set; }
        public ClotheItem() { }
        public ClotheItem(Item item) : base(item)
        {
            UpdateParams();
        }
        public override void UpdateParams()
        {
            if (this.data.Length == 0)
            {
                this.data = NAPI.Util.ToJson(new { ClotheId });
                //какое то бы сохранение сделать
                if (this.Id != -1) _ = ENet.Database.ExecuteAsync($"UPDATE `inventory` SET `data` = '{data}' WHERE `id` = '{this.Id}'");
            }
            else
            {
                ClotheItem props = NAPI.Util.FromJson<ClotheItem>(this.data);
                this.ClotheId = props.ClotheId;
            }
        }
    }
}
