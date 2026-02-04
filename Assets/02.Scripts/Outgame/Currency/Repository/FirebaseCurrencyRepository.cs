using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Outgame
{
    public class FirebaseCurrencyRepository : ICurrencyRepository
    {
        private const string CollectionName = "currencies";

        private readonly IFirebaseStoreService _storeService;

        public FirebaseCurrencyRepository(IFirebaseStoreService storeService)
        {
            _storeService = storeService;
        }

        public async UniTask<List<CurrencySaveData>> Initialize()
        {
            var result = new List<CurrencySaveData>();
            var documents = await _storeService.GetCollection<CurrencySaveData>(CollectionName);

            foreach (var data in documents)
            {
                if (!Enum.TryParse<CurrencyType>(data.Id, out _))
                    continue;

                result.Add(data);
            }

            Debug.Log($"Firestore에서 재화 데이터 로드 완료 ({result.Count}건)");
            return result;
        }

        public void Save(CurrencySaveData item)
        {
            _storeService.SetDocumentAsync(CollectionName, item);
        }
    }
}
