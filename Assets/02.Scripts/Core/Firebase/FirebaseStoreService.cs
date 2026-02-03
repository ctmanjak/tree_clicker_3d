using System;
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

    public async UniTask SetDocument(string collection, string documentId, Dictionary<string, object> data)
    {
        string uid = _authService.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("Firestore 저장 실패: 로그인되지 않은 상태");
            return;
        }

        var docRef = _firestore.Collection("users").Document(uid)
            .Collection(collection).Document(documentId);
        await docRef.SetAsync(data);
    }

    public async UniTask<Dictionary<string, object>> GetDocument(string collection, string documentId)
    {
        string uid = _authService.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return null;

        var docRef = _firestore.Collection("users").Document(uid)
            .Collection(collection).Document(documentId);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
            return null;

        return snapshot.ToDictionary();
    }

    public async UniTask<List<Dictionary<string, object>>> GetCollection(string collection)
    {
        string uid = _authService.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            return new List<Dictionary<string, object>>();

        var collectionRef = _firestore.Collection("users").Document(uid)
            .Collection(collection);
        var snapshot = await collectionRef.GetSnapshotAsync();

        var results = new List<Dictionary<string, object>>();
        foreach (var doc in snapshot.Documents)
        {
            var data = doc.ToDictionary();
            data["_documentId"] = doc.Id;
            results.Add(data);
        }
        return results;
    }

    public void SetDocumentAsync(string collection, string documentId, Dictionary<string, object> data)
    {
        string uid = _authService.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("Firestore 저장 실패: 로그인되지 않은 상태");
            return;
        }

        var docRef = _firestore.Collection("users").Document(uid)
            .Collection(collection).Document(documentId);
        docRef.SetAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogWarning($"Firestore 백그라운드 저장 실패: {task.Exception?.Message}");
            }
        });
    }
}
