using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.rfilkov.components
{
    public class CollisionLogger : MonoBehaviour
    {

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<CollisionLogger>())
                return;

            Debug.Log("Detected collision of " + gameObject.name + " with " + other.gameObject.name);
        }
    }
}
