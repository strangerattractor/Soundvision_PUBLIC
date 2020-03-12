using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cylvester;

public class Hatchability : MonoBehaviour
{
    public Camera cam;
    public GameObject seed;
    List<GameObject> gameObjects = new List<GameObject>();
    private float speed_ = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        int Nx = 5;
        int Ny = 1;
        float sc = 2;
        for (int i = -Ny; i <= Ny; i++)
        {
            for (int j = -Nx; j <= Nx; j++)
            {
                var gp = new GameObject("node");
                gp.transform.parent = transform;
                gp.transform.position = new Vector3(j * sc, i * sc, 0);
                gp.transform.localScale = new Vector3(sc, sc, sc);
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
        int next = (int)Mathf.Floor(Random.Range(0, 5));
        //gameObjects[index].GetComponent<cylvester.CubeAnimation>().nextMove = next;
        //gameObjects[index].GetComponent<cylvester.CubeAnimation>().Invoke("OnTriggerReceived", 0);
        float count = 0;
        foreach (var g in gameObjects)
        {
            if (Random.Range(0.0f, 1.0f) < 0.5f) continue;
            g.GetComponent<cylvester.CubeAnimation>().nextMove = next;
            g.GetComponent<cylvester.CubeAnimation>().Invoke("OnTriggerReceived", count);
            //count += 0.02f;
        }
    }

    public void OnStateChanged(IStateReader currentState)
    {
        speed_ = currentState.CurrentState.Speed;
        foreach (var g in gameObjects)
        {
            g.GetComponent<cylvester.CubeAnimation>().speed_ = speed_;
        }
    }
}
