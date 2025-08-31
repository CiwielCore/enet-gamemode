using eNetwork.Framework.Configs.Tattoo.Classes;
using eNetwork.Framework.Configs.Tattoo.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.Configs.Tattoo
{
    public class TattooConfig 
    {
        private static readonly Logger Logger = new Logger("tattoo-config");
        public static ConcurrentDictionary<TattooZone, List<TattooData>> Tattoos = new ConcurrentDictionary<TattooZone, List<TattooData>>();

        public static void Initialize()
        {
            try
            {
                DataTable data = ENet.Config.ExecuteRead("SELECT * FROM `tattoo_list`");

                if (data != null && data.Rows.Count != 0)
                {
                    foreach(DataRow row in data.Rows)
                    {
                        TattooZone tattoZone = (TattooZone)Enum.Parse(typeof(TattooZone), row["zone"].ToString());
                        string name = Convert.ToString(row["name"]);
                        string collection = Convert.ToString(row["collection"]);
                        string overlayMale = Convert.ToString(row["overlay_male"]);
                        string overlayFemale = Convert.ToString(row["overlay_female"]);
                        int price = Convert.ToInt32(row["price"]);
                        bool isDonate = Convert.ToBoolean(row["is_donate"]);

                        if (!Tattoos.ContainsKey(tattoZone))
                            Tattoos.TryAdd(tattoZone, new List<TattooData>());

                        Tattoos[tattoZone].Add(new TattooData(name, collection, overlayMale, overlayFemale, price, isDonate));
                    }
                }

                Logger.WriteInfo($"Загруженно {Tattoos.Values.SelectMany(x => x).Distinct().Count()} татуировок");
            }
            catch(Exception ex) { Logger.WriteError("Initialize", ex); }
        }
    }
}
