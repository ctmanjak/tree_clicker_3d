using UnityEngine;

[CreateAssetMenu(fileName = "SFXConfig", menuName = "LumberTycoon/Audio/SFX Config")]
public class SFXConfig : ScriptableObject
{
    [System.Serializable]
    public class SFXEntry
    {
        public string Id;
        public AudioClip[] Clips;
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0.8f, 1.2f)] public float PitchMin = 0.95f;
        [Range(0.8f, 1.2f)] public float PitchMax = 1.05f;
    }

    [SerializeField] private SFXEntry[] _entries;

    public SFXEntry GetEntry(string id)
    {
        foreach (var entry in _entries)
        {
            if (entry.Id == id) return entry;
        }
        return null;
    }

    public AudioClip GetRandomClip(string id)
    {
        var entry = GetEntry(id);
        if (entry == null || entry.Clips == null || entry.Clips.Length == 0)
            return null;

        return entry.Clips[Random.Range(0, entry.Clips.Length)];
    }
}
