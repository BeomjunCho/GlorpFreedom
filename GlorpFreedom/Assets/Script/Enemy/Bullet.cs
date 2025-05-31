using UnityEngine;

/// <summary>
/// Bullet behavior: moves in a specified direction at a constant speed,
/// and destroys itself after a set lifetime or on collision.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Tooltip("Speed at which the bullet travels.")]
    [SerializeField] private float speed = 10f;

    [Tooltip("Time (in seconds) before the bullet is automatically destroyed.")]
    [SerializeField] private float lifetime = 2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Destroy the bullet after 'lifetime' seconds to avoid clutter
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Move the bullet forward along its local right direction
        rb.velocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Deal 1 damage to the player if they have a PlayerHealth component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
        Destroy(gameObject);
    }
}
