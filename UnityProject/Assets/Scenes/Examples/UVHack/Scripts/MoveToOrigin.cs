using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToOrigin : MonoBehaviour
{
    public float tween = 0;
    Vector3 spawnedPosition;
    Vector3 originPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (transform.parent == null)
        {
            return;
        }

        spawnedPosition = transform.parent.position;
        /*
        if(transform.parent != null && transform.parent.parent != null)
        {
            var parent = transform.parent.parent; // TODO
            originPosition = transform.InverseTransformPoint(parent.TransformPoint(parent.position));
        }
        else
        {
            originPosition = Vector3.zero;
        }
        */
        originPosition = new Vector3(0,0,0);// transform.InverseTransformPoint(Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null)
        {
            return;
        }

        transform.parent.position = Vector3.Lerp(spawnedPosition, originPosition, tween);
    }
}
