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
        
        defaultLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (aimPos == null) return;

       
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        bool isMovingSideways = Mathf.Abs(horizontalInput) > 0.1f;

       
        bool shouldLookAtAim = isAiming && !isMovingSideways;

        if (shouldLookAtAim)
        {
            Vector3 targetDirection = aimPos.position - transform.position;
            if (targetDirection != Vector3.zero)
            {
               
                Quaternion targetWorldRotation = Quaternion.LookRotation(targetDirection) * Quaternion.Euler(rotationOffset);

               
                transform.rotation = Quaternion.Slerp(transform.rotation, targetWorldRotation, Time.deltaTime * smoothSpeed);
            }
        }
        else
        {
           
            transform.localRotation = Quaternion.Slerp(transform.localRotation, defaultLocalRotation, Time.deltaTime * smoothSpeed);
        }
    }
}