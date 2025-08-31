using System;

namespace eNetwork.Framework.Classes
{
    public class DrivingLicenseCategory
    {
        public int CharacterId { get; set; }
        public DrivingLicenseClass Class { get; set; }
        public DateTime DateOfIssue { get; set; }
    }

    public enum DrivingLicenseClass
    {
        Moto,
        Car,
        Truck,
        Ship
    }

    public static class DrivingLicenseClassHelper
    {
        public static string ToDesignation(this DrivingLicenseClass licenseClass)
        {
            return licenseClass switch
            {
                DrivingLicenseClass.Moto => "A",
                DrivingLicenseClass.Car => "B",
                DrivingLicenseClass.Truck => "C",
                DrivingLicenseClass.Ship => "D",
                _ => throw new NotImplementedException(licenseClass.ToString())
            };
        }
    }
}
