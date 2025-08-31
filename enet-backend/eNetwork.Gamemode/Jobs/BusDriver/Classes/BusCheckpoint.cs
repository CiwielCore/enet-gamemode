using eNetwork.Framework;
using eNetwork.Jobs.BusDriver.Data;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Jobs.BusDriver.Classes
{
    public class BusCheckpoint
    {
        private static readonly Logger Logger = new Logger("bus-checkpoint");

        public Vector3 Position { get; set; }

        public BusCheckpoint(Vector3 position)
        {
            Position = position;
        }

        public int Index { get; set; }

        public BusParking BusParking { get; set; }
        private ENetColShape _colShape { get; set; }
        public void GTAElements(BusParking busParking)
        {
            BusParking = busParking;
            Index = BusParking.Checkpoints.IndexOf(this);

            _colShape = ENet.ColShape.CreateCylinderColShape(Position, 6, 2, 0, ColShapeType.BusCheckpoint);
            _colShape.OnEntityEnterColShape += (s, e) => e.SetData("busCheckpoint.data", this);
            _colShape.OnEntityExitColShape += (s, e) => e.ResetData("busCheckpoint.data");
            _colShape.AddPredicate((shape, player) =>
            {
                if (!player.GetSessionData(out var sessionData) || sessionData.WorkData.BusWorkData.BusStationId != BusParking.Id || sessionData.WorkData.BusWorkData.CheckpointId != Index || sessionData.WorkData.Vehicle is null
                    || player.Vehicle != sessionData.WorkData.Vehicle || sessionData.WorkData.Vehicle.VehicleType != VehicleType.Work) return false;
                return true;
            });
            _colShape.SetInteractionText($"Остановка #{Index + 1}");
        }

        public void Interaction(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData) || !player.IsInVehicle) return;

                if (sessionData.WorkData.BusWorkData.IsStopped || sessionData.WorkData.BusWorkData.CheckpointId != Index) return;

                var vehicle = sessionData.WorkData.Vehicle;
                if (vehicle != player.Vehicle) return;

                sessionData.WorkData.BusWorkData.IsStopped = true;
                ClientEvent.Event(player, "client.blockVeh", true);

                vehicle.SetSharedData("freezePosition", true);
                vehicle.EngineState(false);

                ClientEvent.Event(player, "client.checkpoints.destroyJob", "bus");
                player.SendInfo($"Ожидание {Config.WAIT_CHECKPOINT_TIME} секунд");

                player.SetData("busCheckpoint.timer", Timers.Start(Config.WAIT_CHECKPOINT_TIME * 1000, () =>
                {
                    try
                    {
                        if (player is null) return;

                        if (player.GetData("busCheckpoint.timer", out string timerId))
                        {
                            Timers.Stop(timerId);
                            player.ResetData("busCheckpoint.timer");
                        }

                        sessionData.WorkData.BusWorkData.IsStopped = false;

                        ClientEvent.Event(player, "client.blockVeh", false);
                        if (vehicle != null)
                        {
                            vehicle.SetSharedData("freezePosition", false);
                        }

                        player.ChangeWallet(Config.PAYMENT);
                        BusParking.SendNext(player);    
                    }
                    catch(Exception ex) { Logger.WriteError("Interaction.Task", ex); }
                }, true));
            }
            catch(Exception ex) { Logger.WriteError("Interaction", ex); }
        }
    }
}
