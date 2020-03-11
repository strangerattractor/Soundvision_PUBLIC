using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractalization : MonoBehaviour
{
    public GameObject seed;
    List<GameObject> gameObjects = new List<GameObject>();
    public int generation = 5;
    public float scaleMin = 0.5f;
    public float scaleMax = 0.9f;
    public float offset = 1;

    // Start is called before the first frame update
    void Start()
    {
        var parent = transform;
        for(int i = 0; i < generation; i++)
        {
            var g = Instantiate(seed, parent) as GameObject;
            g.transform.localPosition = new Vector3(0, offset * 0.5f, 0);
            //gameObjects.Add(g);
            //parent = g.transform;

            var h = new GameObject("node");
            h.transform.parent = parent;
            h.transform.localPosition = new Vector3(0, offset, 0);
            float randomScale = Random.Range(scaleMin, scaleMax);
            h.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            h.transform.localEulerAngles = new Vector3(0, 0, 30);
            gameObjects.Add(h);
            parent = h.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float count = 0;
        foreach(var g in gameObjects)
        {
            g.transform.localEulerAngles = new Vector3(30 * Mathf.Sin(Time.time*0.25f + count * 0.01f), 0, 30 * Mathf.Sin(Time.time + count * 0.02f));
            count++;
        }
    }
}
