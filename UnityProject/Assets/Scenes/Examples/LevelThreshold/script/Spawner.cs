using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject column;


    public void OnThresholdExceeded()
    {
        var spawnposition = this.gameObject.GetComponent<Transform>();
        var columnObject = Instantiate(column, spawnposition);
        //columnObject.transform.parent = spawnposition;
    }
    
}
