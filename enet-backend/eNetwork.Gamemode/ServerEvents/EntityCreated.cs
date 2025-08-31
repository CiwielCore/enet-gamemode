using eNetwork.Framework;
using eNetwork.Game.Vehicles;
using GTANetworkAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.ServerEvents
{
    internal class EntityCreated : Script
    {
        private static readonly Logger Logger = new Logger("entity-created");

        [ServerEvent(Event.EntityCreated)]
        public static void OnEvent(Entity entity)
        {
            try
            {
                switch(entity.Type)
                {
                    case EntityType.Vehicle:
                        ENetVehicle vehicle = NAPI.Entity.GetEntityFromHandle<ENetVehicle>(entity);

                        VehicleConfig vehicleConfig = VehicleSync.GetVehicleConfig(vehicle);

                        float maxPetrol = 60;
                        float currentPetrol = maxPetrol;
                        float petrolRate = 1f;
                        if (vehicleConfig != null)
                        {
                            // Проверка на есть ли тачка в списках
                            maxPetrol = (float)vehicleConfig.MaxFuel;
                            petrolRate = (float)vehicleConfig.PetrolRate;

                            if (vehicleConfig.HaveAutopilot)
                            {
                                vehicle.SetSharedData("have_autopilot", true);
                            }
                        }

                        vehicle.SetSharedData("mile", 0f);
                        vehicle.SetSharedData("petrol", currentPetrol);
                        vehicle.SetSharedData("max.petrol", maxPetrol);
                        vehicle.SetSharedData("petrol.rate", petrolRate);
                        vehicle.SetData("last.update", DateTime.Now);

                        vehicle.GetSyncData();
                        return;
                }
            }
            catch(Exception e) { Logger.WriteError("OnEvent", e); }
        }
    }
}
