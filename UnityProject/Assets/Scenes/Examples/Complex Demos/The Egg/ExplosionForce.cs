using UnityEngine;
using System.Collections;

// Applies an explosion force to all nearby rigidbodies
public class ExplosionForce : MonoBehaviour
{
    public float radius = 5.0F;
    public float power = 10.0F;

    public void OnThresholdExceeded()
    {
        Vector3 explosionPos = this.gameObject.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
            Debug.Log(this.gameObject.transform.position);
        }
    }
}