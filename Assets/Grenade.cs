using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    float timer = 2;
    float countdown;
    float radius = 100;
    float force = 500;

    bool hasExploded;
    // Start is called before the first frame update
    [SerializeField] GameObject ExplodingParticle;
    void Start()
    {
        countdown = timer;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0 && !hasExploded)
        {
           Explode();
        }
            
    }
    void Explode()
    {
       GameObject spawnedParticle = Instantiate(ExplodingParticle, transform.position, transform.rotation);
        Destroy(spawnedParticle, 1);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider nerabyObject in colliders)
        {
            Rigidbody rb = nerabyObject.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.AddExplosionForce (force, transform.position, radius);
            }
        }
        hasExploded = true;

        Destroy(gameObject);
    }
}
