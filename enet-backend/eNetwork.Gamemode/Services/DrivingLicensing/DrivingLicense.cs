using eNetwork.Framework.Classes;
using System.Collections.Generic;

namespace eNetwork.Services.DrivingLicensing
{
    class DrivingLicense
    {
        public ENetPlayer Owner => _owner;

        private readonly ENetPlayer _owner;
        private readonly List<DrivingLicenseCategory> _categories;

        public DrivingLicense(ENetPlayer owner, List<DrivingLicenseCategory> categories)
        {
            _owner = owner;
            _categories = categories;
        }

        public bool AddCategory(DrivingLicenseCategory category)
        {
            if (HasCategory(category.Class))
                return false;

            _categories.Add(category);
            return true;
        }

        public bool HasCategory(DrivingLicenseClass licenseClass)
        {
            return _categories.Exists(c => c.Class == licenseClass);
        }
    }
}
