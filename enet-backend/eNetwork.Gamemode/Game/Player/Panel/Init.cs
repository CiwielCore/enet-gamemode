using eNetwork.Game.Player.Panel.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Game.Player.Panel
{
    public class Init
    {
        public static void FirstLoad(ENetPlayer player)
        {
            var data = player.CharacterData;
            var lst = new List<PanelParam>();
            lst.Add(new PanelParam("login", player.AccountData.Login));
            lst.Add(new PanelParam("social", player.AccountData.SocialClub));
            lst.Add(new PanelParam("Name", data.Name));
            lst.Add(new PanelParam("lvl", data.Lvl));
            lst.Add(new PanelParam("exp", data.Exp));
            lst.Add(new PanelParam("coins", player.AccountData.DonatePoints));
            lst.Add(new PanelParam("fraction", new { name = data.Faction?.FactionName, rank = data.FactionRank }));
            lst.Add(new PanelParam("work", data.JobId.ToString()));
            var house = Houses.HousesManager.GetPlayerHouse(data.UUID);
            if (house != null)
            {
                lst.Add(new PanelParam("house", new { name = $"Дом №{house.Id}", garage = house.Garage.Places.Count, street = house.Street }));
            }
            var business = Businesses.BusinessManager.GetPlayerBusiness(data.UUID); 
            if (business != null)
            {
                lst.Add(new PanelParam("biz", new { name = $"{business.GetName()}", street = business.Street }));
            }

            var vehicles = Vehicles.VehicleManager.GetPlayerVehicles(data.UUID);

            lst.Add(new PanelParam("cars", vehicles.Select(el =>
            new
            {
                name = el.StockModel?.RealName,
                type = el.StockModel?.EngineType.ToString().ToLower(),
                number = el.NumberPlate,
                model = el.Model,
                spec = new List<object>(),
                eng = new List<object>(),
            })));

            Interface.SendData(player, "panelMenu/updateData", JsonConvert.SerializeObject(lst));
        }
        public static void UpdatePart(ENetPlayer player, string paramName, object data)
        {
            Interface.SendData(player, "panelMenu/updateData", JsonConvert.SerializeObject(new PanelParam(paramName, data)));
        }
        public static void UpdatePart(ENetPlayer player, Dictionary<string, object> pairs)
        {
            Interface.SendData(player, "panelMenu/updateData", JsonConvert.SerializeObject(pairs.Select(el => new PanelParam(el.Key, el.Value))));
        }
    }
}
