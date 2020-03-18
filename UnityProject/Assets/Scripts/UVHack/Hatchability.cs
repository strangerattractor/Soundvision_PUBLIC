using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cylvester;

namespace cylvester
{
    public class Hatchability : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private GameObject seed;
        [SerializeField] private GameObject backdropSeed;
        List<GameObject> gameObjects = new List<GameObject>();
        GameObject backdrop;
        private float speed_ = 1.0f;
        [SerializeField] private int Nx = 0;
        [SerializeField] private int Ny = 0;
        [SerializeField] private float boxScale = 1;
        [SerializeField] private float textureSpread = 1;
        [SerializeField] private float dampening = 0.1f;

        // Start is called before the first frame update
        void Start()
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            if (backdropSeed != null)
            {
                backdrop = Instantiate(backdropSeed, transform);
                backdrop.GetComponent<UVMapToScreen>().cam = cam;
                backdrop.GetComponent<UVMapToScreen>().amount = 0.02f;
            }

            for (int i = -Ny; i <= Ny; i++)
            {
                for (int j = -Nx; j <= Nx; j++)
                {
                    var gp = new GameObject("node");
                    gp.transform.parent = transform;
                    gp.transform.position = new Vector3(j * boxScale, i * boxScale, 0);
                    gp.transform.localScale = new Vector3(boxScale, boxScale, boxScale);
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
        }

        public void OnTriggerReceived()
        {
            if (backdrop != null)
            {
                backdrop.GetComponent<UVMapToScreen>().textureSpread = textureSpread;
                backdrop.GetComponent<UVMapToScreen>().dampening = dampening;
            }

            int index = (int)Mathf.Floor(Random.Range(0, gameObjects.Count));
            int next = (int)Mathf.Floor(Random.Range(0, 5));
            float count = 0;
            foreach (var g in gameObjects)
            {
                g.GetComponent<UVMapToScreen>().textureSpread = textureSpread;
                g.GetComponent<UVMapToScreen>().dampening = dampening;
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
}
