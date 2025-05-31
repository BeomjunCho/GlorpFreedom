using UnityEngine;

/// <summary>
/// 2D Camera Follow:
/// - The camera follows the designated target (player) at all times.
/// - You can adjust the offset to keep the player at a specific position on screen.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Header("=== Follow Settings ===")]
    [Tooltip("The Transform of the target (e.g., player) to follow.")]
    [SerializeField] private Transform target;

    [Tooltip("Offset from the target position (keep Z at negative value for 2D).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Tooltip("Enable smooth interpolation.")]
    [SerializeField] private bool useSmoothing = false;

    [Tooltip("If smoothing is enabled, this controls how fast the camera catches up.")]
    [SerializeField] private float smoothingSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null)
            return; // If no target assigned, do nothing

        // Calculate desired camera position: target position plus offset
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            offset.z
        );

        if (useSmoothing)
        {
            // Smoothly interpolate from current position to desired position
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothingSpeed * Time.deltaTime
            );
        }
        else
        {
            // Immediately set camera position to desired position
            transform.position = desiredPosition;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere at the target+offset position for visualization
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 gizmoPos = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                0f
            );
            Gizmos.DrawWireSphere(gizmoPos, 0.2f);
        }
    }
#endif
}
