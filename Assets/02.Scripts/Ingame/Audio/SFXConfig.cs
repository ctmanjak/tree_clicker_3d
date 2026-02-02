using UnityEngine;

[CreateAssetMenu(fileName = "SFXConfig", menuName = "Lumberman/Audio/SFX Config")]
public class SFXConfig : ScriptableObject
{
    [System.Serializable]
    public class SFXEntry
    {
        public SFXType Type;
        public AudioClip[] Clips;
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0.8f, 1.2f)] public float PitchMin = 0.95f;
        [Range(0.8f, 1.2f)] public float PitchMax = 1.05f;
    }

    [SerializeField] private SFXEntry[] _entries;

    public SFXEntry GetEntry(SFXType type)
    {
        foreach (var entry in _entries)
        {
            if (entry.Type == type) return entry;
        }
        return null;
    }

    public AudioClip GetRandomClip(SFXType type)
    {
        var entry = GetEntry(type);
        if (entry == null || entry.Clips == null || entry.Clips.Length == 0)
            return null;

        return entry.Clips[Random.Range(0, entry.Clips.Length)];
    }
}
