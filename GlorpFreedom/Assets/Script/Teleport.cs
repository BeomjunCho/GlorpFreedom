using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Teleport : MonoBehaviour
{
    [Tooltip("The destination Transform where the player will teleport after the delay.")]
    [SerializeField] private Transform targetLocation;

    [Tooltip("Time in seconds to wait after the player enters the trigger before starting the teleport.")]
    public float delayTime = 0.5f;

    [Tooltip("Duration in seconds over which the player’s position will interpolate (smoothly move) to the target.")]
    public float teleportDuration = 0f;

    [SerializeField] private AudioSource _teleportAudioSource;

    // Prevent multiple overlapping teleports if the player stays inside the trigger
    private bool isTeleporting = false;

    private void Awake()
    {
        // Ensure that this Collider2D is configured as a trigger.
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[Teleport] '{name}' Collider2D is not set to Trigger. Enabling isTrigger automatically.");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react if the collider belongs to the player (tagged "Player") and no teleport is already in progress.
        if (!other.CompareTag("Player") || isTeleporting)
            return;

        if (targetLocation == null)
        {
            Debug.LogError($"[Teleport] '{name}' has no targetLocation assigned! Cannot perform teleport.");
            return;
        }

        // Begin the teleport sequence.
        StartCoroutine(TeleportCoroutine(other.transform, other.GetComponent<Rigidbody2D>(), other.GetComponent<Collider2D>()));
    }

    private IEnumerator TeleportCoroutine(Transform playerTransform, Rigidbody2D playerRb, Collider2D playerCollider)
    {
        isTeleporting = true;

        _teleportAudioSource.Play();

        // 1) Wait for the initial delay before teleporting.
        yield return new WaitForSeconds(delayTime);

        // 2) Disable the player's collider to avoid unwanted collisions during teleport.
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // 3) If teleportDuration > 0, smoothly move the player from current position to the target over teleportDuration seconds.
        if (teleportDuration > 0f)
        {
            Vector3 startPosition = playerTransform.position;
            Vector3 endPosition = targetLocation.position;
            float elapsed = 0f;

            while (elapsed < teleportDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / teleportDuration);
                playerTransform.position = Vector3.Lerp(startPosition, endPosition, t);
                yield return null;
            }
        }
        else
        {
            // Instant teleport if duration is zero.
            playerTransform.position = targetLocation.position;
        }

        // 4) Reset the Rigidbody2D velocity to zero to prevent any residual movement.
        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
        }

        // 5) Re-enable the player's collider now that teleport is complete.
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        Debug.Log($"[Teleport] Player has been teleported to {targetLocation.position}.");

        // Allow future teleport triggers once this sequence is finished.
        isTeleporting = false;
    }
}
