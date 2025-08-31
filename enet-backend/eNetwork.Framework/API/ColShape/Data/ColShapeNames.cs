using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.ColShape.Data
{
    public class ColShapeNames
    {
        private static Dictionary<ColShapeType, string> Data = new Dictionary<ColShapeType, string>() {
            { ColShapeType.GarbageEmnpoyment, "Мусорщик" }
        };

        public static string GetName(ColShapeType type)
        {
            Data.TryGetValue(type, out string name);
            return name;
        }
    }
}
