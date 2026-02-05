using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;

namespace Outgame
{
    public class SyncedCurrencyRepository : ICurrencyRepository
    {
        private const string CollectionName = "currencies";

        private readonly LocalCurrencyRepository _localRepository;
        private readonly ISyncCoordinator _syncCoordinator;
        private readonly long _timeOffset;

        public SyncedCurrencyRepository(
            LocalCurrencyRepository localRepository,
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
            _syncCoordinator.RegisterPending(CollectionName, item, _localRepository);
        }
    }
}
