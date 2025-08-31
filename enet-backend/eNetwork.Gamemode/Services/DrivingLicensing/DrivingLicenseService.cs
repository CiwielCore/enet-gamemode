using eNetwork.Framework.Classes;
using eNetwork.Framework.Singleton;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eNetwork.Services.DrivingLicensing
{
    class DrivingLicenseService : Singleton<DrivingLicenseService>
    {
        public static event EventHandler<GiveDriverLicenseArgs> PlayerGiveDriverLicenseEvent;

        private readonly object _locker;
        private readonly List<DrivingLicense> _licenses;
        private readonly DrivingLicenseRepository _repository;

        private DrivingLicenseService()
        {
            _locker = new object();
            _licenses = new List<DrivingLicense>();
            _repository = new DrivingLicenseRepository();
        }

        public async Task InitPlayer(ENetPlayer player)
        {
            List<DrivingLicenseCategory> categories = await _repository.GetDrivingLicenseCategories(player.GetUUID());
            DrivingLicense license = new DrivingLicense(player, categories);

            lock (_locker)
            {
                _licenses.Add(license);
            }
        }

        public DrivingLicense GetDrivingLicense(ENetPlayer player)
        {
            lock (_locker)
            {
                return _licenses.Find(l => l.Owner == player);
            }
        }

        public void GiveLicense(ENetPlayer player, DrivingLicenseClass licenseClass)
        {
            DrivingLicense license = GetDrivingLicense(player);
            if (license.HasCategory(licenseClass))
                return;

            DrivingLicenseCategory category = new DrivingLicenseCategory()
            {
                CharacterId = player.GetUUID(),
                Class = licenseClass,
                DateOfIssue = DateTime.Now
            };

            _repository.CreateDrivingLicenseCategory(category).ContinueWith((t) =>
            {
                license.AddCategory(category);
                player.SendInfo($"Вы получили для водительских прав новую категорию: {category.Class.ToDesignation()}");
                PlayerGiveDriverLicenseEvent?.Invoke(player, new GiveDriverLicenseArgs(licenseClass));
            });
        }

        public void OnPlayerDisconnected(ENetPlayer player)
        {
            lock (_locker)
            {
                DrivingLicense license = GetDrivingLicense(player);
                _licenses.Remove(license);
            }
        }
    }
}
