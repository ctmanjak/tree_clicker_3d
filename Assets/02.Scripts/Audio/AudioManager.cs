using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const int SFX_POOL_SIZE = 10;

    [Header("Config")]
    [SerializeField] private SFXConfig _sfxConfig;

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float _masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float _sfxVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float _bgmVolume = 1f;

    private readonly Queue<AudioSource> _sfxPool = new();
    private readonly List<AudioSource> _activeSfx = new();

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = Mathf.Clamp01(value);
            UpdateAllVolumes();
        }
    }

    public float SFXVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            UpdateAllVolumes();
        }
    }

    public float BGMVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = Mathf.Clamp01(value);
            UpdateAllVolumes();
        }
    }

    private void Awake()
    {
        ServiceLocator.Register(this);
        InitializeSFXPool();
    }

    private void InitializeSFXPool()
    {
        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            CreatePooledAudioSource();
        }
    }

    private AudioSource CreatePooledAudioSource()
    {
        var go = new GameObject("SFX_Source");
        go.transform.SetParent(transform);
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        go.SetActive(false);
        _sfxPool.Enqueue(source);
        return source;
    }

    public void PlaySFX(string sfxId)
    {
        if (_sfxConfig == null) return;

        var entry = _sfxConfig.GetEntry(sfxId);
        if (entry == null) return;

        var clip = _sfxConfig.GetRandomClip(sfxId);
        if (clip == null) return;

        PlaySFXInternal(clip, entry.Volume, entry.PitchMin, entry.PitchMax);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        PlaySFXInternal(clip, volume, 0.95f, 1.05f);
    }

    public void PlaySFXAtPosition(string sfxId, Vector3 position)
    {
        if (_sfxConfig == null) return;

        var entry = _sfxConfig.GetEntry(sfxId);
        if (entry == null) return;

        var clip = _sfxConfig.GetRandomClip(sfxId);
        if (clip == null) return;

        PlaySFXInternal(clip, entry.Volume, entry.PitchMin, entry.PitchMax, position);
    }

    private void PlaySFXInternal(AudioClip clip, float volume, float pitchMin, float pitchMax, Vector3? position = null)
    {
        var source = GetPooledSource();
        if (source == null) return;

        source.clip = clip;
        source.volume = volume * _sfxVolume * _masterVolume;
        source.pitch = Random.Range(pitchMin, pitchMax);
        source.spatialBlend = position.HasValue ? 1f : 0f;

        if (position.HasValue)
        {
            source.transform.position = position.Value;
        }

        source.gameObject.SetActive(true);
        source.Play();
        _activeSfx.Add(source);
    }

    private AudioSource GetPooledSource()
    {
        if (_sfxPool.Count > 0)
        {
            return _sfxPool.Dequeue();
        }
        return CreatePooledAudioSource();
    }

    private void Update()
    {
        for (int i = _activeSfx.Count - 1; i >= 0; i--)
        {
            var source = _activeSfx[i];
            if (!source.isPlaying)
            {
                source.gameObject.SetActive(false);
                _activeSfx.RemoveAt(i);
                _sfxPool.Enqueue(source);
            }
        }
    }

    private void UpdateAllVolumes()
    {
        foreach (var source in _activeSfx)
        {
            if (source.clip != null)
            {
                source.volume = _sfxVolume * _masterVolume;
            }
        }
    }
}
