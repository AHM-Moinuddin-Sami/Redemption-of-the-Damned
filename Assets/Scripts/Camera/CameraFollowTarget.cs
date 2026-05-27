using UnityEngine;

/*
 * CameraFollowTarget
 * ------------------
 * Makes the scene camera follow a target transform, usually the player.
 *
 * This script keeps the camera separate from the player prefab. That is safer
 * than placing the camera inside the player prefab because the player is spawned
 * and destroyed by GameBootstrap.
 *
 * Main responsibilities:
 * - store a target to follow
 * - move the camera to the target every LateUpdate
 * - preserve the camera's Z position
 * - optionally smooth the camera movement
 *
 * Why LateUpdate?
 * The player moves during normal update/input logic. LateUpdate runs afterward,
 * so the camera follows the player's final position for that frame.
 *
 * Current behavior:
 * - follows the player on X and Y
 * - keeps the camera's original Z position
 * - supports instant or smoothed following
 */

public class CameraFollowTarget : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private bool useSmoothing = false;
    [SerializeField] private float smoothSpeed = 12f;

    private float fixedZPosition;

    private void Awake()
    {
        // Store the camera's starting Z position so following does not move it
        // onto the same Z plane as the player.
        fixedZPosition = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            fixedZPosition
        );

        if (useSmoothing)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        // GameBootstrap calls this after spawning the player.
        target = newTarget;

        if (target == null)
        {
            return;
        }

        // Snap immediately once so the camera starts in the correct position.
        transform.position = new Vector3(
            target.position.x,
            target.position.y,
            fixedZPosition
        );
    }
}