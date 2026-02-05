using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;

namespace Outgame
{
    public class HybridCurrencyRepository : ICurrencyRepository, IDisposable
    {
        private readonly HybridRepository<CurrencySaveData> _hybrid;

        public string CollectionName => _hybrid.CollectionName;

        public HybridCurrencyRepository(
            ILocalRepository<CurrencySaveData> localRepository,
            ICurrencyRepository firebaseRepository,
            long timeOffset = 0)
        {
            _hybrid = new HybridRepository<CurrencySaveData>(localRepository, firebaseRepository, timeOffset);
        }

        public UniTask<List<CurrencySaveData>> Initialize() => _hybrid.Initialize();

        public void Save(CurrencySaveData item) => _hybrid.Save(item);

        public void ForceFlush() => _hybrid.ForceFlush();

        public void Dispose() => _hybrid.Dispose();
    }
}
