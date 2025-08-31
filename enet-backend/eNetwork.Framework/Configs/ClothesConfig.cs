using eNetwork.Clothes;
using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.Configs
{
    public static class ClothesConfig
    {
        private static Dictionary<int, ClotheModel> Pool = new Dictionary<int, ClotheModel>();
        public static void Initialize()
        {
            var table = ENet.Database.ExecuteRead("SELECT * FROM `clothes`");
            foreach (DataRow row in table.Rows)
            {
                var clotheModel = new ClotheModel(
                    id: Convert.ToInt32(row["id"]),
                    name: Convert.ToString(row["name"]),
                    gender: Convert.ToInt32(row["gender"]),
                    isProp: Convert.ToInt32(row["isProp"]),
                    componentId: Convert.ToInt32(row["cmpId"]),
                    drawableId: Convert.ToInt32(row["drwId"]),
                    textureId: Convert.ToInt32(row["txtId"]),
                    undershirtId: Convert.ToInt32(row["underId"]),
                    torseId: Convert.ToInt32(row["torseId"]),
                    price: Convert.ToInt32(row["price"]));
                Pool.Add(clotheModel.Id, clotheModel);
            }
        }
        public static ClotheModel Get(int id)
        {
            if(Pool.ContainsKey(id)) return Pool[id];
            return null;
        }
    }
}
