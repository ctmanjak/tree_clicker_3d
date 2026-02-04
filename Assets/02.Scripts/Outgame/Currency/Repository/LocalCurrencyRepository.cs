using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Outgame
{
    public class LocalCurrencyRepository : ICurrencyRepository
    {
        private const string SaveKey = "CurrencyData";

        private readonly Dictionary<string, CurrencySaveData> _data = new();

        public UniTask<List<CurrencySaveData>> Initialize()
        {
            LoadFromPlayerPrefs();
            InitializeDefaults();
            return UniTask.FromResult(new List<CurrencySaveData>(_data.Values));
        }

        private void InitializeDefaults()
        {
            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                string key = type.ToString();
                if (!_data.ContainsKey(key))
                    _data[key] = new CurrencySaveData { Id = key, Type = key, Amount = 0 };
            }
        }

        public void Save(CurrencySaveData item)
        {
            _data[item.Id] = item;
            SaveToPlayerPrefs();
        }

        private void SaveToPlayerPrefs()
        {
            var collection = new SaveDataCollection();
            collection.Items.AddRange(_data.Values);
            string json = JsonUtility.ToJson(collection);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        private void LoadFromPlayerPrefs()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;

            string json = PlayerPrefs.GetString(SaveKey);
            var saveData = JsonUtility.FromJson<SaveDataCollection>(json);

            if (saveData?.Items == null) return;

            foreach (var item in saveData.Items)
            {
                _data[item.Id] = item;
            }
        }

        [Serializable]
        private class SaveDataCollection
        {
            public List<CurrencySaveData> Items = new();
        }
    }
}
