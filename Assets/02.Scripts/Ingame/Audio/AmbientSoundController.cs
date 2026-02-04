using System.Collections;
using Core;
using UnityEngine;

namespace Ingame
{
    public class AmbientSoundController : MonoBehaviour
    {
        private const float DefaultFadeTime = 1f;

        [Header("Config")]
        [SerializeField] private AudioClip _defaultAmbient;
        [SerializeField] private bool _playOnStart = true;

        [Header("Volume")]
        [Range(0f, 1f)][SerializeField] private float _volume = 0.5f;

        private AudioSource _source;
        private Coroutine _fadeCoroutine;
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
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.priority = 128;
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _audioManager);

            if (_playOnStart && _defaultAmbient != null)
            {
                Play(_defaultAmbient);
            }
        }

        public void Play(AudioClip clip, float fadeTime = DefaultFadeTime)
        {
            if (clip == null) return;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _source.clip = clip;
            _source.volume = 0f;
            _source.Play();

            _fadeCoroutine = StartCoroutine(FadeInCoroutine(fadeTime));
        }

        public void Stop(float fadeTime = DefaultFadeTime)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeOutCoroutine(fadeTime));
        }

        public void SetVolume(float volume)
        {
            Volume = volume;
        }

        private IEnumerator FadeInCoroutine(float duration)
        {
            float elapsed = 0f;
            float targetVolume = GetTargetVolume();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            _source.volume = targetVolume;
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutCoroutine(float duration)
        {
            float elapsed = 0f;
            float startVolume = _source.volume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            _source.Stop();
            _source.volume = 0f;
            _fadeCoroutine = null;
        }

        private float GetTargetVolume()
        {
            float masterVolume = _audioManager != null ? _audioManager.MasterVolume : 1f;
            return _volume * masterVolume;
        }

        private void UpdateVolume()
        {
            if (_source != null && _source.isPlaying)
            {
                _source.volume = GetTargetVolume();
            }
        }
    }
}
