using UnityEngine;

/// <summary>
/// GameManager initializes EnemyManager and CheckPointManager at game start.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Tooltip("PlayerController2D component attached to the player GameObject.")]
    [SerializeField] private PlayerController2D playerController;

    [Tooltip("Reference to the EnemyManager in the scene.")]
    [SerializeField] private EnemyManager enemyManager;

    [Tooltip("Reference to the CheckPointManager in the scene.")]
    [SerializeField] private CheckPointManager checkpointManager;

    private void Start()
    {
        // Validate references
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

        // Initialize EnemyManager with the player's controller
        enemyManager.Initialize(playerController);

        // Initialize CheckPointManager with the player GameObject
        checkpointManager.Initialize(playerController.gameObject);

        Debug.Log("[GameManager] Initialization complete: EnemyManager and CheckPointManager are set up.");
    }
}
