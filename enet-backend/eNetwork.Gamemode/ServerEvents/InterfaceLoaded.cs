using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.Businesses;
using eNetwork.Businesses.List;
using eNetwork.Businesses.Products;
using eNetwork.Framework;

namespace eNetwork.ServerEvents
{
    class InterfaceLoaded
    {
        private static readonly Logger Logger = new Logger("interface-loaded");

        [CustomEvent("InterfaceLoaded")]
        private static void OnEvent(ENetPlayer player)
        {
            try
            {
                if (!player.IsTimeouted("ui.loaded", 1) || player.HasData("player.ui.loaded")) return;

                player.SetData("player.ui.loaded", true);

                ENet.SceneManager.Load(player);
                ENet.Sounds.LoadSounds3dForPlayer(player);

                Game.LunaPark.FerrisWheel.LoadFerrisWheel(player);

                World.WeatherHandler.LoadWeather(player);
                Configs.BusinessConfig.Load(player);
                Configs.VehicleNames.Load(player);
                Configs.Weapons.LoadConfig(player);
                Businesses.BarberShop.LoadConfig(player);
                Businesses.Ammunation.LoadConfig(player);
                ProductShop.LoadConfig(player);
                ClothesShop.LoadConfig(player);

                ENet.Offroad.Initialize(player);
            }
            catch(Exception ex) { Logger.WriteError("OnEvent", ex); }
        } 
    }
}
