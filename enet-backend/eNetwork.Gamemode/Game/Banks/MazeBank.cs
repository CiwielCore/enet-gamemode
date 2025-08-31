using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace eNetwork.Game.Banks
{
    public class MazeBank
    {
        private readonly static Vector3 Position = new Vector3(-1380.1808, -489.6584, 32.023678);
        public static void Initialize()
        {
            var blip = ENet.Blip.CreateBlip(381, Position, 1f, 1, "Maze Bank", 255, 0, true, 0, 0);
            BlipInformation blipInformation = ENet.Blip.GenerateInformation(
                   name: "Оффис Maze Bank",
                   description: "Сеть банков во вселенной HD, штаб-квартира расположена в мегаполисе Лос-Сантос, Округ Лос-Сантос, Сан-Андреас, США. Одна из влиятельнейших компаний в Южной части штата Сан-Андреас.",
                   picture: "mazebank",
                   type: BlipInfoType.Work,
                   extra: ""
               );
            blip.SetInformation(blipInformation);
        }
    }
}
