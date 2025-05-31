using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheckpointTriggerVolume : MonoBehaviour
{
    // Reference to the checkpoint manager (assigned automatically)
    private CheckPointManager checkpointManager;

    [Tooltip("Transform indicating where the player should respawn when this checkpoint is activated.")]
    [SerializeField] private Transform respawnPoint;

    // Prevent multiple activations if desired
    private bool _triggered = false;

    // Cached reference to the Collider2D for gizmo drawing
    private Collider2D _collider2D;

    private void Awake()
    {
        // Cache the Collider2D component
        _collider2D = GetComponent<Collider2D>();

        // Ensure the collider is set as a trigger
        if (!_collider2D.isTrigger)
        {
            Debug.LogWarning($"[CheckpointTriggerVolume] Collider is not set to 'Is Trigger' on '{name}'. Enabling trigger mode.");
            _collider2D.isTrigger = true;
        }

        // If no explicit respawn point is set, use this GameObject's transform
        if (respawnPoint == null)
        {
            respawnPoint = this.transform;
        }
    }

    /// <summary>
    /// Called by CheckPointManager.Initialize to assign this manager instance.
    /// </summary>
    /// <param name="manager">The CheckPointManager instance in the scene.</param>
    public void SetManager(CheckPointManager manager)
    {
        if (manager == null)
        {
            Debug.LogError("[CheckpointTriggerVolume] Attempted to set null CheckPointManager.");
            return;
        }
        checkpointManager = manager;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;

        // Only respond to objects tagged "Player"
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 respawnPos = respawnPoint.position;
            if (checkpointManager != null)
            {
                checkpointManager.SetRespawnPosition(respawnPos);
            }
            else
            {
                Debug.LogWarning($"[CheckpointTriggerVolume] No CheckPointManager assigned for checkpoint '{name}'.");
            }

            // If you want each checkpoint to activate only once, uncomment below:
            // _triggered = true;

            Debug.Log($"[CheckpointTriggerVolume] Player hit checkpoint '{name}'. Respawn position set to {respawnPos}.");
        }
    }

    /// <summary>
    /// Draws a semi-transparent box and outline in the Scene view to visualize the trigger area.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Ensure we have a Collider2D reference (even in edit mode)
        if (_collider2D == null)
        {
            _collider2D = GetComponent<Collider2D>();
            if (_collider2D == null) return;
        }

        // Get the bounds of the Collider2D
        Bounds bounds = _collider2D.bounds;

        // Set a semi-transparent blue color for the filled area
        Gizmos.color = new Color(0f, 0f, 1f, 0.15f);
        Gizmos.DrawCube(bounds.center, bounds.size);

        // Set a solid blue color for the outline
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
