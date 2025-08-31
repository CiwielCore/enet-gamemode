using eNetwork.Configs;
using eNetwork.Factions.Classes.CrimeFractions;
using eNetwork.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eNetwork.Services.VipServices;
using eNetwork.Services.VipServices.VipAddons;
using eNetwork.Inv;
using eNetwork.Inv;
using System.Data;
using eNetwork.Factions.Classes.StateFactions;
using GTANetworkAPI;
using eNetwork.Factions.Data;
using eNetwork.Factions.Interaction;
using eNetwork.Framework.Classes.Faction;
using eNetwork.Framework.Classes.Faction.Ranks;

namespace eNetwork.Factions
{
    public class FactionsManager
    {
        private static readonly List<Framework.Classes.Faction.Factions> _factionLoaded = new();
        private static readonly Logger _logger = new("faction-manager");

        /// <summary>
        /// Иницилизируем фракции, вытаскиваем из базы данных все данные фракции, создаем их.
        /// </summary>
        public static void Initialization()
        {
            try
            {
                DataTable resultFactions = ENet.Database.ExecuteRead($"SELECT * FROM factions");

                if (resultFactions is null)
                    return;

                foreach (DataRow factionRow in resultFactions.Rows)
                {
                    FactionType factionType = (FactionType)Enum.Parse(typeof(FactionType), Convert.ToString(factionRow["type"]));

                    if (factionType == FactionType.State)
                    {
                        StateFactions faction = new(Convert.ToString(factionRow["name"]),
                               (FactionType)Enum.Parse(typeof(FactionType), Convert.ToString(factionRow["type"])),
                               GetRanksData(Convert.ToInt32(factionRow["factionId"])),
                               GetFactionVehicle(Convert.ToInt32(factionRow["factionId"])),
                               JsonConvert.DeserializeObject<Vector3>(factionRow["position"].ToString()),
                               Convert.ToInt32(factionRow["money"]),
                               new List<Weapons.WeaponData>(),
                               new List<ENetPlayer>());
                        _factionLoaded.Add(faction); // Добавляем каждую фракцию в список загруженных фракций для дальнейших работ
                    }
                    else
                    {
                        CrimeFactions faction = new(Convert.ToString(factionRow["name"]),
                               (FactionType)Enum.Parse(typeof(FactionType), Convert.ToString(factionRow["type"])),
                               GetRanksData(Convert.ToInt32(factionRow["factionId"])),
                               GetFactionVehicle(Convert.ToInt32(factionRow["factionId"])),
                               JsonConvert.DeserializeObject<Vector3>(factionRow["position"].ToString()),
                               Convert.ToInt32(factionRow["money"]),
                               new List<Weapons.WeaponData>(),
                               new List<ENetPlayer>());
                        _factionLoaded.Add(faction); // Добавляем каждую фракцию в список загруженных фракций для дальнейших работ
                    }
                }

                LoadFactions(); // Подгружаем дополнительные предметы для фракций: блипы, нпс, маркеры.

                _logger.WriteInfo($"Загружено {_factionLoaded.Count} фракций");
            }
            catch (Exception ex) { _logger.WriteError("Initialization ", ex); }
        }

        /// <summary>
        /// После инициализации подгружаем блипы, НПС, колшейпы
        /// </summary>
        private static void LoadFactions()
        {
            List<FactionVehiclesData> createdVehicle = new List<FactionVehiclesData>();

            for (int i = 0; i < FactionsData.BlipData.Count; i++) // Создание блипов фракций
            {
                ENet.Blip.CreateBlip(FactionsData.BlipData[i].Sprite,
                                     FactionsData.FactionNPCData[i].Item1.GetVector3(),
                                     0.8f,
                                     FactionsData.BlipData[i].Color,
                                     _factionLoaded[i].FactionName, 255, 0.0f, true);
            }

            for (int i = 0; i < FactionsData.FactionStockPosition.Length; i++) // Создание колшейпов выгрузки материалов и склада
            {
                ENet.ColShape.CreateCylinderColShape(FactionsData.FactionStockPosition[i].UnLoadingStockPosition, 20, 20, 0, ColShapeType.FactionUnLoadingStock);
                ENet.ColShape.CreateCylinderColShape(FactionsData.FactionStockPosition[i].StockPosition, 20, 20, 0, ColShapeType.FactionStock);

                eNetwork.Framework.API.Interaction.Interaction colshape = new eNetwork.Framework.API.Interaction.Interaction(FactionsData.FactionStockPosition[i].UnLoadingStockPosition,
                                                                                                              PedInteraction.Interaction);

                NAPI.Marker.CreateMarker(1, FactionsData.FactionStockPosition[i].UnLoadingStockPosition,
                                         new Vector3(0.0f, 0.0f, 0.0f),
                                         new Vector3(0.0f, 0.0f, 0.0f),
                                         5, new Color(255, 0, 0));

                NAPI.TextLabel.CreateTextLabel("Выгрузка материалов",
                                               FactionsData.FactionStockPosition[i].UnLoadingStockPosition,
                                               5, 10, 2, new Color(255, 255, 255));
            }

            for (int i = 0; i < FactionsData.FactionNPCData.Length; i++)
            {
                // Создаем ПЕДа для фракций
                GTANetworkAPI.Ped factionPed = NAPI.Ped.CreatePed(NAPI.Util.GetHashKey(FactionsData.FactionNPCData[i].Item2),
                                                                  FactionsData.FactionNPCData[i].Item1.GetVector3(),
                                                                  FactionsData.FactionNPCData[i].Item1.GetHeading(),
                                                                  dynamic: false, invincible: true, frozen: true);
                factionPed.SetData<Position>("POSITION_DATA", FactionsData.FactionNPCData[i].Item1);

                eNetwork.Framework.API.Interaction.Interaction colshape = new eNetwork.Framework.API.Interaction.Interaction(FactionsData.FactionNPCData[i].Item1.GetVector3(),
                                                                                                                              PedInteraction.Interaction);
                colshape.SetStay("FACTION_ID", i);
                colshape.SetStay("PED_NAME", FactionsData.FactionNPCData[i].Item3);
                colshape.SetStay("PED", factionPed);
            }
        }

        /// <summary>
        /// Получает значение из базы данных о рангах фракции
        /// </summary>
        /// <param name="factionId">ИД фракции</param>
        /// <returns>Словарь всех рангов фракции</returns>
        private static List<RanksData> GetRanksData(int factionId)
        {
            List<RanksData> ranks = new List<RanksData>() { };
            DataTable resultRanks = ENet.Database.ExecuteRead($"SELECT * FROM factions_ranks WHERE factionId={factionId}");

            if (resultRanks is null)
                throw new ArgumentException("Запрос был составлен неверно");

            foreach (DataRow ranksRow in resultRanks.Rows)
            {
                ranks.Add(new(Convert.ToString(ranksRow["name"]), Convert.ToInt32(ranksRow["payDay"]), Convert.ToInt32(ranksRow["lvl"]), JsonConvert.DeserializeObject<RanksPermission>(ranksRow["permission"].ToString())));
            }

            return ranks;
        }

        private static List<ENetVehicle> GetFactionVehicle(int factionId)
        {
            List<ENetVehicle> factionVehicles = new List<ENetVehicle>() { };
            DataTable resultVehicles = ENet.Database.ExecuteRead($"SELECT * FROM vehicles WHERE JSON_EXTRACT(owner, '$.OwnerUUID') = {factionId}");

            if (resultVehicles is null)
                throw new ArgumentException("Запрос был составлен неверно");

            foreach (DataRow vehiclesRow in resultVehicles.Rows)
            {
                dynamic vehicleOwnerParse = JsonConvert.DeserializeObject(vehiclesRow["owner"].ToString());
                OwnerVehicleEnum vehicleOwnerConvert = vehicleOwnerParse["OwnerVehicleType"];
                VehicleOwner vehicleOwner = new VehicleOwner(vehicleOwnerConvert, (int)vehicleOwnerParse["OwnerUUID"]);

                FactionVehiclesData vehicleData = new FactionVehiclesData((int)vehiclesRow["id"],
                                                           vehicleOwner,
                                                           Convert.ToString(vehiclesRow["number"]),
                                                           Convert.ToString(vehiclesRow["model"]),
                                                           (int)vehiclesRow["health"],
                                                           (float)vehiclesRow["fuel"],
                                                           (float)vehiclesRow["mile"],
                                                           JsonConvert.DeserializeObject<VehicleCustomization>(vehiclesRow["components"].ToString()),
                                                           JsonConvert.DeserializeObject<List<Item>>(vehiclesRow["items"].ToString()),
                                                           JsonConvert.DeserializeObject<Position>(vehiclesRow["position"].ToString()),
                                                           (float)vehiclesRow["dirt"],
                                                           "",
                                                           (int)vehiclesRow["accessRank"]);

                ENetVehicle vehicle = ENet.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(vehicleData.Model), vehicleData.Position.GetVector3(), (float)vehicleData.Position.Heading, 0, 0, Convert.ToString(vehiclesRow["number"]), 255, true, false, 0);
                vehicle.SetVehicleData(vehicleData);
                vehicle.SetType(VehicleType.Fraction);
                vehicle.ApplyCustomization();

                vehicle.SetSharedData("model.name", vehicleData.Model);
                vehicle.SetSharedData("owner", vehicleData.Owner);
                factionVehicles.Add(vehicle);
            }

            return factionVehicles;
        }

        public static void AddMemberInFactionList(ENetPlayer player)
        {
            if (player.CharacterData.FactionId < 0)
            {
                return;
            }

            _factionLoaded[player.CharacterData.FactionId].MembersInFaction.Add(player);
            player.CharacterData.Faction = _factionLoaded[player.CharacterData.FactionId];
        }

        public static List<ENetPlayer> GetCurrentMembersListInFaction(int factionId)
        {
            if (factionId < 0)
                throw new ArgumentException("Переданное ИД фракции не соответствует действительности");

            return _factionLoaded[factionId].MembersInFaction;
        }

        /// <summary>
        /// Возвращает количество материалов у фракции
        /// </summary>
        /// <param name="factionId">ИД фракции</param>
        /// <returns>Количество материалов</returns>
        public int GetFactionMaterials(int factionId)
        {
            if (factionId <= 0 && factionId > _factionLoaded.Count)
                throw new ArgumentException("Переданное ИД фракции не соответствует действительности");

            return _factionLoaded[factionId].MaterialsCounts;
        }

        /// <summary>
        /// Возвращает позицию фракции для спавна, блипов и т.д.
        /// </summary>
        /// <param name="factionId">ИД фракции</param>
        /// <returns>Позиция фракции</returns>
        public GTANetworkAPI.Vector3 GetFactionPosition(int factionId)
        {
            if (factionId <= 0 && factionId > _factionLoaded.Count)
                throw new ArgumentException("Переданное ИД фракции не соответствует действительности");

            return _factionLoaded[factionId].Position;
        }

        /// <summary>
        /// Возвращает список игроков, которые находятся во фракции
        /// </summary>
        /// <param name="factionId">ИД фракции</param>
        /// <returns>Список игроков во фракции</returns>
        public List<ENetPlayer> GetFactionMembers(int factionId)
        {
            List<ENetPlayer> factionMembers = new List<ENetPlayer>() { };

            if (factionId <= 0 && factionId > _factionLoaded.Count)
                throw new ArgumentException("Переданное ИД фракции не соответствует действительности");

            foreach (ENetPlayer targetInPool in ENet.Pools.GetAllRegisteredPlayers())
            {
                if (((int)targetInPool.CharacterData.FactionId) == factionId)
                    factionMembers.Add(targetInPool);
            }

            return factionMembers;
        }

        public FactionType GetFactionTypeByID(int factionId)
        {
            if (factionId <= 0 && factionId > _factionLoaded.Count)
                throw new ArgumentException("Переданное ИД фракции не соответствует действительности");

            return _factionLoaded[factionId].FactionType;
        }

        public bool GetRanksPermission(int factionId, int ranksId, string permission)
        {
            if (factionId <= 0 && factionId > _factionLoaded.Count)
                throw new ArgumentException("Переданное ИД фракции не соответствует действительности");

            return _factionLoaded[factionId].Ranks[ranksId].Permission.GetPropertyValue(permission);
        }
    }
}