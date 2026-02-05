using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;

namespace Outgame
{
    public class SyncedUpgradeRepository : IUpgradeRepository
    {
        private readonly LocalUpgradeRepository _localRepository;
        private readonly ISyncCoordinator _syncCoordinator;
        private readonly long _timeOffset;

        public string CollectionName => _localRepository.CollectionName;

        public SyncedUpgradeRepository(
            LocalUpgradeRepository localRepository,
            ISyncCoordinator syncCoordinator,
            long timeOffset = 0)
        {
            _localRepository = localRepository;
            _syncCoordinator = syncCoordinator;
            _timeOffset = timeOffset;
        }

        public UniTask<List<UpgradeSaveData>> Initialize()
        {
            return _localRepository.Initialize();
        }

        public void Save(UpgradeSaveData item)
        {
            item.LastModified = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + _timeOffset;
            _syncCoordinator.RegisterPending(_localRepository.CollectionName, item, _localRepository);
        }

    }
}
