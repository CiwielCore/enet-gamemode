using eNetwork.Businesses;
using eNetwork.Framework;
using GTANetworkAPI;
using System;
using eNetwork.GameUI;
using System.Collections.Generic;
using eNetwork.Framework.Enums;
using eNetwork.Services.CarRental;
using System.Drawing;
using GTANetworkMethods;
using eNetwork.Framework.Classes.Sessions.Classes;
using eNetwork.Jobs.BusDriver.Classes;
using System.Numerics;
using eNetwork.Jobs.Trucker.Classes;
using eNetwork.Framework.API.InteractionDepricated.Data;

namespace eNetwork.Jobs.Trucker
{

    public class TruckerManager
    {
        private static readonly Logger Logger = new Logger("trucker-manager");
        private static GTANetworkAPI.Ped Ped;
        private static ENetColShape PedColShape;
        private static readonly List<TruckerCarData> _rentsData = new List<TruckerCarData>();


        public static void Initialize()
        {
            try
            {
                InitPed();
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static void InitPed()
        {
            if (Ped != null)
                Ped.Delete();

            Ped = NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("s_m_y_construct_02"), Config.NPCPos.GetVector3(), -133.9571f, false, true, true, true, 0);
            Ped.SetData("POSITION_DATA", Config.NPCPos);

            if (PedColShape != null)
                PedColShape.Delete();

            PedColShape = ENet.ColShape.CreateCylinderColShape(Config.NPCPos.GetVector3(), 2, 2, 0, ColShapeType.TruckerEmployer);
            PedColShape.SetInteractionText("Поговорить с бомжом");
        }

        [InteractionDeprecated(ColShapeType.TruckerEmployer, InteractionType.Key)]
        public static void InteractionPed(ENetPlayer player)
        {
            try
            {
                var characterData = player.CharacterData;

                var dialog = new Dialog()
                {
                    Name = "Борис Иванович",
                    Description = "Человек подающий надежды",
                };

                var answers = new List<DialogAnswer>();
                if (characterData.JobId != JobId.Trucker)
                {
                    dialog.Text = "Привет, хочешь поработать у меня?";
                    answers.Add(new DialogAnswer("Да, давай", (p, o) => { OnEmploy(player); InteractionPed(player); }, "employ"));
                }
                else
                {
                    dialog.Text = "Ну что поработаем сейчас?";
                    answers.Add(new DialogAnswer("Арендовать ТС", OnRentVeh, "rentcar"));

                    answers.Add(new DialogAnswer("Уволится", (p, o) => { OnFiring(player); InteractionPed(player); }, "firing"));
                }

                answers.Add(new DialogAnswer("В другой раз...", (p, o) => { }, "close"));
                dialog.Answers = answers;

                dialog.Open(player, Ped);
            }
            catch (Exception ex) { Logger.WriteError("InteractionPed", ex); }
        }

        public static void OnEmploy( ENetPlayer player, params object[] arguments)
        {
            if (player.CharacterData.JobId != JobId.None)
            {
                player.SendError("Вы должны уволиться с предыдущей работы!");
                return;
            }

            Dialogs.Close(player);
            player.CharacterData.JobId = JobId.Trucker;
            player.SendDone("Вы устроились на работу");

        }

        public static void OnFiring(ENetPlayer player, params object[] arguments)
        {
            if (player.CharacterData.JobId != JobId.Trucker)
            {
                player.SendError("Вы тут не работаете");
                return;
            }

            Dialogs.Close(player);
            player.CharacterData.JobId = JobId.None;
            deleteVehicle(player);

        }


        public static void OnRentVeh(ENetPlayer player, params object[] arguments)
        {
           
            Dialogs.Close(player);
            if (!player.GetSessionData(out var sessionData)) return;

           if (player.CharacterData.JobId != JobId.Trucker)
           {
               player.SendError("Вы должны устроиться на работу");
               return;
           }

           if( !player.ChangeWallet(-1000) )
           {
               return;
           }

           sessionData.WorkData.TruckerWorkData = new TruckerWorkData();
           sessionData.WorkData.TruckerWorkData.IsStopped = false;


           var spawn = Config.VehSpawns[ENet.Random.Next(0, Config.VehSpawns.Count)];

           uint modelHash = (uint)(VehicleHash)NAPI.Util.GetHashKey("nero2");
           ENetVehicle vehicle = ENet.Vehicle.CreateVehicle(
               modelHash,
               spawn.GetVector3(),
               spawn.GetHeading(),
               111,
               111,
               GenerateNumberPlateText(),
               255, true, false, 0
               );

           //vehicle.Rotation = new Vector3(0, 0, spawn.GetHeading());
           vehicle.SetType(VehicleType.Work);
           vehicle.LockStatus(false);
           vehicle.EngineState(false);
           vehicle.SetOwner(player.GetUUID());
           NAPI.Vehicle.SetVehicleCustomPrimaryColor(vehicle, 255, 255, 255);


           TruckerCarData rentalData = new TruckerCarData()
           {
               CharacterId = player.CharacterData.UUID,
               CarModelName = "nero2",
               RentalCar = vehicle
           };
           sessionData.WorkData.Vehicle = vehicle;
           _rentsData.Add(rentalData);
           player.SendInfo($"Вы арендовали mule. Едьте на точку загрузки!");
           player.SetIntoVehicle(vehicle, (int)VehicleSeat.Driver);
           getLoadPoint(player);
           

        }

        private static void openMenu ( ENetPlayer player )
        {

            Framework.ClientEvent.Event(player, "client.jobs.trucker.open");
        }

        [CustomEvent("server.jobs.tucker.buy")]
        private static void buyVeh(ENetPlayer player, string vehName)
        {

        }

        private static void getLoadPoint( ENetPlayer player )
        {
            if (!player.GetSessionData(out var sessionData)) return;

            Framework.ClientEvent.Event(player, "client.checkpoints.destroyJob", "trucker");
            var loadPoint = Config.LoadPos[ENet.Random.Next(0, Config.LoadPos.Count)];
            Framework.ClientEvent.Event(player, "client.checkpoints.createJob", "trucker", 473, 5, loadPoint.GetVector3().X, loadPoint.GetVector3().Y, loadPoint.GetVector3().Z, 7);
            
            if(sessionData.WorkData.TruckerWorkData.ColShape != null)
            {
                sessionData.WorkData.TruckerWorkData.ColShape.Delete();
            }

            sessionData.WorkData.TruckerWorkData.ColShape = ENet.ColShape.CreateCylinderColShape(loadPoint.GetVector3(), 2, 2, 0, ColShapeType.TruckerLoadPoint);
        }

        [InteractionDeprecated(ColShapeType.TruckerLoadPoint)]
        public static void OnTruckerLoadPoint(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData) || !player.IsInVehicle) return;

                var vehicle = sessionData.WorkData.Vehicle;
                if (vehicle != player.Vehicle) return;
                if (sessionData.WorkData.TruckerWorkData.IsStopped) return;

                sessionData.WorkData.TruckerWorkData.IsStopped = true;
                Framework.ClientEvent.Event(player, "client.blockVeh", true);

                vehicle.SetSharedData("freezePosition", true);
                vehicle.EngineState(false);

                Framework.ClientEvent.Event(player, "client.checkpoints.destroyJob", "trucker");
                player.SendInfo($"Ожидание 10 секунд");

                player.SetData("truckerCheckpoint.timer", Timers.Start(10 * 1000, () =>
                {
                    try
                    {
                        if (player is null) return;

                        if (player.GetData("truckerCheckpoint.timer", out string timerId))
                        {
                            Timers.Stop(timerId);
                            player.ResetData("truckerCheckpoint.timer");
                        }

                        sessionData.WorkData.TruckerWorkData.IsStopped = false;

                        Framework.ClientEvent.Event(player, "client.blockVeh", false);
                        if (vehicle != null)
                        {
                            vehicle.SetSharedData("freezePosition", false);
                        }
                        
                        getUnLoadPoint(player);
                        player.SendInfo($"Едьте на точку выгрузки!");
                    }
                    catch (Exception ex) { Logger.WriteError("Interaction.Task", ex); }
                }, true));
            }
            catch (Exception ex) { Logger.WriteError("OnTruckerLoadPoint", ex); }
        }

        private static void getUnLoadPoint(ENetPlayer player)
        {
            if (!player.GetSessionData(out var sessionData)) return;

            Framework.ClientEvent.Event(player, "client.checkpoints.destroyJob", "trucker");
            var loadPoint = Config.UnLoadPos[ENet.Random.Next(0, Config.UnLoadPos.Count)];
            Framework.ClientEvent.Event(player, "client.checkpoints.createJob", "trucker", 473, 5, loadPoint.GetVector3().X, loadPoint.GetVector3().Y, loadPoint.GetVector3().Z, 7);

            if (sessionData.WorkData.TruckerWorkData.ColShape != null)
            {
                sessionData.WorkData.TruckerWorkData.ColShape.Delete();
            }

            sessionData.WorkData.TruckerWorkData.ColShape = ENet.ColShape.CreateCylinderColShape(loadPoint.GetVector3(), 2, 2, 0, ColShapeType.TruckerUnloadPoint);
        }

        [InteractionDeprecated(ColShapeType.TruckerUnloadPoint)]
        public static void OnTruckerUnloadPoint(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData) || !player.IsInVehicle) return;

                if (sessionData.WorkData.TruckerWorkData.IsStopped) return;

                var vehicle = sessionData.WorkData.Vehicle;
                if (vehicle != player.Vehicle) return;

                sessionData.WorkData.TruckerWorkData.IsStopped = true;
                Framework.ClientEvent.Event(player, "client.blockVeh", true);

                vehicle.SetSharedData("freezePosition", true);
                vehicle.EngineState(false);

                Framework.ClientEvent.Event(player, "client.checkpoints.destroyJob", "bus");
                player.SendInfo($"Ожидание 10 секунд");

                player.SetData("truckerCheckpoint.timer", Timers.Start(10 * 1000, () =>
                {
                    try
                    {
                        if (player is null) return;

                        if (player.GetData("truckerCheckpoint.timer", out string timerId))
                        {
                            Timers.Stop(timerId);
                            player.ResetData("truckerCheckpoint.timer");
                        }

                        sessionData.WorkData.TruckerWorkData.IsStopped = false;
                        Framework.
                                                ClientEvent.Event(player, "client.blockVeh", false);
                        if (vehicle != null)
                        {
                            vehicle.SetSharedData("freezePosition", false);
                        }

                        //player.ChangeWallet( 1000 * sessionData.WorkData.Vehicle.GetData<float>("multiplier") * (characterData.JobLvl / 10));
                        getLoadPoint(player);
                    }
                    catch (Exception ex) { Logger.WriteError("Interaction.Task", ex); }
                }, true));
            }
            catch (Exception ex) { Logger.WriteError("OnTruckerUnloadPoint", ex); }
        }

        private static void deleteVehicle( ENetPlayer player )
        {
            if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

            if (sessionData.WorkData.VehicleTimer != null)
            {
                Timers.Stop(sessionData.WorkData.VehicleTimer);
                sessionData.WorkData.VehicleTimer = null;
            }

            if (sessionData.WorkData.Vehicle != null)
            {
                sessionData.WorkData.Vehicle.Delete();
            }

            Framework.ClientEvent.Event(player, "client.checkpoints.destroyJob", "trucker");
            if (sessionData.WorkData.TruckerWorkData.ColShape != null)
            {
                sessionData.WorkData.TruckerWorkData.ColShape.Delete();
            }
            if(sessionData.WorkData.TruckerWorkData != null)
            {
                sessionData.WorkData.TruckerWorkData = null;
            }
            player.SendInfo($"Работа закончена!");
        }

        private static string GenerateNumberPlateText()
        {
            return Guid.NewGuid().ToString()[..8];
        }
    }
}
