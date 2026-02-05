using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;

namespace Outgame
{
    public class HybridUpgradeRepository : IUpgradeRepository, IDisposable
    {
        private readonly HybridRepository<UpgradeSaveData> _hybrid;

        public string CollectionName => _hybrid.CollectionName;

        public HybridUpgradeRepository(
            ILocalRepository<UpgradeSaveData> localRepository,
            IUpgradeRepository firebaseRepository,
            long timeOffset = 0)
        {
            _hybrid = new HybridRepository<UpgradeSaveData>(localRepository, firebaseRepository, timeOffset);
        }

        public UniTask<List<UpgradeSaveData>> Initialize() => _hybrid.Initialize();

        public void Save(UpgradeSaveData item) => _hybrid.Save(item);

        public void ForceFlush() => _hybrid.ForceFlush();

        public void Dispose() => _hybrid.Dispose();
    }
}
