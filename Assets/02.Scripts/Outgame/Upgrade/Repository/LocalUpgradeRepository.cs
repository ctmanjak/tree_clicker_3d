using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Outgame
{
    public class LocalUpgradeRepository : IUpgradeRepository
    {
        private readonly Dictionary<string, UpgradeSaveData> _data = new();

        private string SavePath => Path.Combine(Application.persistentDataPath, "upgrade_save.json");

        public UniTask<List<UpgradeSaveData>> Initialize()
        {
            LoadFromFile();
            return UniTask.FromResult(new List<UpgradeSaveData>(_data.Values));
        }

        public void Save(UpgradeSaveData item)
        {
            _data[item.Id] = item;
            SaveToFile();
        }

        private void SaveToFile()
        {
            var collection = new SaveDataCollection();
            collection.Entries.AddRange(_data.Values);
            string json = JsonUtility.ToJson(collection, true);
            File.WriteAllText(SavePath, json);
        }

        private void LoadFromFile()
        {
            if (!File.Exists(SavePath)) return;

            try
            {
                string json = File.ReadAllText(SavePath);
                var saveData = JsonUtility.FromJson<SaveDataCollection>(json);

                if (saveData?.Entries == null) return;

                foreach (var entry in saveData.Entries)
                {
                    _data[entry.Id] = entry;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load upgrade data: {e.Message}");
            }
        }

        [Serializable]
        private class SaveDataCollection
        {
            public List<UpgradeSaveData> Entries = new();
        }
    }
}
