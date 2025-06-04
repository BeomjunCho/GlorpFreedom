using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("Audio clip to play on scene load (intro/cutscene).")]
    [SerializeField] private AudioClip introClip;

    [Tooltip("Ambient audio clip to play after delay.")]
    [SerializeField] private AudioClip ambientClip;

    [Tooltip("Music audio clip to play after delay.")]
    [SerializeField] private AudioClip musicClip;

    [Header("Settings")]
    [Tooltip("Delay in seconds before playing the ambient and music audio.")]
    [SerializeField] private float delayBeforeAmbientAndMusic = 12f;

    [Header("AudioSource components")]
    [SerializeField] private AudioSource _introSource;
    [SerializeField] private AudioSource _ambSource;
    [SerializeField] private AudioSource _musicSource;

    private void Start()
    {
        if (_introSource == null)
        {
            Debug.LogError("AudioManager: _introSource is not assigned.");
            return;
        }
        if (_ambSource == null)
        {
            Debug.LogError("AudioManager: _ambSource is not assigned.");
            return;
        }
        if (_musicSource == null)
        {
            Debug.LogError("AudioManager: _musicSource is not assigned.");
            return;
        }

        // Assign and play intro clip immediately
        if (introClip != null)
        {
            _introSource.clip = introClip;
            _introSource.volume = 1f;
            _introSource.loop = false;
            _introSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioManager: Intro clip is not assigned.");
        }

        // Schedule ambient and music to start after the specified delay
        StartCoroutine(PlayAmbientAndMusicAfterDelay());
    }

    private IEnumerator PlayAmbientAndMusicAfterDelay()
    {
        // Wait for the delay before starting ambient and music audio
        yield return new WaitForSeconds(delayBeforeAmbientAndMusic);

        // Play ambient with fade-in
        if (ambientClip != null)
        {
            _ambSource.clip = ambientClip;
            _ambSource.loop = true;
            StartCoroutine(FadeInAudio(_ambSource, 2f, 0.25f));
            _ambSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioManager: Ambient clip is not assigned.");
        }

        // Play music with fade-in
        if (musicClip != null)
        {
            _musicSource.clip = musicClip;
            _musicSource.loop = true;
            StartCoroutine(FadeInAudio(_musicSource, 2f, 0.15f));
            _musicSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioManager: Music clip is not assigned.");
        }
    }

    private IEnumerator FadeInAudio(AudioSource source, float duration, float targetVol)
    {
        source.volume = 0f;     // Start muted
        float elapsed = 0f;
        float targetVolume = targetVol; // Fade to full volume

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        // Ensure volume reaches exactly targetVolume
        source.volume = targetVolume;
    }
}
