using eNetwork.Inv;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Jobs.Fishing.Classes
{
    public class FishSpot
    {
        /// <summary>
        /// Название места для рыбалки
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Радиус зоны места для рыбалки
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Список рыб, которые можно выловить в этом месте
        /// </summary>
        public List<ItemId> Fish { get; set; }

        /// <summary>
        /// Позиция места для рыбалки
        /// </summary>
        public Vector3 Position { get; set; }

        public FishSpot(string name, float radius, Vector3 position, List<ItemId> fish)
        {
            Name = name;
            Radius = radius;
            Position = position;
            Fish = fish;
        }

        public void GTAElements()
        {
            NAPI.Blip.CreateBlip(68, Position, 1f, 4, Name, 255, 0, true, 0, 0);
            
            var colShape = NAPI.ColShape.CreateSphereColShape(Position, Radius, 0);
            colShape.OnEntityEnterColShape += (s, e) => e.SetData("fish.spot", this);
            colShape.OnEntityExitColShape += (s, e) => e.ResetData("fish.spot");
        }
    }
}
