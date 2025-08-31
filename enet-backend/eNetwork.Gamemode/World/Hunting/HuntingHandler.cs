using GTANetworkAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using eNetwork.Framework;
using Newtonsoft.Json;
using eNetwork.World.Hunting.Enums;
using eNetwork.Framework.API.Interaction;

namespace eNetwork.World.Hunting
{
    public class HuntingHandler : Script
    {
        private static Logger Logger = new Logger("hunting");
        private static readonly int _countSpawnAnimals = 25;
        public static List<Vector3> SpawnPoints = new List<Vector3>();
        public static List<HuntingAnimal> SpawnedAnimals = new List<HuntingAnimal>();

        public static void Initialize()
        {
            try
            {
                var data = ENet.Config.ExecuteRead("SELECT * FROM `hunting_points`");
                if (data is null || data.Rows.Count == 0) return;
                foreach (DataRow row in data.Rows)
                {
                    Vector3 point = JsonConvert.DeserializeObject<Vector3>(row["vector"].ToString());

                    SpawnPoints.Add(point);
                }

                CreateAnimals();

                Logger.WriteInfo("Создано " + SpawnedAnimals.Count + " животных");
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static void CreateAnimals()
        {
            try
            {
                var positions = GetPositions();
                foreach (Vector3 vector in positions)
                {
                    new HuntingAnimal(SpawnPoints.IndexOf(vector) + 1, vector, AnimalsCollection.GetRandomType());
                }
            }
            catch (Exception ex) { Logger.WriteError("CreateAnimals", ex); }
        }

        public static List<Vector3> GetPositions()
        {
            try
            {
                if (SpawnPoints.Count < _countSpawnAnimals) return null;

                var list = new List<Vector3>();

                var item = SpawnPoints[ENet.Random.Next(0, SpawnPoints.Count)];
                while (list.Count < _countSpawnAnimals)
                {
                    item = SpawnPoints[ENet.Random.Next(0, SpawnPoints.Count)];
                    if (!list.Contains(item))
                        list.Add(item);
                }

                return list;
            }
            catch (Exception ex) { Logger.WriteError("GetPositions", ex); return null; }
        }

        [ChatCommand("huntpoint")]
        private void CreateSpawnAnimal(ENetPlayer player)
        {
            try
            {
                Vector3 vector = player.Position - new Vector3(0, 0, 1.12);

                SpawnPoints.Add(vector);
                NAPI.Blip.CreateBlip(141, vector, 0.7f, 4, "Точка охоты", 255, 0, true, 0, 0);
                ENet.Database.Execute($"INSERT INTO `hunting_points` (`vector`) VALUES('{JsonConvert.SerializeObject(vector)}')");
            }
            catch (Exception e) { Logger.WriteError("CreateSpawnAnimal", e); }
        }

        [CustomEvent("server.animals.controller.require")]
        public void RequireController(ENetPlayer player, int pedId)
        {
            try
            {
                if (pedId < 0) return;

                var animal = SpawnedAnimals.Find(x => x.ID == pedId);
                if (animal is null) return;

                animal.Handle.Controller = player;
            }
            catch (Exception ex) { Logger.WriteError("RequireController", ex); }
        }

        [CustomEvent("server.animals.controller.remove")]
        public void RemoveController(ENetPlayer player, int pedId)
        {
            try
            {
                if (pedId < 0 || pedId > SpawnedAnimals.Count - 1) return;

                if (SpawnedAnimals[pedId - 1].Handle != null)
                    SpawnedAnimals[pedId - 1].Handle.Controller = null;
            }
            catch (Exception ex) { Logger.WriteError("RemoveController", ex); }
        }

        [CustomEvent("server.animals.dead")]
        public void Death(ENetPlayer player, int pedId, double x, double y, double z)
        {
            try
            {
                if (pedId < 0 || pedId > SpawnedAnimals.Count - 1) 
                    return;

                SpawnedAnimals[pedId - 1].Death(new Vector3(x, y, z));
            }
            catch (Exception ex) { Logger.WriteError("Death", ex); }
        }

        public static void CheckAnimals()
        {
            foreach (HuntingAnimal anm in SpawnedAnimals)
            {
                if (!anm.IsAlive)
                {
                    anm.Respawn();
                }
            }
        }

        public class HuntingAnimal
        {
            public Ped Handle;
            public AnimalsCollection.Animal AnimalData;
            public int ID;
            public Vector3 Spawn;
            public AnimalType Type;
            public Interaction Interaction;

            public bool IsAlive = true;
            public bool IsDead = false;

            public HuntingAnimal(int id, Vector3 spawn, AnimalType type)
            {

                ID = id;
                Type = type;
                AnimalData = AnimalsCollection.GetAnimal(type);

                Spawn = spawn;
                Respawn();

                SpawnedAnimals.Add(this);
            }

            public void Harvest()
            {
                try
                {
                    ClientEvent.EventToPlayers(NAPI.Pools.GetAllPlayers().ToArray(), "client.animals.destroy", ID);

                    IsAlive = false;

                    NAPI.Task.Run(Interaction.Destroy);

                    NAPI.Task.Run(() =>
                    {
                        Handle.Delete();
                        Handle = null;
                    }, 5000);
                }
                catch (Exception ex) { Logger.WriteError("Harvest", ex); }
            }

            public void Respawn()
            {
                try
                {
                    if (Handle != null) 
                        return;

                    NAPI.Task.Run(() =>
                    {
                        float heading = ENet.Random.Next(0, 359);
                        Handle = NAPI.Ped.CreatePed(NAPI.Util.GetHashKey(AnimalData.Model), Spawn, heading, true, false, false);
                        Handle.Controller = null;
                        IsDead = false;

                        Dictionary<string, object> dict = new Dictionary<string, object>();

                        dict.Add("staticDead", false);
                        dict.Add("controllerScript", "ANIMAL_HUNTING");
                        dict.Add("autoControl", true);
                        dict.Add("animal.data", JsonConvert.SerializeObject(AnimalData));
                        dict.Add("ped.type", "animal");

                        dict.Add("ped.id", ID);

                        Handle.SetSharedData(dict);
                    });
                }
                catch (Exception ex) { Logger.WriteError("Respawn", ex); }
            }

            public void Death(Vector3 pos)
            {
                try
                {
                    if (IsDead) 
                        return;

                    IsDead = true;
                    Handle.SetSharedData("staticDead", true);

                    Interaction = new Interaction(pos, (player, shape) =>
                    {
                        int animalId = Interaction.GetStay<int>(player, "A_ID");
                        SpawnedAnimals[animalId - 1].Harvest();
                    }, radius: 1, height: 2);
                    Interaction.SetStay("A_ID", ID);
                }
                catch (Exception ex) { Logger.WriteError("Death", ex); }
            }

        }
    }
}
