using UnityEngine;

/// <summary>
/// Manages all EnemyBase instances in the scene.
/// Receives the PlayerController2D from GameManager and injects it into each enemy.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public EnemyBase[] allEnemies;
    /// <summary>
    /// Initializes all enemies by passing the playerController reference.
    /// Call this once when the scene starts or when enemies are spawned.
    /// </summary>
    /// <param name="playerController">The PlayerController2D instance to share with enemies.</param>
    public void Initialize(PlayerController2D playerController)
    {
        // Using the newer FindObjectsByType API to avoid the obsolete warning.
        // We exclude inactive objects for performance (only active enemies).
            allEnemies = Object.FindObjectsByType<EnemyBase>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyBase enemy in allEnemies)
        {
            enemy.Initialize(playerController);
        }
    }

    /// <summary>
    /// If enemies are spawned at runtime, call this method after instantiation:
    ///
    ///     EnemyBase spawned = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity)
    ///                              .GetComponent<EnemyBase>();
    ///     spawned.Initialize(playerController);
    ///
    /// Ensuring that each new enemy also receives the playerController.
    /// </summary>
}
