using System.Collections;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    // Reference to the player GameObject (set via Initialize)
    private GameObject _player;

    // Last saved respawn position
    private Vector3 _lastRespawnPosition;
    private bool _hasCheckpoint = false;

    // Inspector-editable respawn delay (in seconds)
    [Tooltip("Time in seconds to wait before respawning the player after death.")]
    [SerializeField] private float respawnDelay = 1f;

    // Cached player components
    private PlayerHealth _playerHealth;
    private Rigidbody2D _playerRb2D;
    private Transform _playerTransform;

    // All checkpoint trigger volumes in the scene
    public CheckpointTriggerVolume[] allTriggers;

    /// <summary>
    /// Initializes the checkpoint manager with a reference to the player.
    /// Also finds all CheckpointTriggerVolume instances in the scene and assigns this manager to them.
    /// </summary>
    /// <param name="playerRef">The player GameObject that contains PlayerHealth.</param>
    public void Initialize(GameObject playerRef)
    {
        if (playerRef == null)
        {
            Debug.LogError("[CheckPointManager] Initialize called with null playerRef.");
            return;
        }

        _player = playerRef;
        _playerTransform = _player.transform;

        // Cache PlayerHealth component
        _playerHealth = _player.GetComponent<PlayerHealth>();
        if (_playerHealth == null)
        {
            Debug.LogError($"[CheckPointManager] PlayerHealth component not found on '{_player.name}'.");
        }

        // Cache Rigidbody2D component (optional)
        _playerRb2D = _player.GetComponent<Rigidbody2D>();
        if (_playerRb2D == null)
        {
            Debug.LogWarning($"[CheckPointManager] Rigidbody2D not found on '{_player.name}'. Velocity reset on respawn may not work.");
        }

        // Set initial respawn position to player's starting position
        _lastRespawnPosition = _playerTransform.position;
        _hasCheckpoint = true;

        // Subscribe to the player's death event
        PlayerHealth.OnPlayerDied += HandlePlayerDeath;

        // Find all checkpoint trigger volumes in the scene and assign this manager
        allTriggers = Object.FindObjectsByType<CheckpointTriggerVolume>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        foreach (var trig in allTriggers)
        {
            trig.SetManager(this);
        }

        Debug.Log($"[CheckPointManager] Initialized. Initial respawn position at {_lastRespawnPosition}. Found {allTriggers.Length} checkpoints.");
    }

    private void OnDestroy()
    {
        // Unsubscribe from death event to avoid null-reference issues
        PlayerHealth.OnPlayerDied -= HandlePlayerDeath;
    }

    /// <summary>
    /// Called by CheckpointTriggerVolume to update the saved respawn position.
    /// </summary>
    /// <param name="pos">World-space respawn position to set.</param>
    public void SetRespawnPosition(Vector3 pos)
    {
        _lastRespawnPosition = pos;
        _hasCheckpoint = true;
        Debug.Log($"[CheckPointManager] Respawn position updated to {_lastRespawnPosition}");
    }

    /// <summary>
    /// Handles player death by starting a coroutine that waits for respawnDelay
    /// seconds before teleporting the player to the last respawn position and resetting health/velocity.
    /// </summary>
    private void HandlePlayerDeath()
    {
        // Start the respawn coroutine on death
        StartCoroutine(RespawnAfterDelay());
    }

    /// <summary>
    /// Coroutine that waits for respawnDelay seconds, then teleports the player
    /// and resets their health and velocity.
    /// </summary>
    private IEnumerator RespawnAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(respawnDelay);

        if (!_hasCheckpoint)
        {
            Debug.LogWarning($"[CheckPointManager] No respawn position set! Respawning at initial position: {_playerTransform.position}");
        }

        // Teleport the player to the saved respawn position
        _playerTransform.position = _lastRespawnPosition;

        // Reset velocity if Rigidbody2D is available
        if (_playerRb2D != null)
        {
            _playerRb2D.velocity = Vector2.zero;
        }

        // Reset player health and controls
        if (_playerHealth != null)
        {
            _playerHealth.ResetHealth();
        }

        Debug.Log($"[CheckPointManager] Player respawned at {_lastRespawnPosition} after {respawnDelay} seconds delay.");
    }
}
