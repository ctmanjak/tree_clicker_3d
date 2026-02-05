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

            var docRef = GetDocumentReference(uid, collection, data.Id);
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

            var docRef = GetDocumentReference(uid, collection, data.Id);
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

        private DocumentReference GetDocumentReference(string uid, string collection, string documentId)
        {
            return _firestore.Collection("users").Document(uid)
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
    }
}
