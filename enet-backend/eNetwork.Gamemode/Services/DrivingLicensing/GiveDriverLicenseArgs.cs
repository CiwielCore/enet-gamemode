using eNetwork.Framework.Classes;
using System;

namespace eNetwork.Services.DrivingLicensing
{
    class GiveDriverLicenseArgs : EventArgs
    {
        public DrivingLicenseClass LicenseClass { get; set; }

        public GiveDriverLicenseArgs(DrivingLicenseClass licenseClass)
        {
            LicenseClass = licenseClass;
        }
    }
}
