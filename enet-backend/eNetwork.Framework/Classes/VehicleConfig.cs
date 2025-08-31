using eNetwork.Framework.Classes;

namespace eNetwork.Framework
{
    public class VehicleConfig
    {
        public int MaxFuel { get; set; } = 1;
        public int PetrolRate { get; set; } = 1;
        public string PetrolType { get; set; } = "Regular";
        public bool HaveAutopilot { get; set; } = false;
        public DrivingLicenseClass? LicenseClass { get; set; }

        public string GetPetrolType()
        {
            return PetrolType;
        }
    }
}
