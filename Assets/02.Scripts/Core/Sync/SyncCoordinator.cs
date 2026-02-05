using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class SyncCoordinator : ISyncCoordinator
    {
        private const float DebounceDelaySeconds = 1f;
        private const int SaveCountThreshold = 5;

        private readonly IFirebaseStoreService _storeService;

        private readonly Dictionary<string, Dictionary<string, PendingItem>> _pendingItems = new();
        private int _localSaveCount;
        private bool _isSyncInProgress;
        private CancellationTokenSource _debounceCts;

        public SyncCoordinator(IFirebaseStoreService storeService)
        {
            _storeService = storeService;
        }

        public void RegisterPending<T>(string collection, T item, IRepository<T> localRepo)
            where T : IIdentifiable, ITimestamped
        {
            if (!_pendingItems.TryGetValue(collection, out var collectionPending))
            {
                collectionPending = new Dictionary<string, PendingItem>();
                _pendingItems[collection] = collectionPending;
            }

            collectionPending[item.Id] = new PendingItem
            {
                Item = item,
                SaveToLocal = () => localRepo.Save(item)
            };

            RestartDebounceTimer();
        }

        public void ForceFlushAll()
        {
            _debounceCts?.Cancel();
            FlushPending(forceFirebase: true).Forget();
        }

        public void Dispose()
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = null;
        }

        private void RestartDebounceTimer()
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();

            StartDebounceTimer(_debounceCts.Token).Forget();
        }

        private async UniTaskVoid StartDebounceTimer(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(DebounceDelaySeconds),
                    cancellationToken: cancellationToken
                );

                await FlushPending(forceFirebase: false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTask FlushPending(bool forceFirebase)
        {
            if (HasPendingItems())
            {
                var snapshot = TakePendingSnapshot();
                int totalCount = 0;

                foreach (var (collection, items) in snapshot)
                {
                    foreach (var (id, pending) in items)
                    {
                        pending.SaveToLocal();
                        totalCount++;
                    }
                }

                _localSaveCount++;
                Debug.Log($"[SyncCoordinator] 로컬 저장 완료: {totalCount}건 (배치 #{_localSaveCount})");

                bool shouldSyncFirebase = forceFirebase || _localSaveCount >= SaveCountThreshold;

                if (shouldSyncFirebase && !_isSyncInProgress)
                {
                    await FlushToFirebase(snapshot);
                }
            }
        }

        private bool HasPendingItems()
        {
            foreach (var collectionItems in _pendingItems.Values)
            {
                if (collectionItems.Count > 0)
                    return true;
            }
            return false;
        }

        private Dictionary<string, Dictionary<string, PendingItem>> TakePendingSnapshot()
        {
            var snapshot = new Dictionary<string, Dictionary<string, PendingItem>>();
            foreach (var (collection, items) in _pendingItems)
            {
                snapshot[collection] = new Dictionary<string, PendingItem>(items);
            }
            _pendingItems.Clear();
            return snapshot;
        }

        private async UniTask FlushToFirebase(Dictionary<string, Dictionary<string, PendingItem>> snapshot)
        {
            _isSyncInProgress = true;
            _localSaveCount = 0;

            try
            {
                var batch = _storeService.CreateWriteBatch();
                int totalCount = 0;

                foreach (var (collection, items) in snapshot)
                {
                    foreach (var (id, pending) in items)
                    {
                        batch.Set(collection, id, pending.Item);
                        totalCount++;
                    }
                }

                await batch.CommitAsync();
                Debug.Log($"[SyncCoordinator] Firebase WriteBatch 커밋 완료: {totalCount}건");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SyncCoordinator] Firebase WriteBatch 커밋 실패: {ex.Message}");
                RestorePendingItems(snapshot);
            }
            finally
            {
                _isSyncInProgress = false;
            }
        }

        private void RestorePendingItems(Dictionary<string, Dictionary<string, PendingItem>> snapshot)
        {
            foreach (var (collection, items) in snapshot)
            {
                if (!_pendingItems.TryGetValue(collection, out var collectionPending))
                {
                    collectionPending = new Dictionary<string, PendingItem>();
                    _pendingItems[collection] = collectionPending;
                }

                foreach (var (id, pending) in items)
                {
                    if (!collectionPending.ContainsKey(id))
                    {
                        collectionPending[id] = pending;
                    }
                }
            }
            Debug.Log("[SyncCoordinator] Firebase 커밋 실패로 pending 복원됨");
        }

        private struct PendingItem
        {
            public IIdentifiable Item;
            public Action SaveToLocal;
        }
    }
}
