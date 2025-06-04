using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class VolumeTextTriggerTMP : MonoBehaviour
{
    [Tooltip("TextMeshPro component to show/hide above the trigger volume.")]
    [SerializeField] private TextMeshPro textMeshPro;

    [Tooltip("Tag used to identify the player.")]
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        // Hide the TextMeshPro object at start
        if (textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("VolumeTextTriggerTMP: TextMeshPro is not assigned.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // When the player enters, enable the TextMeshPro above the volume
        if (other.CompareTag(playerTag) && textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // When the player exits, hide the TextMeshPro again
        if (other.CompareTag(playerTag) && textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(false);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw a translucent cube in Scene view to visualize the trigger area
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Collider2D col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            Gizmos.DrawCube(box.bounds.center, box.bounds.size);
        }
    }
#endif
}
