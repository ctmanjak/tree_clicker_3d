using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalUpgradeRepository : IUpgradeRepository
{
    private readonly Dictionary<string, int> _levels = new();

    private string SavePath => Path.Combine(Application.persistentDataPath, "upgrade_save.json");

    public LocalUpgradeRepository()
    {
        LoadFromFile();
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
            saveData.Ids.Add(kvp.Key);
            saveData.Levels.Add(kvp.Value);
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

            if (saveData?.Ids == null) return;

            for (int i = 0; i < saveData.Ids.Count; i++)
            {
                _levels[saveData.Ids[i]] = saveData.Levels[i];
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load upgrade data: {e.Message}");
        }
    }

    [Serializable]
    private class UpgradeSaveData
    {
        public List<string> Ids = new();
        public List<int> Levels = new();
    }
}
