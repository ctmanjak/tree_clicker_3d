using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Outgame
{
    public class HybridRepository<T> : IRepository<T>, IDisposable
        where T : IIdentifiable, ITimestamped
    {
        private const float DebounceDelaySeconds = 1f;
        private const int SaveCountThreshold = 5;

        private readonly ILocalRepository<T> _localRepository;
        private readonly IRepository<T> _firebaseRepository;
        private readonly long _timeOffset;

        public string CollectionName => _localRepository.CollectionName;

        private Dictionary<string, T> _pendingItems = new Dictionary<string, T>();
        private int _localSaveCount;
        private bool _isSyncInProgress;
        private CancellationTokenSource _debounceCts;

        public HybridRepository(
            ILocalRepository<T> localRepository,
            IRepository<T> firebaseRepository,
            long timeOffset = 0)
        {
            _localRepository = localRepository;
            _firebaseRepository = firebaseRepository;
            _timeOffset = timeOffset;
        }

        public async UniTask<List<T>> Initialize()
        {
            var localItems = await _localRepository.Initialize();

            List<T> firebaseItems;
            try
            {
                firebaseItems = await _firebaseRepository.Initialize();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HybridRepository] Firebase 로드 실패, 로컬만 사용: {ex.Message}");
                return localItems;
            }

            return MergeByTimestamp(localItems, firebaseItems);
        }

        public void Save(T item)
        {
            item.LastModified = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + _timeOffset;
            _pendingItems[item.Id] = item;
            RestartDebounceTimer();
        }

        public void ForceFlush()
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
                // 타이머 취소됨 - 정상
            }
        }

        private async UniTask FlushPending(bool forceFirebase)
        {
            if (_pendingItems.Count > 0)
            {
                var snapshot = _pendingItems;
                _pendingItems = new Dictionary<string, T>();

                foreach (var item in snapshot.Values)
                {
                    _localRepository.Save(item);
                }

                _localSaveCount++;
                Debug.Log($"[HybridRepository] 로컬 저장 완료: {snapshot.Count}건 (배치 #{_localSaveCount})");
            }

            bool shouldSyncFirebase = forceFirebase || _localSaveCount >= SaveCountThreshold;

            if (shouldSyncFirebase && !_isSyncInProgress)
            {
                await FlushToFirebase();
            }
        }

        private async UniTask FlushToFirebase()
        {
            _isSyncInProgress = true;
            _localSaveCount = 0;

            try
            {
                var localItems = await _localRepository.Initialize();

                foreach (var item in localItems)
                {
                    _firebaseRepository.Save(item);
                }

                Debug.Log($"[HybridRepository] Firebase 동기화 완료: {localItems.Count}건");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HybridRepository] Firebase 동기화 실패: {ex.Message}");
            }
            finally
            {
                _isSyncInProgress = false;
            }
        }

        private List<T> MergeByTimestamp(List<T> local, List<T> firebase)
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

            var result = merged.Values.ToList();

            foreach (var item in result)
                _localRepository.Save(item);

            return result;
        }
    }
}
