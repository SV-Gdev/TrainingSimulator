using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private float laserdistance = 10f;

    [SerializeField ] private LayerMask ignoreMask;

    [SerializeField] private UnityEvent OnhitTarget;

    private RaycastHit rayhit;
    private Ray ray;


    private void Start()
    {
        lineRenderer.positionCount = 2;
    } 
    private void Update()
    {
        ray =new(transform.position, transform.forward);

        if (Physics.Raycast(ray, out rayhit, laserdistance, ~ignoreMask))
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, rayhit.point);
            if (rayhit.collider.TryGetComponent(out Target target))
            {
                target.Hit();
                OnhitTarget?.Invoke();
            }
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * laserdistance);
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
