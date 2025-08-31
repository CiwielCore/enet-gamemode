using eNetwork.Framework;
using eNetwork.Framework.Singleton;
using eNetwork.Game.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eNetwork.Services.VipServices
{
    public class VipService : Singleton<VipService>
    {
        private readonly Logger _logger;
        private readonly VipRepository _repository;
        private readonly List<Vip> _vips;
        private readonly Dictionary<ENetPlayer, VipStatus> _statuses;
        private readonly object _locker;

        private VipService()
        {
            _logger = new Logger("vip-service");
            _repository = new VipRepository();
            _vips = new List<Vip>();
            _statuses = new Dictionary<ENetPlayer, VipStatus>();
            _locker = new object();
        }

        public void OnResourceStart()
        {
            InitVips();
            ClearExpiredVips();
        }

        public Vip GetVipByName(string vipName)
        {
            return _vips.FirstOrDefault(v => v.Name == vipName);
        }

        public async Task<bool> BuyVipStatus(ENetPlayer player, string vipName)
        {
            try
            {
                Vip vip = GetVipByName(vipName);
                if (vip is null)
                    throw new NullReferenceException(vipName);

                int characterId = player.GetUUID();
                AccountData account = AccountManager.GetAccountByUUID(characterId);
                if (account is null)
                    throw new NullReferenceException(nameof(account));

                if (account.DonatePoints < vip.Price)
                {
                    player.SendError($"Недостаточно донат поинтов");
                    return false;
                }

                account.DonatePoints -= vip.Price;
                await GiveVipToCharacter(player, vip);
                await AccountManager.Save(player);
                return true;
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex.Message);
                return false;
            }
        }

        public async Task GiveVipToCharacter(ENetPlayer player, Vip vip)
        {
            VipStatus status = new VipStatus();
            status.CharacterId = player.GetUUID();
            status.VipName = vip.Name;
            status.DateOfIssue = DateTime.Now;
            status.DateOfEnd = DateTime.Now.AddDays(vip.ValidDays);

            await _repository.CreateVipStatusInDB(status);

            lock (_locker)
            {
                _statuses.Add(player, status);
            }
        }

        public async Task GiveVipStatusToCharacter(int characterId, VipStatus status)
        {
            if (status is null)
                return;

            status.CharacterId = characterId;
            await _repository.CreateVipStatusInDB(status);

            ENetPlayer player = ENet.Pools.GetPlayerByUUID(characterId);
            if (player is not null)
            {
                lock (_statuses)
                {
                    _statuses.Add(player, status);
                }
            }
        }

        public Vip GetVipOfPlayer(ENetPlayer player)
        {
            if (!_statuses.TryGetValue(player, out VipStatus status))
                return null;

            return GetVipByName(status.VipName);
        }

        public async Task InitPlayer(ENetPlayer player)
        {
            if (player is null)
                return;

            lock (_locker)
            {
                if (_statuses.ContainsKey(player))
                    _statuses.Remove(player);
            }

            VipStatus status = await _repository.GetVipStatusByCharacterId(player.GetUUID());
            if (status is null)
                return;

            if (status.DateOfEnd <= DateTime.Now)
            {
                await _repository.DeleteVipStatusForCharacter(status.CharacterId);
                return;
            }

            lock (_locker)
            {
                _statuses.Add(player, status);
            }
        }

        public void OnPlayerDisconnected(ENetPlayer player)
        {
            lock (_locker)
            {
                _statuses.Remove(player);
            }
        }

        private void InitVips()
        {
            Type vipType = typeof(Vip);
            IEnumerable<Type> types = vipType.GetTypeInfo().Assembly.GetTypes()
                .Where((t) => !t.IsAbstract && t.IsClass && t.IsSubclassOf(vipType));

            foreach (Type t in types)
            {
                Vip vip = (Vip)Activator.CreateInstance(t);
                _vips.Add(vip);
            }

            _logger.WriteInfo($"Initialized {_vips.Count} types of vip...");
        }

        private void ClearExpiredVips()
        {
            Task.Run(_repository.ClearExpiredVipStatuses);
            TimeSpan nowTime = DateTime.Now.TimeOfDay;
            TimeSpan updateTime = new TimeSpan(24, 0, 0);
            TimeSpan nextTime = updateTime.Subtract(nowTime);
            Timers.StartOnce(Convert.ToInt32(nextTime.TotalMilliseconds), ClearExpiredVips);

            lock (_locker)
            {
                foreach (ENetPlayer player in _statuses.Keys)
                {
                    VipStatus status = _statuses[player];
                    if (status.DateOfEnd >= DateTime.Now)
                        continue;

                    player.SendInfo($"Срок вашего вип статуса ({status.VipName}) истек.");
                    _statuses.Remove(player);
                }
            }
        }
    }
}