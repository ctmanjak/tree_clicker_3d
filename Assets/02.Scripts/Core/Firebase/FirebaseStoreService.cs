using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

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
        string uid = _authService.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("Firestore 저장 실패: 로그인되지 않은 상태");
            return;
        }

        var docRef = _firestore.Collection("users").Document(uid)
            .Collection(collection).Document(data.Id);
        await docRef.SetAsync(data);
    }

    public async UniTask<List<T>> GetCollection<T>(string collection) where T : IIdentifiable
    {
        string uid = _authService.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
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
        string uid = _authService.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("Firestore 저장 실패: 로그인되지 않은 상태");
            return;
        }

        var docRef = _firestore.Collection("users").Document(uid)
            .Collection(collection).Document(data.Id);
        docRef.SetAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogWarning($"Firestore 백그라운드 저장 실패: {task.Exception?.Message}");
            }
        });
    }
}
