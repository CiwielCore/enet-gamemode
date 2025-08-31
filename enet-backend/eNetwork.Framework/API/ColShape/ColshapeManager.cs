using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace eNetwork.Framework.API.ColShape
{
    public class ColshapeManager
    {
        public ColshapeManager() { }

        public ENetColShape CreateCylinderColShape(Vector3 position, float range, float height, uint dimension = uint.MaxValue, ColShapeType type = ColShapeType.Default)
        {
            var shape = (ENetColShape)NAPI.ColShape.CreateCylinderColShape(position, range, height, dimension);
            shape.SetIntraction(type);
            
            return shape;
        }

        public ENetColShape Create2DColShape(float x, float y, float width, float height, uint dimension = uint.MaxValue, ColShapeType type = ColShapeType.Default)
        {
            var shape = (ENetColShape)NAPI.ColShape.Create2DColShape(x, y, width, height, dimension);
            shape.SetIntraction(type);

            return shape;
        }

        public ENetColShape Create3DColShape(Vector3 start, Vector3 end, uint dimension = uint.MaxValue, ColShapeType type = ColShapeType.Default)
        {
            var shape = (ENetColShape)NAPI.ColShape.Create3DColShape(start, end, dimension);
            shape.SetIntraction(type);

            return shape;
        }    

        public ENetColShape CreatCircleColShape(float x, float y, float range, uint dimension = uint.MaxValue, ColShapeType type = ColShapeType.Default)
        {
            var shape = (ENetColShape)NAPI.ColShape.CreatCircleColShape(x, y, range, dimension);
            shape.SetIntraction(type);

            return shape;
        }
    }
}
