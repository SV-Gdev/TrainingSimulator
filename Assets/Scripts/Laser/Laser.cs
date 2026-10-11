using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float laserdistance = 10f;
    [SerializeField] private LayerMask ignoreMask;
    [SerializeField] private UnityEvent OnhitTarget;
    [SerializeField] private float damageAmount = 10f; 

    private RaycastHit rayhit;
    private Ray ray;
    private float damageAccumulator = 0f;

    private void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
        }
    }

    private void Update()
    {
        ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out rayhit, laserdistance, ~ignoreMask))
        {
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, rayhit.point);
            }

            if (rayhit.collider != null)
            {
                PlayerHealth playerHealth = rayhit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                   
                    damageAccumulator += damageAmount * Time.deltaTime;
                    if (damageAccumulator >= 1f)
                    {
                        int damageToApply = Mathf.FloorToInt(damageAccumulator);
                        damageAccumulator -= damageToApply;

                        playerHealth.TakeContinuousDamage(damageToApply);
                    }

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.InflictLaserDamage();
                    }

                    OnhitTarget?.Invoke();
                }
            }
        }
        else
        {
            damageAccumulator = 0f; 
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, transform.position + transform.forward * laserdistance);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, ray.direction * laserdistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(rayhit.point, 0.23f);
    }
}