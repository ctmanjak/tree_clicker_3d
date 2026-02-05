using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Outgame
{
    public class HybridUpgradeRepository : IUpgradeRepository, IDisposable
    {
        private readonly HybridRepository<UpgradeSaveData> _hybrid;

        public HybridUpgradeRepository(
            IUpgradeRepository localRepository,
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
