using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;

namespace Outgame
{
    public class SyncedCurrencyRepository : ICurrencyRepository
    {
        private readonly ILocalRepository<CurrencySaveData> _localRepository;
        private readonly ISyncCoordinator _syncCoordinator;
        private readonly long _timeOffset;

        public string CollectionName => _localRepository.CollectionName;

        public SyncedCurrencyRepository(
            ILocalRepository<CurrencySaveData> localRepository,
            ISyncCoordinator syncCoordinator,
            long timeOffset = 0)
        {
            _localRepository = localRepository;
            _syncCoordinator = syncCoordinator;
            _timeOffset = timeOffset;
        }

        public UniTask<List<CurrencySaveData>> Initialize()
        {
            return _localRepository.Initialize();
        }

        public void Save(CurrencySaveData item)
        {
            item.LastModified = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + _timeOffset;
            _syncCoordinator.RegisterPending(_localRepository.CollectionName, item, _localRepository);
        }

    }
}
