using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cylvester;

public class Hatchability : MonoBehaviour
{
    public Camera cam;
    public GameObject seed;
    List<GameObject> gameObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        int N = 2;
        for (int i = -N; i <= N; i++)
        {
            for (int j = -N; j <= N; j++)
            {
                var gp = new GameObject("node");
                gp.transform.parent = transform;
                gp.transform.position = new Vector3(j, i, 0);
                var g = Instantiate(seed, gp.transform);
                g.GetComponent<UVMapToScreen>().cam = cam;
                g.GetComponent<UVMapToScreen>().amount = 0.02f;
                gameObjects.Add(g);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //foreach(var g in gameObjects)
        //{
        //    g.transform.eulerAngles = new Vector3(0, Time.time * 10, 0);
        //}
    }

    public void OnTriggerReceived()
    {
        int index = (int)Mathf.Floor(Random.Range(0, gameObjects.Count));
        gameObjects[index].GetComponent<cylvester.OrangeCube>().Invoke("OnTriggerReceived", 0);
    }

    public void OnStateChanged(IStateReader currentState)
    {
    }
}
