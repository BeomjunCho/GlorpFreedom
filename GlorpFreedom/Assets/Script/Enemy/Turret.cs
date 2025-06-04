using UnityEngine;

/// <summary>
/// Turret enemy that periodically shoots bullets in bursts,
/// but only when the player is within a specified shooting range.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Turret : EnemyBase
{
    [Header("Bullet Settings")]
    [Tooltip("Prefab of the bullet to shoot.")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Point from which bullets are spawned.")]
    [SerializeField] private Transform firePoint;

    [Header("Firing Pattern Settings")]
    [Tooltip("Time in seconds between individual shots during the shooting phase.")]
    [SerializeField] private float fireRate = 0.5f;

    [Tooltip("Duration (in seconds) of the active shooting phase.")]
    [SerializeField] private float shootingDuration = 3f;

    [Tooltip("Duration (in seconds) of the pause (idle) phase.")]
    [SerializeField] private float pauseDuration = 2f;

    [Header("Target Tracking Settings")]
    [Tooltip("If true, the turret will rotate to face the player.")]
    [SerializeField] private bool rotateTowardsTarget = true;

    [Header("Shooting Range Settings")]
    [Tooltip("Maximum distance at which the turret can shoot the player.")]
    [SerializeField] private float shootingRange = 5f;

    [SerializeField] private AudioSource _shootingAudioSource;

    // Internal timers and state
    private float phaseTimer = 0f;
    private float nextFireTime = 0f;
    private bool isShootingPhase = true;

    private void Awake()
    {
        if (firePoint == null)
        {
            Debug.LogError("Turret: firePoint is not assigned.", this);
        }
    }

    private void Update()
    {
        if (isDead) return;

        phaseTimer += Time.deltaTime;

        if (isShootingPhase)
        {
            // Only rotate or attempt to shoot if the player is within shootingRange
            if (playerController != null)
            {
                float distanceToPlayer = Vector2.Distance(firePoint.position, playerController.transform.position);
                if (distanceToPlayer <= shootingRange)
                {
                    if (rotateTowardsTarget)
                    {
                        RotateToFacePlayer();
                    }

                    if (Time.time >= nextFireTime)
                    {
                        FireBullet();
                        nextFireTime = Time.time + fireRate;
                    }
                }
            }

            if (phaseTimer >= shootingDuration)
            {
                isShootingPhase = false;
                phaseTimer = 0f;
            }
        }
        else
        {
            // Pause phase: no firing; after pauseDuration, switch back
            if (phaseTimer >= pauseDuration)
            {
                isShootingPhase = true;
                phaseTimer = 0f;
                nextFireTime = Time.time;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the player dashes into this turret, kill it immediately
        if (IsPlayerDashing(other))
        {
            Die();
        }
    }

    /// <summary>
    /// Instantiates one bullet at the firePoint with its current rotation.
    /// </summary>
    protected virtual void FireBullet()
    {
        _shootingAudioSource.Play();
        if (bulletPrefab == null || firePoint == null) return;
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        // Optional: Play muzzle flash or sound effect here.
    }

    /// <summary>
    /// Rotates the firePoint (and optionally the turret body) to face the player.
    /// </summary>
    private void RotateToFacePlayer()
    {
        Vector3 direction = playerController.transform.position - firePoint.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);

        // If you want the turret body to rotate as well:
        // transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        // Optional: Play explosion effect or sound here.
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualize the firePoint position
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        }

        // Visualize shooting range around the firePoint
        Gizmos.color = Color.green;
        Vector3 center = (firePoint != null) ? firePoint.position : transform.position;
        Gizmos.DrawWireSphere(center, shootingRange);
    }
#endif
}
