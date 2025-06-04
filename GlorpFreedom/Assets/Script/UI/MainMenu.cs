using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _howToPlay;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip startGameClip;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private float _initialAmbientVolume;

    private void Start()
    {
        if (ambientSource != null && ambientSource.clip != null)
        {
            ambientSource.loop = true;
            ambientSource.Play();
            _initialAmbientVolume = ambientSource.volume;
        }
    }

    public void ButtonStartGame()
    {
        if (sfxSource == null || startGameClip == null)
        {
            SceneManager.LoadScene("InGame");
            return;
        }

        sfxSource.PlayOneShot(startGameClip);

        if (ambientSource != null)
        {
            StartCoroutine(FadeOutAmbient());
        }

        StartCoroutine(DelayedLoadInGame(startGameClip.length));
    }

    public void ButtonHowToPlay()
    {
        if (_howToPlay == null)
        {
            Debug.LogWarning("HowToPlay GameObject is not assigned.");
            return;
        }

        if (sfxSource != null && clickClip != null)
        {
            sfxSource.PlayOneShot(clickClip);
        }

        bool isActive = _howToPlay.activeSelf;
        _howToPlay.SetActive(!isActive);
    }

    public void QuitGame()
    {
        if (sfxSource != null && clickClip != null)
        {
            sfxSource.PlayOneShot(clickClip);
        }

        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Quit game!");
    }

    private IEnumerator FadeOutAmbient()
    {
        float elapsed = 0f;
        float startVolume = ambientSource.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        ambientSource.volume = 0f;
        ambientSource.Stop();
    }

    private IEnumerator DelayedLoadInGame(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("InGame");
    }

    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverClip != null)
        {
            sfxSource.PlayOneShot(hoverClip);
        }
    }
}
