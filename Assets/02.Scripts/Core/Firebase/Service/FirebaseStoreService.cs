using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

namespace Core
{
    public class FirebaseStoreService : IFirebaseStoreService
    {
        private FirebaseFirestore _firestore;
        private readonly IFirebaseAuthService _authService;

        private const int MAX_DOCUMENT_ID_BYTES = 1500;

        public bool IsInitialized => _firestore != null;

        public FirebaseStoreService(IFirebaseAuthService authService)
        {
            _authService = authService;
        }

        public UniTask Initialize()
        {
            _firestore = FirebaseFirestore.DefaultInstance;
            return UniTask.CompletedTask;
        }

        public async UniTask SetDocument<T>(string collection, T data) where T : IIdentifiable
        {
            if (!TryGetUserId(out string uid))
                return;

            if (!TryValidateDocumentId(data.Id, out string error))
            {
                Debug.LogWarning($"[FirebaseStoreService] SetDocument 실패: {error}");
                return;
            }

            var docRef = GetDocumentReference(_firestore, uid, collection, data.Id);
            await docRef.SetAsync(data);
        }

        public async UniTask<List<T>> GetCollection<T>(string collection) where T : IIdentifiable
        {
            if (!TryGetUserId(out string uid))
                return new List<T>();

            var collectionRef = _firestore.Collection("users").Document(uid)
                .Collection(collection);
            var snapshot = await collectionRef.GetSnapshotAsync();

            var results = new List<T>();
            foreach (var doc in snapshot.Documents)
            {
                results.Add(doc.ConvertTo<T>());
            }
            return results;
        }

        public void SetDocumentAsync<T>(string collection, T data) where T : IIdentifiable
        {
            if (!TryGetUserId(out string uid))
                return;

            if (!TryValidateDocumentId(data.Id, out string error))
            {
                Debug.LogWarning($"[FirebaseStoreService] SetDocumentAsync 실패: {error}");
                return;
            }

            var docRef = GetDocumentReference(_firestore, uid, collection, data.Id);
            docRef.SetAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogWarning($"Firestore 백그라운드 저장 실패: {task.Exception?.Message}");
                }
            });
        }

        private bool TryGetUserId(out string uid)
        {
            uid = _authService.CurrentUserId;
            if (string.IsNullOrEmpty(uid))
            {
                Debug.LogWarning("Firestore 저장 실패: 로그인되지 않은 상태");
                return false;
            }
            return true;
        }

        private static bool TryValidateDocumentId(string documentId, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(documentId))
            {
                errorMessage = "문서 ID가 비어있습니다";
                return false;
            }

            if (documentId.Contains("/") || documentId.Contains("\\"))
            {
                errorMessage = $"문서 ID에 경로 구분자가 포함되어 있습니다: {documentId}";
                return false;
            }

            if (documentId.Contains(".."))
            {
                errorMessage = $"문서 ID에 상위 경로 참조가 포함되어 있습니다: {documentId}";
                return false;
            }

            if (System.Text.Encoding.UTF8.GetByteCount(documentId) > MAX_DOCUMENT_ID_BYTES)
            {
                errorMessage = $"문서 ID가 최대 길이({MAX_DOCUMENT_ID_BYTES}바이트)를 초과합니다";
                return false;
            }

            return true;
        }

        private static DocumentReference GetDocumentReference(
            FirebaseFirestore firestore, string uid, string collection, string documentId)
        {
            return firestore.Collection("users").Document(uid)
                .Collection(collection).Document(documentId);
        }

        public async UniTask<long> GetServerTimeAsync()
        {
            if (!TryGetUserId(out string uid))
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                var docRef = _firestore.Collection("users").Document(uid)
                    .Collection("_metadata").Document("serverTime");

                var data = new Dictionary<string, object>
                {
                    { "timestamp", FieldValue.ServerTimestamp }
                };
                await docRef.SetAsync(data);

                var snapshot = await docRef.GetSnapshotAsync();
                var timestamp = snapshot.GetValue<Timestamp>("timestamp");

                return timestamp.ToDateTimeOffset().ToUnixTimeSeconds();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FirebaseStoreService] 서버 시간 가져오기 실패, 로컬 시간 사용: {ex.Message}");
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }

        public IWriteBatchWrapper CreateWriteBatch()
        {
            if (!TryGetUserId(out string uid))
                return new NoOpWriteBatchWrapper();
            return new FirestoreWriteBatchWrapper(_firestore, uid);
        }

        private class FirestoreWriteBatchWrapper : IWriteBatchWrapper
        {
            private readonly WriteBatch _batch;
            private readonly FirebaseFirestore _firestore;
            private readonly string _uid;

            public FirestoreWriteBatchWrapper(FirebaseFirestore firestore, string uid)
            {
                _firestore = firestore;
                _uid = uid;
                _batch = firestore.StartBatch();
            }

            public void Set<T>(string collection, string documentId, T data)
            {
                if (!TryValidateDocumentId(documentId, out string error))
                {
                    Debug.LogWarning($"[FirestoreWriteBatchWrapper] Set 실패: {error}");
                    return;
                }

                var docRef = GetDocumentReference(_firestore, _uid, collection, documentId);
                _batch.Set(docRef, data);
            }

            public async UniTask CommitAsync()
            {
                await _batch.CommitAsync();
            }
        }

        private class NoOpWriteBatchWrapper : IWriteBatchWrapper
        {
            public void Set<T>(string collection, string documentId, T data)
            {
                Debug.LogWarning("[NoOpWriteBatchWrapper] 로그인되지 않아 WriteBatch 무시됨");
            }

            public UniTask CommitAsync()
            {
                return UniTask.CompletedTask;
            }
        }
    }
}
