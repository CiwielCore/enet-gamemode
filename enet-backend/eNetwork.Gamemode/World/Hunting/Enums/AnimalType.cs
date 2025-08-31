using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.World.Hunting.Enums
{
    public class AnimalsCollection
    {
        private static IReadOnlyDictionary<AnimalType, Animal> Animals = new Dictionary<AnimalType, Animal>()
        {
            { AnimalType.Rabbit, new Animal("Дикий кролик", "a_c_rabbit_01") },
            { AnimalType.Deer, new Animal("Олень", "a_c_deer") },
            { AnimalType.Boar, new Animal("Кабан", "a_c_boar", maxHealth: 500) },
            { AnimalType.Coyote, new Animal("Кайот", "a_c_coyote", false) },
            { AnimalType.Milton, new Animal("Милтон", "a_c_mtlion") },
            { AnimalType.Panther, new Animal("Пантера", "a_c_panther") },
            { AnimalType.Bear, new Animal("Медведь", "BrnBear") },
        };

        public static Animal GetAnimal(AnimalType type)
        {
            Animals.TryGetValue(type, out Animal data);
            return data;
        }

        public static AnimalType GetRandomType()
        {
            Array arr = Enum.GetValues(typeof(AnimalType));
            return (AnimalType)arr.GetValue(ENet.Random.Next(0, arr.Length));
        }

        public class Animal
        {
            public string Name;
            public string Model;
            public bool IsAgressive;
            public int MaxHealth;
            public Animal(string name, string model, bool agressive = true, int maxHealth = 200)
            {
                Name = name;
                Model = model;
                IsAgressive = agressive;
                MaxHealth = maxHealth;
            }
        }
    }
    public enum AnimalType
    {
        Rabbit, Deer, Boar, Coyote, Milton, Panther, Bear
    }
}
