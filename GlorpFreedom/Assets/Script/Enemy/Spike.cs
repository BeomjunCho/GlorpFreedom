using UnityEngine;

public class Spike : MonoBehaviour
{
    [Tooltip("Damage to deal to the player on contact")]
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the object that entered is tagged "Player", deal damage
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damageAmount);
            }
        }
    }
}
