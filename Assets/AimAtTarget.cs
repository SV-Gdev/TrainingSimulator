using UnityEngine;

public class AimAtTarget : MonoBehaviour
{
    [Header("References")]
    public Transform aimPos;

    [Header("Aiming Settings")]
    public bool isAiming = true;
    public float smoothSpeed = 15f;

    [Header("Offset Correction (When Aiming)")]
    public Vector3 rotationOffset = new Vector3(0, 90, 0);

    private Quaternion defaultLocalRotation;

    void Start()
    {
        // Remember the default pose inside the hand/holder before any script overrides
        defaultLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (aimPos == null) return;

        // 1. Check for sideways movement input (A/D or Left/Right arrows)
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        bool isMovingSideways = Mathf.Abs(horizontalInput) > 0.1f;

        // 2. Master Check: Only look at crosshair when not moving sideways
        bool shouldLookAtAim = isAiming && !isMovingSideways;

        if (shouldLookAtAim)
        {
            Vector3 targetDirection = aimPos.position - transform.position;
            if (targetDirection != Vector3.zero)
            {
                // Calculate world rotation pointing at aimPos + mesh correction
                Quaternion targetWorldRotation = Quaternion.LookRotation(targetDirection) * Quaternion.Euler(rotationOffset);

                // Smoothly aim at target
                transform.rotation = Quaternion.Slerp(transform.rotation, targetWorldRotation, Time.deltaTime * smoothSpeed);
            }
        }
        else
        {
            // Smoothly return to the natural hand/animation pose without forced world angles
            transform.localRotation = Quaternion.Slerp(transform.localRotation, defaultLocalRotation, Time.deltaTime * smoothSpeed);
        }
    }
}