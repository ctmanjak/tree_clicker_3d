using System;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Outgame
{
    public class HybridRepositoryProvider : IRepositoryProvider, IFlushable
    {
        private readonly IFirebaseAuthService _authService;
        private readonly IFirebaseStoreService _storeService;

        private HybridCurrencyRepository _hybridCurrencyRepository;
        private HybridUpgradeRepository _hybridUpgradeRepository;

        public IAccountRepository AccountRepository { get; private set; }
        public ICurrencyRepository CurrencyRepository { get; private set; }
        public IUpgradeRepository UpgradeRepository { get; private set; }

        public HybridRepositoryProvider(
            IFirebaseAuthService authService,
            IFirebaseStoreService storeService)
        {
            _authService = authService;
            _storeService = storeService;
        }

        public async UniTask Initialize()
        {
            AccountRepository = new FirebaseAccountRepository(_authService);

            long timeOffset = await CalculateTimeOffset();

            _hybridCurrencyRepository = new HybridCurrencyRepository(
                new LocalCurrencyRepository(),
                new FirebaseCurrencyRepository(_storeService),
                timeOffset
            );
            CurrencyRepository = _hybridCurrencyRepository;

            _hybridUpgradeRepository = new HybridUpgradeRepository(
                new LocalUpgradeRepository(),
                new FirebaseUpgradeRepository(_storeService),
                timeOffset
            );
            UpgradeRepository = _hybridUpgradeRepository;
        }

        private async UniTask<long> CalculateTimeOffset()
        {
            long serverTime = await _storeService.GetServerTimeAsync();
            long localTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long offset = serverTime - localTime;

            Debug.Log($"[HybridRepositoryProvider] 시간 오프셋 계산 완료: {offset}초");
            return offset;
        }

        public void ForceFlushAll()
        {
            _hybridCurrencyRepository?.ForceFlush();
            _hybridUpgradeRepository?.ForceFlush();
        }

        public void Dispose()
        {
            _hybridCurrencyRepository?.Dispose();
            _hybridUpgradeRepository?.Dispose();
        }
    }
}
