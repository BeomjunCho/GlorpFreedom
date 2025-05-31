using UnityEngine;

/// <summary>
/// Example GameManager that provides the PlayerController2D reference to the EnemyManager
/// when the game starts.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Tooltip("PlayerController2D component attached to the player GameObject.")]
    [SerializeField] private PlayerController2D playerController;

    [Tooltip("Reference to the EnemyManager in the scene.")]
    [SerializeField] private EnemyManager enemyManager;

    private void Start()
    {
        // Ensure references are assigned
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

        // Inject the playerController into all enemies
        enemyManager.Initialize(playerController);

        // Additional initialization logic can follow here if needed
    }
}
