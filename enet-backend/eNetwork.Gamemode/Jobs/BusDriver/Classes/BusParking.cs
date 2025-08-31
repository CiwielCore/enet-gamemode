using eNetwork.Framework;
using eNetwork.GameUI;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.Framework.Classes.Sessions.Classes;
using System.Linq;
using eNetwork.Framework.Enums;

namespace eNetwork.Jobs.BusDriver.Classes
{
    public class BusParking
    {
        private static readonly Logger Logger = new Logger("bus-parking");

        public string Id { get; set; }
        public Position Position { get; set; }
        public List<Position> SpawnPositions { get; set; }
        public List<BusCheckpoint> Checkpoints { get; set; }

        private Ped _ped { get; set; }
        private ENetColShape _colShape { get; set; }
        private ENetBlip _blip { get; set; }

        public void GTAElements()
        {
            _ped = NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("ig_andreas"), Position.GetVector3(), Position.GetHeading(), false, true, true, true, 0);
            _ped.SetData("POSITION_DATA", Position);

            _blip = ENet.Blip.CreateBlip(351, Position.GetVector3(), .8f, 2, "Работа: Автобусник", 255, 0, true, 0, 0);

            _colShape = ENet.ColShape.CreateCylinderColShape(Position.GetVector3(), 2, 2, 0, ColShapeType.Bus);
            _colShape.OnEntityEnterColShape += (s, e) => e.SetData("busParking.data", this);
            _colShape.OnEntityExitColShape += (s, e) => e.ResetData("busParking.data");
            _colShape.SetInteractionText("Устройство на работу");

            if (Checkpoints.Any())
                Checkpoints.ForEach(checkpoint => checkpoint.GTAElements(this));
        }

        public void InteractionNpc(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                var dialog = new Dialog()
                {
                    Name = "Дядя Володя",
                    Description = "Владелец паркинга автобусников",
                };

                var answers = new List<DialogAnswer>();
                if (characterData.JobId != JobId.BusDriver)
                {
                    dialog.Text = "Привет, хочешь поработать у меня?";
                    answers.Add(new DialogAnswer("Да, давай", (p, o) => { Hire(player); InteractionNpc(player); }, "hire"));
                }
                else
                {
                    if (sessionData.WorkData.IsWorking)
                    {
                        dialog.Text = "Как поработал?";
                        answers.Add(new DialogAnswer("Остановить работу", (p, o) => { Stop(player); InteractionNpc(player); }, "stop"));
                    }
                    else
                    {
                        dialog.Text = "Ну что поработаем сейчас?";
                        answers.Add(new DialogAnswer("Начать работу", (p, o) => { Start(player); Dialogs.Close(player); }, "start"));
                    }

                    answers.Add(new DialogAnswer("Уволится", (p, o) => { Fire(player); InteractionNpc(player); }, "fire"));
                }

                answers.Add(new DialogAnswer("В другой раз...", (p, o) => { }, "close"));
                dialog.Answers = answers;

                dialog.Open(player, _ped);
            }
            catch(Exception ex) { Logger.WriteError("InteractionNpc", ex); }
        }

        public void Hire(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                if (characterData.JobId != JobId.None)
                {
                    player.SendError("Вы уже где-то устроены!");
                    return;
                }

                characterData.JobId = JobId.BusDriver;
                sessionData.WorkData.BusWorkData = new BusWorkData();

                player.SendDone("Вы устроились на работу Автобусника!");
            }
            catch(Exception ex) { Logger.WriteError("Hire", ex); }
        }

        public void Fire(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;
                
                if (characterData.JobId != JobId.BusDriver)
                {
                    player.SendError("Вы не работете на работе Автобусника!");
                    return;
                }

                if (sessionData.WorkData.IsWorking)
                {
                    player.SendError("Сначала закончите работу");
                    return;
                }

                characterData.JobId = JobId.None;
                sessionData.WorkData.BusWorkData = new BusWorkData();

                player.SendInfo("Вы уволились с работы Автобусника!");
            }
            catch (Exception ex) { Logger.WriteError("Fire", ex); }
        }

        public void Start(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                if (!SpawnPositions.Any())
                {
                    player.SendError("Ошибка создания транспорта");
                    return;
                }

                var position = SpawnPositions.ElementAt(ENet.Random.Next(0, SpawnPositions.Count));
                var vehicle = ENet.Vehicle.CreateVehicle(NAPI.Util.GetHashKey("coach"), position.GetVector3(), position.GetHeading(), 111, 70, "WORK", 255, true, false, 0);

                vehicle.SetType(VehicleType.Work);
                vehicle.SetOwner(player.GetUUID());

                player.SetIntoVehicle(vehicle, (int)VehicleSeat.Driver);
                
                sessionData.WorkData.IsWorking = true;
                sessionData.WorkData.Vehicle = vehicle;
                sessionData.WorkData.BusWorkData.BusStationId = Id;

                player.SendDone("Вы начали рабочий день! Следуйте по точкамю.");
                SendNext(player);
            }
            catch (Exception ex) { Logger.WriteError("Start", ex); }
        }

        public void Stop(ENetPlayer player)
        {
            try
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

                sessionData.WorkData.IsWorking = false;
                sessionData.WorkData.BusWorkData = new BusWorkData();

                player.SendInfo("Вы закончили работу!");

                ClientEvent.Event(player, "client.checkpoints.destroyJob", "bus");
            }
            catch (Exception ex) { Logger.WriteError("Stop", ex); }
        }

        public void SendNext(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                if (!sessionData.WorkData.IsWorking || characterData.JobId != JobId.BusDriver) return;

                if (sessionData.WorkData.BusWorkData.CheckpointId == Checkpoints.Count - 1)
                {
                    sessionData.WorkData.BusWorkData.CheckpointId = 0;
                }
                else sessionData.WorkData.BusWorkData.CheckpointId++;

                var checkPoint = Checkpoints.ElementAt(sessionData.WorkData.BusWorkData.CheckpointId);
                if (checkPoint is null)
                {
                    player.SendError("Неудалось построить маршрут, попробуйте еще раз!");
                    return;
                }

                ClientEvent.Event(player, "client.checkpoints.createJob", "bus", 513, 5, checkPoint.Position.X, checkPoint.Position.Y, checkPoint.Position.Z, 7);
                player.SendDone("Новая точка установлена!");
            }
            catch(Exception ex) { Logger.WriteError("SendNext", ex); }
        }
    }
}
