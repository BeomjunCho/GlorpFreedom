using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController2D playerController;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private CheckPointManager checkpointManager;

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float firstDelay = 7f;
    [SerializeField] private float secondDelay = 7f;

    private SpriteRenderer playerSpriteRenderer;
    private Rigidbody2D rb2d;

    private void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("GameManager: PlayerController2D reference is not assigned.");
            return;
        }

        if (enemyManager == null)
        {
            Debug.LogError("GameManager: EnemyManager reference is not assigned.");
            return;
        }

        if (checkpointManager == null)
        {
            Debug.LogError("GameManager: CheckPointManager reference is not assigned.");
            return;
        }

        // Cache the player's SpriteRenderer so we can hide/show it
        playerSpriteRenderer = playerController.GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer == null)
        {
            Debug.LogError("GameManager: PlayerController2D does not have a SpriteRenderer component.");
            return;
        }

        rb2d = playerController.GetComponent<Rigidbody2D>();

        if (rb2d == null)
        {
            Debug.LogError("GameManager: PlayerController2D does not have a rb2d component.");
            return;
        }

        // Initially hide the player sprite so it is not visible until the second fade-in
        playerSpriteRenderer.enabled = false;
        rb2d.gravityScale = 0f;

        enemyManager.Initialize(playerController);
        checkpointManager.Initialize(playerController.gameObject);

        Debug.Log("[GameManager] Initialization complete: EnemyManager and CheckPointManager are set up.");

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(HandleFadeSequence());
        }
    }

    private IEnumerator HandleFadeSequence()
    {
        // Ensure fadeImage starts fully opaque (black screen)
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        // Fade from opaque to transparent over firstDelay seconds (player remains hidden)
        yield return StartCoroutine(FadeToAlpha(0f, firstDelay));

        // Immediately set fadeImage back to fully opaque (black)
        color.a = 1f;
        fadeImage.color = color;

        // Wait for secondDelay seconds while screen is fully black (player still hidden)
        yield return new WaitForSeconds(secondDelay);

        // Enable the player sprite just before the second fade-in begins
        playerSpriteRenderer.enabled = true;
        rb2d.gravityScale = 1f;

        // Fade from opaque to transparent over fadeDuration seconds (player now revealed)
        yield return StartCoroutine(FadeToAlpha(0f, fadeDuration));

        // Once done fading, disable the fadeImage so it no longer blocks the view
        fadeImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color endColor = startColor;
        endColor.a = targetAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        // Ensure final alpha is exactly targetAlpha
        Color finalColor = fadeImage.color;
        finalColor.a = targetAlpha;
        fadeImage.color = finalColor;
    }
}
