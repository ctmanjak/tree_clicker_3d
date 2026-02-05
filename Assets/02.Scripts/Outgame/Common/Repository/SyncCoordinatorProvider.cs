using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Outgame
{
    public class SyncCoordinatorProvider : IRepositoryProvider, IFlushable
    {
        private readonly IFirebaseAuthService _authService;
        private readonly IFirebaseStoreService _storeService;

        private SyncCoordinator _syncCoordinator;

        public IAccountRepository AccountRepository { get; private set; }
        public ICurrencyRepository CurrencyRepository { get; private set; }
        public IUpgradeRepository UpgradeRepository { get; private set; }

        public SyncCoordinatorProvider(
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

            _syncCoordinator = new SyncCoordinator(_storeService);

            var localCurrencyRepo = new LocalCurrencyRepository();
            var localUpgradeRepo = new LocalUpgradeRepository();
            var firebaseCurrencyRepo = new FirebaseCurrencyRepository(_storeService);
            var firebaseUpgradeRepo = new FirebaseUpgradeRepository(_storeService);

            await MergeAndInitialize(localCurrencyRepo, firebaseCurrencyRepo);
            await MergeAndInitialize(localUpgradeRepo, firebaseUpgradeRepo);

            CurrencyRepository = new SyncedCurrencyRepository(
                localCurrencyRepo,
                _syncCoordinator,
                timeOffset
            );

            UpgradeRepository = new SyncedUpgradeRepository(
                localUpgradeRepo,
                _syncCoordinator,
                timeOffset
            );

            Debug.Log("[SyncCoordinatorProvider] 초기화 완료");
        }

        private async UniTask<long> CalculateTimeOffset()
        {
            long serverTime = await _storeService.GetServerTimeAsync();
            long localTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long offset = serverTime - localTime;

            Debug.Log($"[SyncCoordinatorProvider] 시간 오프셋 계산 완료: {offset}초");
            return offset;
        }

        private async UniTask MergeAndInitialize<T>(
            IRepository<T> localRepo,
            IRepository<T> firebaseRepo)
            where T : IIdentifiable, ITimestamped
        {
            var localItems = await localRepo.Initialize();

            List<T> firebaseItems;
            try
            {
                firebaseItems = await firebaseRepo.Initialize();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SyncCoordinatorProvider] Firebase 로드 실패, 로컬만 사용: {ex.Message}");
                return;
            }

            var merged = MergeByTimestamp(localItems, firebaseItems);

            foreach (var item in merged)
            {
                localRepo.Save(item);
            }
        }

        private List<T> MergeByTimestamp<T>(List<T> local, List<T> firebase)
            where T : IIdentifiable, ITimestamped
        {
            var merged = new Dictionary<string, T>();

            foreach (var item in local)
                merged[item.Id] = item;

            foreach (var fbItem in firebase)
            {
                if (!merged.TryGetValue(fbItem.Id, out var localItem)
                    || fbItem.LastModified >= localItem.LastModified)
                {
                    merged[fbItem.Id] = fbItem;
                }
            }

            return merged.Values.ToList();
        }

        public void ForceFlushAll()
        {
            _syncCoordinator?.ForceFlushAll();
        }

        public void Dispose()
        {
            _syncCoordinator?.Dispose();
        }
    }
}
