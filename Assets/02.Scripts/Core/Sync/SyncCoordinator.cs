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
        
        private readonly Dictionary<string, Dictionary<string, Func<IIdentifiable>>> _dirtyItems = new();

        private int _localSaveCount;
        private bool _isSyncInProgress;
        private CancellationTokenSource _debounceCts;

        public SyncCoordinator(IFirebaseStoreService storeService)
        {
            _storeService = storeService;
        }

        public void RegisterPending<T>(string collection, T item, ILocalRepository<T> localRepo)
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
                SaveToLocal = () => localRepo.Save(item),
                GetFromLocal = () => localRepo.Get(item.Id)
            };

            RestartDebounceTimer();
        }

        public void MarkDirty<T>(string collection, string id, ILocalRepository<T> localRepo)
            where T : IIdentifiable
        {
            AddToDirtyItems(collection, id, () => localRepo.Get(id));
        }

        public void ForceFlushAll()
        {
            _debounceCts?.Cancel();
            
            if (HasPendingItems())
            {
                var snapshot = TakePendingSnapshot();
                FlushToLocal(snapshot);
            }
            
            FlushToFirebase().Forget();
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
            catch (Exception ex)
            {
                Debug.LogError($"[SyncCoordinator] Debounce 타이머 처리 중 예외 발생: {ex}");
            }
        }

        private async UniTask FlushPending(bool forceFirebase)
        {
            if (HasPendingItems())
            {
                var snapshot = TakePendingSnapshot();
                int totalCount = FlushToLocal(snapshot);

                _localSaveCount++;
                Debug.Log($"[SyncCoordinator] 로컬 저장 완료: {totalCount}건 (배치 #{_localSaveCount})");

                bool shouldSyncFirebase = forceFirebase || _localSaveCount >= SaveCountThreshold;

                if (shouldSyncFirebase && !_isSyncInProgress)
                {
                    await FlushToFirebase();
                }
            }
        }

        private int FlushToLocal(Dictionary<string, Dictionary<string, PendingItem>> snapshot)
        {
            int totalCount = 0;

            foreach (var (collection, items) in snapshot)
            {
                foreach (var (id, pending) in items)
                {
                    pending.SaveToLocal();
                    AddToDirtyItems(collection, id, pending.GetFromLocal);
                    totalCount++;
                }
            }

            return totalCount;
        }

        private void AddToDirtyItems(string collection, string id, Func<IIdentifiable> getFromLocal)
        {
            if (!_dirtyItems.TryGetValue(collection, out var collectionDirty))
            {
                collectionDirty = new Dictionary<string, Func<IIdentifiable>>();
                _dirtyItems[collection] = collectionDirty;
            }

            collectionDirty[id] = getFromLocal;
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

        private async UniTask FlushToFirebase()
        {
            if (!HasDirtyItems())
                return;

            Dictionary<string, Dictionary<string, Func<IIdentifiable>>> snapshot = null;

            try
            {
                _isSyncInProgress = true;
                _localSaveCount = 0;

                snapshot = TakeDirtySnapshot();

                var batch = _storeService.CreateWriteBatch();
                int totalCount = 0;

                foreach (var (collection, items) in snapshot)
                {
                    foreach (var (id, getFromLocal) in items)
                    {
                        var data = getFromLocal();
                        if (data != null)
                        {
                            batch.Set(collection, id, data);
                            totalCount++;
                        }
                    }
                }

                await batch.CommitAsync();
                Debug.Log($"[SyncCoordinator] Firebase WriteBatch 커밋 완료: {totalCount}건");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SyncCoordinator] Firebase WriteBatch 처리 중 예외 발생: {ex}");

                if (snapshot != null)
                {
                    RestoreDirtyItems(snapshot);
                }
            }
            finally
            {
                _isSyncInProgress = false;
            }
        }

        private bool HasDirtyItems()
        {
            foreach (var collectionItems in _dirtyItems.Values)
            {
                if (collectionItems.Count > 0)
                    return true;
            }
            return false;
        }

        private Dictionary<string, Dictionary<string, Func<IIdentifiable>>> TakeDirtySnapshot()
        {
            var snapshot = new Dictionary<string, Dictionary<string, Func<IIdentifiable>>>();
            foreach (var (collection, items) in _dirtyItems)
            {
                snapshot[collection] = new Dictionary<string, Func<IIdentifiable>>(items);
            }
            _dirtyItems.Clear();
            return snapshot;
        }

        private void RestoreDirtyItems(Dictionary<string, Dictionary<string, Func<IIdentifiable>>> snapshot)
        {
            foreach (var (collection, items) in snapshot)
            {
                if (!_dirtyItems.TryGetValue(collection, out var collectionDirty))
                {
                    collectionDirty = new Dictionary<string, Func<IIdentifiable>>();
                    _dirtyItems[collection] = collectionDirty;
                }

                foreach (var (id, getFromLocal) in items)
                {
                    if (!collectionDirty.ContainsKey(id))
                    {
                        collectionDirty[id] = getFromLocal;
                    }
                }
            }
            Debug.Log("[SyncCoordinator] Firebase 커밋 실패로 dirty items 복원됨");
        }

        private struct PendingItem
        {
            public IIdentifiable Item;
            public Action SaveToLocal;
            public Func<IIdentifiable> GetFromLocal;
        }
    }
}
