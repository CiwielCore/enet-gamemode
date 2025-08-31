using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Jobs.Builder.Classes
{
    internal class ClothesBuilder
    {
        public int ComponentID { get; set; }
        public int Drawable { get; set; }
        public int Texture { get; set; }

        public ClothesBuilder (int componentID, int drawable, int texture)
        {
            ComponentID = componentID;
            Drawable = drawable;
            Texture = texture;
        }

    }
}
