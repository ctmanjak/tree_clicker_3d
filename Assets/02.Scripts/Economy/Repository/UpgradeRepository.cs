using System.Collections.Generic;
using UnityEngine;

public class UpgradeRepository : MonoBehaviour, IUpgradeRepository, ISaveable
{
    private readonly Dictionary<string, UpgradeState> _states = new();

    public string SaveKey => "UpgradeRepository";

    private void Awake()
    {
        ServiceLocator.Register<IUpgradeRepository>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<IUpgradeRepository>(this);
    }

    public UpgradeState GetState(string upgradeId)
    {
        if (!_states.TryGetValue(upgradeId, out var state))
        {
            state = new UpgradeState(upgradeId);
            _states[upgradeId] = state;
        }
        return state;
    }

    public void SaveState(UpgradeState state)
    {
        _states[state.UpgradeId] = state;
    }

    public IEnumerable<UpgradeState> GetAllStates()
    {
        return _states.Values;
    }

    public object CaptureState()
    {
        var saveData = new Dictionary<string, int>();
        foreach (var kvp in _states)
        {
            saveData[kvp.Key] = kvp.Value.Level;
        }
        return saveData;
    }

    public void RestoreState(object state)
    {
        if (state is Dictionary<string, int> savedLevels)
        {
            _states.Clear();
            foreach (var kvp in savedLevels)
            {
                _states[kvp.Key] = new UpgradeState(kvp.Key, kvp.Value);
            }
        }
    }
}
