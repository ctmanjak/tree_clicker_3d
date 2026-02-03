using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FirebaseUpgradeRepository : IUpgradeRepository
{
    private const string CollectionName = "upgrades";

    private readonly IFirebaseStoreService _storeService;
    private readonly Dictionary<string, int> _levels = new();

    public FirebaseUpgradeRepository(IFirebaseStoreService storeService)
    {
        _storeService = storeService;
    }

    public async UniTask Initialize()
    {
        _levels.Clear();

        var documents = await _storeService.GetCollection(CollectionName);

        foreach (var doc in documents)
        {
            if (!doc.TryGetValue("_documentId", out var docIdObj))
                continue;

            string upgradeId = docIdObj.ToString();

            if (!doc.TryGetValue("level", out var levelObj))
                continue;

            int level = Convert.ToInt32(levelObj);
            _levels[upgradeId] = level;
        }

        Debug.Log($"Firestore에서 업그레이드 데이터 로드 완료 ({documents.Count}건)");
    }

    public int GetLevel(string upgradeId)
    {
        return _levels.TryGetValue(upgradeId, out int level) ? level : 0;
    }

    public void SetLevel(string upgradeId, int level)
    {
        _levels[upgradeId] = level;

        var data = new Dictionary<string, object>
        {
            { "level", level }
        };
        _storeService.SetDocumentAsync(CollectionName, upgradeId, data);
    }

    public void Save()
    {
        foreach (var kvp in _levels)
        {
            var data = new Dictionary<string, object>
            {
                { "level", kvp.Value }
            };
            _storeService.SetDocumentAsync(CollectionName, kvp.Key, data);
        }
    }
}
