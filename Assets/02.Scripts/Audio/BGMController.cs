using System.Collections;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    private const float DEFAULT_FADE_TIME = 0.5f;

    [Header("Config")]
    [SerializeField] private AudioClip _defaultBGM;
    [SerializeField] private bool _playOnStart = true;

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float _volume = 1f;

    private AudioSource _sourceA;
    private AudioSource _sourceB;
    private AudioSource _currentSource;
    private Coroutine _fadeCoroutine;
    private bool _isPaused;
    private AudioManager _audioManager;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Mathf.Clamp01(value);
            UpdateVolume();
        }
    }

    private void Awake()
    {
        _sourceA = CreateAudioSource("BGM_A");
        _sourceB = CreateAudioSource("BGM_B");
        _currentSource = _sourceA;
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _audioManager);

        if (_playOnStart && _defaultBGM != null)
        {
            Play(_defaultBGM);
        }
    }

    private AudioSource CreateAudioSource(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.priority = 0;
        return source;
    }

    public void Play(AudioClip clip, float fadeTime = DEFAULT_FADE_TIME)
    {
        if (clip == null) return;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        var nextSource = _currentSource == _sourceA ? _sourceB : _sourceA;
        nextSource.clip = clip;
        nextSource.volume = 0f;
        nextSource.Play();

        _fadeCoroutine = StartCoroutine(CrossfadeCoroutine(_currentSource, nextSource, fadeTime));
        _currentSource = nextSource;
        _isPaused = false;
    }

    public void Stop(float fadeTime = DEFAULT_FADE_TIME)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeOutCoroutine(_currentSource, fadeTime));
    }

    public void Pause()
    {
        if (_currentSource.isPlaying)
        {
            _currentSource.Pause();
            _isPaused = true;
        }
    }

    public void Resume()
    {
        if (_isPaused)
        {
            _currentSource.UnPause();
            _isPaused = false;
        }
    }

    private IEnumerator CrossfadeCoroutine(AudioSource fromSource, AudioSource toSource, float duration)
    {
        float elapsed = 0f;
        float startVolume = fromSource.volume;
        float targetVolume = GetTargetVolume();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            fromSource.volume = Mathf.Lerp(startVolume, 0f, t);
            toSource.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        fromSource.Stop();
        fromSource.volume = 0f;
        toSource.volume = targetVolume;
        _fadeCoroutine = null;
    }

    private IEnumerator FadeOutCoroutine(AudioSource source, float duration)
    {
        float elapsed = 0f;
        float startVolume = source.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.Stop();
        source.volume = 0f;
        _fadeCoroutine = null;
    }

    private float GetTargetVolume()
    {
        float masterVolume = _audioManager != null ? _audioManager.MasterVolume : 1f;
        float bgmVolume = _audioManager != null ? _audioManager.BGMVolume : 1f;
        return _volume * bgmVolume * masterVolume;
    }

    private void UpdateVolume()
    {
        if (_currentSource != null && _currentSource.isPlaying && !_isPaused)
        {
            _currentSource.volume = GetTargetVolume();
        }
    }
}
