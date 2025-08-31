using eNetwork.Framework;
using eNetwork.Framework.API.ColShape;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Framework.Enums;
using eNetwork.GameUI;
using eNetwork.Jobs.Builder.Classes;
using eNetwork.Jobs.Builder.Data;
using GTANetworkAPI;
using GTANetworkMethods;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using ClientEvent = eNetwork.Framework.ClientEvent;

namespace eNetwork.Jobs.Builder
{
    internal class BuilderManager
    {
        private static readonly Logger _logger = new Logger("builder-manager");
        private static GTANetworkAPI.Ped _ped;
        private static ENetColShape _pedColShape;

        private static GTANetworkAPI.Marker _currentMarker;
        private static ENetColShape _currentColShape;
        private static Position _currentPoint;
        private static GTANetworkAPI.Blip _currentBlip;

        /// <summary>
        ///     Инициализация работы, создание педов и объектов.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                InitPed();
                InitObjects();
            }
            catch (Exception ex)
            {
                _logger.WriteError("Initialize", ex);
            }
        }

        /// <summary>
        /// Создание педов
        /// </summary>
        private static void InitPed()
        {
            _ped = NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("s_m_m_lathandy_01"), Config.PedPosition.GetVector3(), 175.0f, dynamic: false, invincible: true, frozen: true);
            NAPI.Blip.CreateBlip(478, Config.PedPosition.GetVector3(), 1, 26, "Стройка", 255, 0, true, 0, 0);
            _ped.SetData<Position>("POSITION_DATA", Config.PedPosition);

            _pedColShape = ENet.ColShape.CreateCylinderColShape(Config.PedPosition.GetVector3(), 2, 2, 0, ColShapeType.Builder);
            _pedColShape.SetInteractionText("Устроиться грузчиком");
        }

        /// <summary>
        /// Создание объектов работы
        /// </summary>
        private static void InitObjects()
        {
            foreach (var item in Config.PointsArray)
            {
                NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_boxpile_02b"), item.GetVector3(), new Vector3(0.0, 0.0, 0.0), 255, 0);
            }
        }

        /// <summary>
        /// Создание интерактива для педа
        /// </summary>
        /// <param name="player">Объект игрового персонажа</param>
        [InteractionDeprecated(ColShapeType.Builder, InteractionType.Key)]
        public static void InteractionPed(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                Dialog dialog = new Dialog()
                {
                    Name = "Сергей Николаевич",
                    Description = "Прораб на стройке",
                };

                List<DialogAnswer> answers = new List<DialogAnswer>();
                if (characterData.JobId != JobId.Builder)
                {
                    dialog.Text = "Привет, хочешь поработать у меня?";
                    answers.Add(new DialogAnswer("Да, давай", (p, o) => { OnEmploy(player); InteractionPed(player); }, "hire"));
                }
                else
                {
                    if (sessionData.WorkData.IsWorking)
                    {
                        dialog.Text = "Как поработал?";
                        answers.Add(new DialogAnswer("Остановить работу", (p, o) => { OnFiring(player); InteractionPed(player); }, "stop"));
                    }
                    else
                    {
                        dialog.Text = "Ну что поработаем сейчас?";
                        answers.Add(new DialogAnswer("Начать работу", (p, o) => { OnEmploy(player); Dialogs.Close(player); }, "start"));
                    }

                    answers.Add(new DialogAnswer("Уволится", (p, o) => { OnFiring(player); InteractionPed(player); }, "fire"));
                }

                answers.Add(new DialogAnswer("В другой раз...", (p, o) => { }, "close"));
                dialog.Answers = answers;

                dialog.Open(player, _ped);
            }
            catch (Exception ex) { _logger.WriteError("InteractionPed", ex); }
        }

        /// <summary>
        /// Метод трудоустройства на работу
        /// </summary>
        /// <param name="player">Объект игрового персонажа</param>
        private static void OnEmploy(ENetPlayer player)
        {
            if (player.CharacterData.JobId != JobId.None)
            {
                player.SendError("Вы должны уволиться с предыдущей работы!");
                return;
            }

            Dialogs.Close(player);
            player.CharacterData.JobId = JobId.Builder;
            player.SendDone("Вы устроились на работу");

            SetJobClothes(player);
            CreateOrder(player);
        }

        /// <summary>
        /// Метод увольнения с работы
        /// </summary>
        /// <param name="player">Объект игрового персонажа</param>
        private static void OnFiring(ENetPlayer player)
        {
            if (player.CharacterData.JobId != JobId.Builder)
            {
                player.SendError("Вы тут не работаете");
                return;
            }

            if (_currentBlip != null)
            {
                NAPI.Entity.DeleteEntity(_currentBlip);
            }

            Dialogs.Close(player);
            player.CharacterData.JobId = JobId.None;

            BackPlayerClothes(player);
        }

        /// <summary>
        /// Создание работы для игрока
        /// </summary>
        /// <param name="player">Объект игрового персонажа</param>
        private static void CreateOrder(ENetPlayer player)
        {
            Random random = new();
            int randomNumber = random.Next(0, Config.PointsArray.Count);
            _currentPoint = Config.PointsArray[randomNumber];

            _currentMarker = NAPI.Marker.CreateMarker(1, _currentPoint.GetVector3(), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), 3, new Color(255, 0, 0));
            _currentBlip = NAPI.Blip.CreateBlip(1, _currentPoint.GetVector3(), 1, 1, "Взять мешок", 255, 0, true, 0, 0);
            _currentColShape = ENet.ColShape.CreateCylinderColShape(_currentPoint.GetVector3(), 2.0f, 2.0f, 0, ColShapeType.BuilderTakeBag);
        }

        /// <summary>
        /// Метод в случае взятие коробки во время выполнения работы
        /// </summary>
        /// <param name="player">Объект игрового персонажа</param>
        [InteractionDeprecated(ColShapeType.BuilderTakeBag, InteractionType.Enter)]
        public static void InteractionTakeBox(ENetPlayer player)
        {
            NAPI.Entity.DeleteEntity(_currentBlip);
            NAPI.Entity.DeleteEntity(_currentColShape);
            NAPI.Entity.DeleteEntity(_currentMarker);

            player.AddAttachment("woolder_box");
            player.PlayAnimation("anim@heists@box_carry@", "idle", 49);

            _currentMarker = NAPI.Marker.CreateMarker(1, Config.DeliveryPoint.GetVector3(), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), 3, new Color(255, 0, 0));
            _currentColShape = ENet.ColShape.CreateCylinderColShape(Config.DeliveryPoint.GetVector3(), 2, 2, 0, ColShapeType.BuildDeliveryBag);
            _currentBlip = NAPI.Blip.CreateBlip(1, Config.DeliveryPoint.GetVector3(), 1, 1, "Сдать мешок", 255, 0, true, 0, 0);
        }

        /// <summary>
        /// Метод при возвращении коробки на финальную точку, выполнение работы
        /// </summary>
        /// <param name="player">Объект игрового персонажа</param>
        [InteractionDeprecated(ColShapeType.BuildDeliveryBag, InteractionType.Enter)]
        public static void InteractionDelivaryBox(ENetPlayer player)
        {
            player.ClearAttachments();
            player.StopAnimation();
            player.ChangeWallet(Config.Sallary);

            NAPI.Entity.DeleteEntity(_currentBlip);
            NAPI.Entity.DeleteEntity(_currentColShape);
            NAPI.Entity.DeleteEntity(_currentMarker);

            CreateOrder(player);
        }

        /// <summary>
        /// Возвращение стандартной одежды игрока до работы
        /// </summary>
        /// <param name="player">Объект игрового персонажа</param>
        private static void BackPlayerClothes(ENetPlayer player)
        {
            player.SetClothes(11, player.GetData<int>("TopsDrawable"), player.GetData<int>("TopsTexture"));
            player.SetClothes(8, player.GetData<int>("UndershirtsDrawable"), player.GetData<int>("UndershirtsTexture"));
            player.SetClothes(6, player.GetData<int>("ShoesDrawable"), player.GetData<int>("ShoesTexture"));
            player.SetClothes(4, player.GetData<int>("LegsDrawable"), player.GetData<int>("LegsTexture"));
            player.SetClothes(3, player.GetData<int>("TorsosDrawable"), player.GetData<int>("TorsosTexture"));
        }

        /// <summary>
        /// Выдача новый одежды на время работы
        /// </summary>
        /// <param name="player">Объект игрового персонажа</param>
        private static void SetJobClothes(ENetPlayer player)
        {
            SavePlayerClothes(player);
            if (player.Gender == Gender.Male)
            {
                foreach (var item in Config.MaleClothes)
                {
                    player.SetClothes(item.ComponentID, item.Drawable, item.Texture);
                }
            }
            else
            {
                foreach (var item in Config.WomenClothes)
                {
                    player.SetClothes(item.ComponentID, item.Drawable, item.Texture);
                }
            }

            static void SavePlayerClothes(ENetPlayer player)
            {
                player.SetData<int>("TopsDrawable", player.GetClothesDrawable(11));
                player.SetData<int>("TopsTexture", player.GetAccessoryTexture(11));
                player.SetData<int>("UndershirtsDrawable", player.GetClothesDrawable(8));
                player.SetData<int>("UndershirtsTexture", player.GetAccessoryTexture(8));
                player.SetData<int>("ShoesDrawable", player.GetClothesDrawable(6));
                player.SetData<int>("ShoesTexture", player.GetAccessoryTexture(6));
                player.SetData<int>("LegsDrawable", player.GetClothesDrawable(4));
                player.SetData<int>("LegsTexture", player.GetAccessoryTexture(4));
                player.SetData<int>("TorsosDrawable", player.GetClothesDrawable(3));
                player.SetData<int>("TorsosTexture", player.GetAccessoryTexture(3));
            }
        }
    }
}