using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Jobs.Trucker
{
    public class Store
    {
        public Store()
        {
            new Vehicle("test1", "test2", "nero2", 0, 0);
        }
    }

    class Vehicle
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ModelName { get; set; }
        public int needLvl { get; set; }
        public int price { get; set; }

        public Vehicle(string _Name, string _Description, string _ModelName, int _needLvl, int _price)
        {
            Name = _Name;
            Description = _Description;
            ModelName = _ModelName;
            needLvl = _needLvl;
            price = _price;

        }

    }
}
