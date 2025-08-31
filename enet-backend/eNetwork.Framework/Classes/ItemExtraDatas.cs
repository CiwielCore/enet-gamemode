using eNetwork.Framework;
using eNetwork.Inv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork
{
    public class InvClothesData
    {
        public bool Gender;
        public int Component;
        public int Texture;
        public InvClothesData(Gender gender, int component, int texture = -1) 
        { 
            Gender = gender == eNetwork.Gender.Male ? true : false;
            Component = component;
            if(texture != -1) Texture = texture;
        }
    }

    public class InvBackpackData 
    {
        public bool Gender;
        public int Component;
        public int Texture;
        public List<Item> Inventory = new List<Item>();
        public int Slots;

        public InvBackpackData(Gender gender, int component, int texture, int slots)
        {
            Gender = gender == eNetwork.Gender.Male ? true : false;
            Component = component;
            Texture = texture;
            Slots = slots;
        }
    }
}
