using eNetwork.Framework;
using eNetwork.Framework.Enums;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Businesses
{
    public class Business
    {
        private static readonly Logger Logger = new Logger("business");

        protected string Name { get; set; } = "undefined";
        protected uint BlipType { get; set; }
        protected byte BlipColor { get; set; }
        protected float BlipSize { get; set; } = 0.9f;
        protected uint PedHash { get; set; }
        public float ShapeRadius { get; set; } = 2;
        public float ShapeHeight { get; set; } = 2;
        public int ID { get; set; }
        public BusinessType Type { get; set; }
        public int Owner { get; set; }
        public int Price { get; set; }
        public int ControlFraction { get; set; }
        public List<BizProduct> Products { get; set; }
        public List<Vector3> Positions { get; set; } = new List<Vector3>();
        public Position PositionNPC { get; set; }
        public Vector3 PositionInfo { get; set; }
        public double Tax { get; set; }
        public double Earning { get; set; }
        public int Markup { get; set; } = 100;
        public string Street { get; set; }

        public string InteractionText = "";
        public string InteractionPedText = "";

        [JsonIgnore]
        public ENetBlip Blip { get; set; }
        
        [JsonIgnore]
        public Ped Ped { get; set; }

        public List<ENetColShape> ColShapes = new List<ENetColShape>();
        public ENetColShape PedColShape;

        public ENetColShape InfoColShape;
        public Marker InfoMarker;

        public Business(int id, BusinessType type)
        {
            ID = id; Type = type;
        }

        public void GTAElements()
        {
            foreach(var position in Positions)
            {
                var shape = ENet.ColShape.CreateCylinderColShape(position, ShapeRadius, ShapeHeight, 0, ColShapeType.Business);
                shape.OnEntityEnterColShape += (s, e) =>
                {
                    e.SetData("BUSINESS_DATA", this);
                    e.SetData("BUSINESS_POSITION_INDEX", Positions.IndexOf(position));
                };
                shape.OnEntityExitColShape += (s, e) =>
                {
                    e.ResetData("BUSINESS_DATA");
                    e.ResetData("BUSINESS_POSITION_INDEX");
                };
                shape.SetInteractionText(InteractionText);

                ColShapes.Add(shape);
            }

            CreateInfo();
            CreatePed();
        }

        public void CreateInfo()
        {
            if (InfoColShape != null)
            {
                InfoColShape.Delete();
            }

            if (InfoMarker != null)
            {
                InfoMarker.Delete();
            }

            InfoColShape = ENet.ColShape.CreateCylinderColShape(PositionInfo, 2, 2, 0, ColShapeType.BusinessInfo);
            InfoColShape.OnEntityEnterColShape += (s, e) =>
            {
                e.SetData("BUSINESS_INFO", this);
            };
            InfoColShape.OnEntityExitColShape += (s, e) =>
            {
                e.ResetData("BUSINESS_INFO");
                ClientEvent.Event((ENetPlayer)e, "client.business.info.close");
            };
            InfoColShape.SetInteractionText("Узнать информацию");

            InfoMarker = NAPI.Marker.CreateMarker(29, PositionInfo, new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 120), false, 0);
        }

        public void CreatePed()
        {
            if (Ped != null)
                Ped.Delete();

            Ped = NAPI.Ped.CreatePed(PedHash, PositionNPC.GetVector3(), PositionNPC.GetHeading(), false, true, true, true, 0);
            Ped.SetData("POSITION_DATA", PositionNPC);

            if (PedColShape != null)
                PedColShape.Delete();

            PedColShape = ENet.ColShape.CreateCylinderColShape(PositionNPC.GetVector3(), 2, 2, 0, ColShapeType.BusinessPed);
            PedColShape.OnEntityEnterColShape += (s, e) =>
            {
                e.SetData("BUSINESS_PED_DATA", this);
            };
            PedColShape.OnEntityExitColShape += (s, e) =>
            {
                e.ResetData("BUSINESS_PED_DATA");
            };
            PedColShape.SetInteractionText(InteractionPedText);
        }

        public void CreateBlip(BlipInformation info)
        {
            Blip = ENet.Blip.CreateBlip(BlipType, Positions[0], BlipSize, BlipColor, Name, 255, 0, true, 0, 0);
            Blip.SetInformation(info);
        }

        public bool ChangePosNPC(ENetPlayer player)
        {
            try
            {
                PositionNPC = new Position(player.Position.X, player.Position.Y, player.Position.Z, player.Heading);
                PositionNPC.Set(Ped);
                return true;
            }
            catch (Exception ex) { Logger.WriteError("ChangePosNPC", ex); return false; }
        }

        public bool TakeProduct(int value, string prodName, int addCash)
        {
            try
            {
                if (Products == null || Products.Count == 0) return false;
                foreach (BizProduct prod in Products)
                {
                    if (prod.Name == prodName)
                    {
                        if (prod.Count - value < 0) return false;

                        if (Owner == -1) break;
                        prod.Count -= value;
                        prod.CountSell += value;
                        Earning += addCash;
                    }
                }
                return true;
            }
            catch (Exception ex) { Logger.WriteError("TakeProduct", ex); return false; }
        }

        public async Task Save()
        {
            try
            {
                await ENet.Database.ExecuteAsync($"UPDATE `{BusinessManager.DBName}` SET `owner`={Owner}, `products`='{JsonConvert.SerializeObject(Products)}', `tax`={Tax}, `earning`={Earning}, `earning`={Earning}, `markup`={Markup} WHERE `id`={ID}");
            }
            catch(Exception ex) { Logger.WriteError("Save", ex); }
        }

        public void BuyProduct(ENetPlayer player, string prodName, int count)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                var product = Products.Find(x => x.Name == prodName);
                if (product is null)
                {
                    player.SendError($"Произошла ошибка при попытке закупить продукт - {prodName}");
                    return;
                }

                if (product.Count + count > product.MaxCount)
                {
                    player.SendError($"Невозможно столько закупить товара. Максимум - {product.MaxCount}");
                    return;
                }

                double totalPrice = product.Price * count;
                if (!player.ChangeWallet(-totalPrice)) return;

                product.Count += count;
            }
            catch (Exception ex) { Logger.WriteError("BuyProduct", ex); }
        }

        public string GetName()
        {
            return Name;
        }

        public int GetPrice(int price)
        {
            return Convert.ToInt32(Math.Ceiling(price * (Markup / 100.0)));
        }

        public virtual void Initialize() { }
        public virtual void InteractionBiz(ENetPlayer player) { }
        public virtual void InteractionNpc(ENetPlayer player) { }
    }
}
