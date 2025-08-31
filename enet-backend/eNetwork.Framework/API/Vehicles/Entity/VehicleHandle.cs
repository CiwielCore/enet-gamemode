using eNetwork.Framework.Classes;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eNetwork.Framework;
using Newtonsoft.Json;

namespace eNetwork
{
    public class ENetVehicle : Vehicle
    {
        private static readonly Logger _logger = new Logger("enet-vehicles");
        private const string DataSync = "vehicle.sync";

        public ENetVehicle(NetHandle handle) : base(handle) { }
        public VehicleData VehicleData;
        public float GetPetrol()
        {
            if (HasSharedData("petrol")) return GetSharedData<float>("petrol");
            return 0f;
        }
        public float GetMile()
        {
            if (HasSharedData("mile")) return GetSharedData<float>("mile");
            return 0f;
        }
        public void SetVehicleData(VehicleData data)
        {
            VehicleData = data;
        }
        public VehicleData GetVehicleData()
        {
            return VehicleData;
        }

        public VehicleType VehicleType = VehicleType.Server;
        public void SetType(VehicleType type)
        {
            VehicleType = type;
            NAPI.Task.Run(() => SetSharedData("vehicle.type", type.ToString()));
        }
        public int Driver { get; set; } = -1;
        public void SetOwner(int driver)
            => Driver = driver;
        public bool GetVehicleData(out VehicleData data)
        {
            if (VehicleData is null)
            {
                data = null;
                return false;
            }
            data = VehicleData;
            return true;
        }

        public void SetDimension(uint dimension)
        {
            NAPI.Task.Run(() => Dimension = dimension);
        }

        public void ApplyCustomization()
        {
            try
            {
                if (VehicleData is null) return;

                VehicleCustomization data = VehicleData.Components;

                if (data.NeonColor.Alpha != 0)
                {
                    NAPI.Vehicle.SetVehicleNeonState(this, true);
                    NAPI.Vehicle.SetVehicleNeonColor(this, data.NeonColor.Red, data.NeonColor.Green, data.NeonColor.Blue);
                }

                SetMod(4, data.Muffler);
                SetMod(3, data.SideSkirt);
                SetMod(7, data.Hood);
                SetMod(0, data.Spoiler);
                SetMod(6, data.Lattice);
                SetMod(8, data.Wings);
                SetMod(10, data.Roof);
                SetMod(48, data.Vinyls);
                SetMod(1, data.FrontBumper);
                SetMod(2, data.RearBumper);

                SetMod(11, data.Engine);
                SetMod(18, data.Turbo);
                SetMod(13, data.Transmission);
                SetMod(15, data.Suspension);
                SetMod(12, data.Brakes);
                SetMod(14, data.Horn);
                SetMod(16, data.Armor);

                WindowTint = data.WindowTint;
                NumberPlateStyle = data.NumberPlate;

                if (data.Headlights >= 0)
                {
                    SetMod(22, 0);
                    SetSharedData("vehicle.highlight.color", data.Headlights);
                }
                else
                {
                    SetMod(22, -1);
                    SetSharedData("vehicle.highlight.color", 0);
                }


                PrimaryColor = data.PrimModColor;
                SecondaryColor = data.SecModColor;

                SetColors(data.PrimModColor, data.SecModColor);

                //NAPI.Vehicle.SetVehicleCustomPrimaryColor(this, data.PrimColor.Red, data.PrimColor.Green, data.PrimColor.Blue);
                //NAPI.Vehicle.SetVehicleCustomSecondaryColor(this, data.SecColor.Red, data.SecColor.Green, data.SecColor.Blue);

                WheelType = data.WheelsType;
                SetMod(23, data.Wheels);

                SetDirt(VehicleData.Dirt);

                VehicleSyncData syncData = GetSyncData();
                UpdateSyncData(syncData);
            }
            catch(Exception ex) { _logger.WriteError("ApplyCustomization: " + ex.ToString()); }
        }

        public void SetDirt(float dirt)
        {
            try { 
                VehicleSyncData data = GetSyncData();
                if (data == default(VehicleSyncData)) data = new VehicleSyncData();
                data.Dirt = dirt;
                UpdateSyncData(data);

                if (VehicleData != null)
                {
                    VehicleData.Dirt = data.Dirt;
                }
            }
            catch (Exception e) { _logger.WriteError("SetVehicleDirt", e); }
        }

        public void SetColors(int color1, int color2)
        {
            try { 
                VehicleSyncData data = GetSyncData();
                if (data == default(VehicleSyncData)) data = new VehicleSyncData();

                data.Color1 = color1;
                data.Color2 = color2;
                UpdateSyncData(data);
            }
            catch (Exception e) { _logger.WriteError("SetVehicleColors", e); }
        }

        public VehicleSyncData GetSyncData()
        {
            if (!HasSharedData(DataSync))
            {
                var data = new VehicleSyncData();

                data.Color1 = NAPI.Vehicle.GetVehiclePrimaryColor(this);
                data.Color2 = NAPI.Vehicle.GetVehicleSecondaryColor(this);

                data.BodyHealth = NAPI.Vehicle.GetVehicleBodyHealth(this);

                SetSharedData(DataSync, JsonConvert.SerializeObject(data));
                return data;
            }
            else return JsonConvert.DeserializeObject<VehicleSyncData>(GetSharedData<string>(DataSync));
        }
        public bool GetEngine()
        {
            try
            {
                var data = GetSyncData();
                return data.Engine;
            }
            catch (Exception e) { _logger.WriteError("GetEngine", e); return false; }
        }
        public bool GetLocked()
        {
            try
            {
                var data = GetSyncData();
                return data.Locked;
            }
            catch (Exception e) { _logger.WriteError("GetLocked", e); return false; }
        }

        public int GetMaxFuel()
        {
            try
            {
                if (!HasSharedData("max.petrol")) return 120;
                var data = GetSharedData<float>("max.petrol");
                return Convert.ToInt32(data);
            }
            catch (Exception e) { _logger.WriteError("GetMaxFuel", e); return 120; }
        }

        public void UpdateSyncData(VehicleSyncData data)
        {
            try
            {
                if (!NAPI.Entity.DoesEntityExist(this) || data is null) return;
                if (data == default(VehicleSyncData))
                    data = GetSyncData();

                SetSharedData(DataSync, JsonConvert.SerializeObject(data));
            }
            catch (Exception e) { _logger.WriteError("UpdateSyncData", e); }
        }
        public void LockStatus(bool status)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    var data = GetSyncData();

                    SetSharedData("IS_LOCKED", status);
                    data.Locked = status;
                    Locked = status;
                    UpdateSyncData(data);

                    ClientEvent.EventInRange(Position, Helper.DrawDistance, "client.vehicleSync.lock", this, status);
                }
                catch (Exception e) { _logger.WriteError("LockStatus", e); }
            });
        }
        public void EngineState(bool state)
        {
            NAPI.Task.Run(() => 
            { 
                try
                {
                    var data = GetSyncData();

                    NAPI.Vehicle.SetVehicleEngineStatus(this, state);
                    data.Engine = state;

                    UpdateSyncData(data);
                    ClientEvent.EventInRange(Position, Helper.DrawDistance, "client.vehicleSync.engine", this, state, true, data.LeftIL, data.RightIL);
                }
                catch (Exception e) { _logger.WriteError("EngineState", e); }
            });
        }
        public void LightState(Lights light, bool state)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    var data = GetSyncData();
                    if (data.Engine)
                    {
                        if (light == Lights.Right)
                        {
                            SetSharedData("RIGHT_LIGHT", state);
                            data.RightIL = state;
                        }
                        else
                        {
                            SetSharedData("LEFT_LIGHT", state);
                            data.LeftIL = state;
                        }
                        UpdateSyncData(data);
                        ClientEvent.EventInDimension(Dimension, "client.vehicleSync.light", Handle, light, state);
                    }
                }
                catch (Exception e) { _logger.WriteError("LightState", e); }
            });
        }

        public void SetVehicleWheelState(WheelID wheel, WheelState state)
        {
            var data = GetSyncData();
            data.Wheel[(int)wheel] = (int)state;
            UpdateSyncData(data);
        }

        public WheelState GetVehicleWheelState(WheelID wheel)
        {
            var data = GetSyncData();
            return (WheelState)data.Wheel[(int)wheel];
        }

        public void SetVehicleWindowState(WindowID window, WindowState state)
        {
            var data = GetSyncData();

            data.Window[(int)window] = (int)state;
            UpdateSyncData(data);
        }

        public WindowState GetVehicleWindowState(WindowID window)
        {
            var data = GetSyncData();
            return (WindowState)data.Window[(int)window];
        }

        public void SetDoorState(DoorID door, DoorState state)
        {
            var data = GetSyncData();

            data.Door[(int)door] = (int)state;
            UpdateSyncData(data);
        }

        public DoorState GetDoorState(DoorID door)
        {
            var data = GetSyncData();
            return (DoorState)data.Door[(int)door];
        }
    }
}
