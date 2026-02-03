using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LocalUpgradeRepository : IUpgradeRepository
{
    private readonly Dictionary<string, int> _levels = new();

    private string SavePath => Path.Combine(Application.persistentDataPath, "upgrade_save.json");

    public UniTask Initialize()
    {
        LoadFromFile();
        return UniTask.CompletedTask;
    }

    public int GetLevel(string upgradeId)
    {
        return _levels.TryGetValue(upgradeId, out int level) ? level : 0;
    }

    public void SetLevel(string upgradeId, int level)
    {
        _levels[upgradeId] = level;
        Save();
    }

    public void Save()
    {
        var saveData = new UpgradeSaveData();
        foreach (var kvp in _levels)
        {
            saveData.Entries.Add(new UpgradeEntry { Id = kvp.Key, Level = kvp.Value });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
    }

    private void LoadFromFile()
    {
        if (!File.Exists(SavePath)) return;

        try
        {
            string json = File.ReadAllText(SavePath);
            var saveData = JsonUtility.FromJson<UpgradeSaveData>(json);

            if (saveData?.Entries == null) return;

            foreach (var entry in saveData.Entries)
            {
                _levels[entry.Id] = entry.Level;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load upgrade data: {e.Message}");
        }
    }

    [Serializable]
    private class UpgradeEntry
    {
        public string Id;
        public int Level;
    }

    [Serializable]
    private class UpgradeSaveData
    {
        public List<UpgradeEntry> Entries = new();
    }
}
