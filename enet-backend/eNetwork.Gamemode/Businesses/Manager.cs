using eNetwork.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using GTANetworkAPI;
using System.Text;
using System.Data;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using eNetwork.Framework.Enums;
using eNetwork.Businesses.List;
using eNetwork.Framework.API.InteractionDepricated.Data;

namespace eNetwork.Businesses
{
    public class BusinessManager
    {
        private static readonly Logger Logger = new Logger("business-manager");
        private static int LastID = 0;
        private static ConcurrentDictionary<int, Business> List = new ConcurrentDictionary<int, Business>();
        public static readonly string DBName = "business";

        public static void Initialize()
        {
            try
            {
                DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM `{DBName}`");
                if (result != null)
                {
                    foreach (DataRow row in result.Rows)
                    {
                        BusinessType type = (BusinessType)Convert.ToInt32(row["type"]);
                        int id = Convert.ToInt32(row["id"]);
                        int owner = Convert.ToInt32(row["owner"]);
                        int price = Convert.ToInt32(row["price"]);
                        int controlfrac = Convert.ToInt32(row["fraction"]);
                        List<BizProduct> bizProducts = JsonConvert.DeserializeObject<List<BizProduct>>(row["products"].ToString());
                        List<Vector3> position = JsonConvert.DeserializeObject<List<Vector3>>(row["position"].ToString());
                        Position npc = JsonConvert.DeserializeObject<Position>(row["npc"].ToString());
                        Vector3 posInfo = JsonConvert.DeserializeObject<Vector3>(row["posinfo"].ToString());
                        int tax = Convert.ToInt32(row["tax"]);
                        int earning = Convert.ToInt32(row["earning"]);
                        int markUp = Convert.ToInt32(row["markUp"]);

                        var street = row["street"].ToString();

                        var business = Create(type, id, owner, price, controlfrac, bizProducts, position, npc, posInfo, tax, earning, markUp, street);
                        if (business is null)
                            Logger.WriteError($"Не удалось создать бизнес #{id} - {type.ToString()}");
                    }
                }

                Logger.WriteDone($"Загружено {List.Count} бизнесов");
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }
        public static Business Create(BusinessType type, int id, int owner, int price, int controlfrac, List<BizProduct> products, List<Vector3> pos, Position npc, Vector3 posInfo, int tax, int earning, int markUp, string street = "")
        {
            Business data;

            switch (type)
            {
                case BusinessType.PetrolStation:
                    if (products is null)
                        products = new List<BizProduct>() { 
                            new BizProduct("92 Топливо", 100, 0, 100000), 
                            new BizProduct("95 Топливо", 100, 0, 100000), 
                            new BizProduct("98 Топливо", 100, 0, 100000), 
                            new BizProduct("100 Топливо", 100, 0, 100000), 
                            new BizProduct("Дизельное топливо", 100, 0, 100000) 
                        };
                    data = new PetrolStation(id, type);
                    break;
                case BusinessType.BarberShop:
                    if (products is null)
                        products = new List<BizProduct>() {
                            new BizProduct("Расходники", 100, 0, 100000),
                        };
                    data = new BarberShop(id, type);
                    break;
                case BusinessType.TattooShop:
                    if (products is null)
                        products = new List<BizProduct>() {
                            new BizProduct("Краски", 100, 0, 100000),
                        };
                    data = new TattooShop(id, type);
                    break;
                case BusinessType.AmmuNation:
                    var ammunationData = Products.Ammunations.GetProducts(type);
                    var tempProds = products;
                    if (products is null)
                    {
                        products = new List<BizProduct>();
                        foreach (var product in ammunationData.ToList())
                        {
                            var prod = new BizProduct(product.Item, product.Price, 0, product.MaxCount);
                            products.Add(prod);
                        }
                    }
                    else
                    {
                        foreach (var product in ammunationData.ToList())
                        {
                            if (products.Find(x => x.Name == product.Item) is null)
                                tempProds.Add(new BizProduct(product.Item, product.Price, 0, product.MaxCount));
                        }

                        foreach (var product in products.ToList())
                        {
                            var prod = ammunationData.Find(x => x.Item.ToString() == product.Name);
                            if (prod is null)
                                tempProds.Remove(product);
                        }

                        products = tempProds;
                    }

                    data = new Ammunation(id, type);
                    break;
                case BusinessType.Showroom:
                    var showroomData = Products.Showrooms.GetProducts(type);
                    tempProds = products;
                    if (products != null)
                    {
                        foreach (var product in showroomData.ToList())
                        {
                            if (products.Find(x => x.Name == product.Model) is null)
                                tempProds.Add(new BizProduct(product.Model, product.Price, 0, product.MaxCount));
                        }

                        foreach (var product in products.ToList())
                        {
                            var prod = showroomData.Find(x => x.Model == product.Name);
                            if (prod is null)
                                tempProds.Remove(product);
                        }

                        products = tempProds;
                    }
                    else if (products is null)
                    {
                        products = new List<BizProduct>();
                        foreach(var product in showroomData.ToList())
                        {
                            var prod = new BizProduct(product.Model, product.Price, 0, product.MaxCount);
                            products.Add(prod);
                        }
                    }

                    data = new Showroom(id, type);
                    break;
                case BusinessType.Shop24:
                    var shop24Data = Products.Shops24.GetProducts(type);
                    tempProds = products;
                    if (products is null)
                    {
                        products = new List<BizProduct>();
                        foreach (var product in shop24Data.ToList())
                        {
                            var prod = new BizProduct(product.Name, product.Price, 0, product.MaxCount);
                            products.Add(prod);
                        }
                    }
                    else
                    {
                        foreach (var product in shop24Data.ToList())
                        {
                            if (products.Find(x => x.Name == product.Name) is null)
                                tempProds.Add(new BizProduct(product.Name, product.Price, 0, product.MaxCount));
                        }

                        foreach (var product in products.ToList())
                        {
                            var prod = shop24Data.Find(x => x.Name.ToString() == product.Name);
                            if (prod is null)
                                tempProds.Remove(product);
                        }

                        products = tempProds;
                    }
                    data = new ProductShop(id, type);
                    break;
                case BusinessType.ClothesShop:
                    var clothesData = Products.ClothesShops.GetProducts(type);
                    tempProds = products;
                    if (products is null)
                    {
                        products = new List<BizProduct>();
                        foreach (var product in clothesData)
                        {
                            var prod = new BizProduct(product.Name, product.Price, 0, product.MaxCount);
                            products.Add(prod);
                        }
                    }
                    else
                    {
                        foreach (var product in clothesData)
                        {
                            if (products.Find(x => x.Name == product.Name) is null)
                                tempProds.Add(new BizProduct(product.Name, product.Price, 0, product.MaxCount));
                        }

                        foreach (var product in products)
                        {
                            var prod = clothesData.Find(x => x.Name.ToString() == product.Name);
                            if (prod is null)
                                tempProds.Remove(product);
                        }

                        products = tempProds;
                    }
                    data = new ClothesShop(id, type);
                    break;
                default: return null;
            }

            data.Owner = owner;
            data.Products = products;
            data.ControlFraction = controlfrac;
            data.Tax = tax;
            data.Earning = earning;
            data.PositionNPC = npc;
            data.PositionInfo = posInfo;
            data.Positions = pos;
            data.Price = price;
            data.Markup = markUp;

            if (LastID < id)
                LastID = id;

            data.Street = street;

            data.Initialize();

            List.TryAdd(id, data);
            return data;
        }
        
        public static int GetIndexBussinesOfType(BusinessType type, int bizId)
        {
            return List.Values.ToList().Where(x => x.Type == type).ToList().FindIndex(x => x.ID == bizId);
        }

        public static Business GetBusiness(int id)
        {
            List.TryGetValue(id, out Business business);
            return business;
        }

        public static Business GetPlayerBusiness(ENetPlayer player)
        {
            return GetPlayerBusiness(player.GetUUID());
        }

        public static Business GetPlayerBusiness(int uuid)
        {
            var business = List.Values.ToList().Find(x => x.Owner == uuid);
            return business;
        }

        public static int GenID()
        {
            var id = LastID;
            while(List.ContainsKey(id)) 
                id++;

            return id;
        }

        [InteractionDeprecated(ColShapeType.Business, InteractionType.Key)]
        public static void Interaction(ENetPlayer player)
        {
            try
            {
                if (!player.GetData<Business>("BUSINESS_DATA", out Business business)) return;
                player.SetData("CURRENT_BUSINESS", business);
                business.InteractionBiz(player);
            }
            catch (Exception ex) { Logger.WriteError("Interaction", ex); }
        }

        [InteractionDeprecated(ColShapeType.BusinessPed, InteractionType.Key)]
        public static void InteractionPed(ENetPlayer player)
        {
            try
            {
                if (!player.GetData<Business>("BUSINESS_PED_DATA", out Business business)) return;
                player.SetData("CURRENT_PED_BUSINESS", business);
                business.InteractionNpc(player);
            }
            catch (Exception ex) { Logger.WriteError("InteractionPed", ex); }
        }

        public static async Task SavingBusinesses()
        {
            try
            {
                foreach (var biz in List.Values)
                    await biz.Save();
            }
            catch(Exception ex) { Logger.WriteError("SavingBusinesses", ex); }
        }
    }
}
