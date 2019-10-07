using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject column;
    
    public void OnThresholdExceeded()
    {
        var columnObject = Instantiate(column);
        columnObject.transform.parent = this.transform;
    }
    
}
