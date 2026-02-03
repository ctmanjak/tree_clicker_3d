using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FirebaseUpgradeRepository : IUpgradeRepository
{
    private const string CollectionName = "upgrades";

    private readonly IFirebaseStoreService _storeService;

    public FirebaseUpgradeRepository(IFirebaseStoreService storeService)
    {
        _storeService = storeService;
    }

    public async UniTask<List<UpgradeSaveData>> Initialize()
    {
        var documents = await _storeService.GetCollection<UpgradeSaveData>(CollectionName);
        Debug.Log($"Firestore에서 업그레이드 데이터 로드 완료 ({documents.Count}건)");
        return documents;
    }

    public void Save(UpgradeSaveData item)
    {
        _storeService.SetDocumentAsync(CollectionName, item);
    }
}
