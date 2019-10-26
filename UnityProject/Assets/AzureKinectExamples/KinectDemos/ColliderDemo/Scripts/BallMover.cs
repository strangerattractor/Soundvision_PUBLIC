using UnityEngine;
using System.Collections;


namespace com.rfilkov.components
{
    public class BallMover : MonoBehaviour
    {
        void Update()
        {
            if (transform.position.y < -2f)
            {
                Destroy(gameObject);
            }
        }
    }
}
