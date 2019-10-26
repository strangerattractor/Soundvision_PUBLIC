using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    public class BallSpawner : MonoBehaviour
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("Prefab used to instantiate balls in the scene.")]
        public Transform ballPrefab;

        [Tooltip("Prefab used to instantiate cubes in the scene.")]
        public Transform cubePrefab;

        [Tooltip("How many objects do we want to spawn.")]
        public int numberOfObjects = 20;

        private float nextSpawnTime = 0.0f;
        private float spawnRate = 1.5f;
        private int ballsCount = 0;


        void Update()
        {
            if (nextSpawnTime < Time.time)
            {
                SpawnBalls();
                nextSpawnTime = Time.time + spawnRate;

                spawnRate = Random.Range(0f, 1f);
                //numberOfBalls = Mathf.RoundToInt(Random.Range(1f, 10f));
            }
        }

        void SpawnBalls()
        {
            KinectManager kinectManager = KinectManager.Instance;

            if (ballPrefab && cubePrefab && ballsCount < numberOfObjects &&
                kinectManager && kinectManager.IsInitialized() && kinectManager.IsUserDetected(playerIndex))
            {
                ulong userId = kinectManager.GetUserIdByIndex(playerIndex);
                Vector3 posUser = kinectManager.GetUserPosition(userId);

                float xOfs = Random.Range(-1.5f, 1.5f);
                float zOfs = Random.Range(-2.0f, 1.0f);
                float yOfs = Random.Range(1.0f, 4.0f);
                Vector3 spawnPos = new Vector3(posUser.x + xOfs, posUser.y + yOfs, posUser.z + zOfs);

                int ballOrCube = Mathf.RoundToInt(Random.Range(0f, 1f));

                Transform ballTransform = Instantiate(ballOrCube > 0 ? ballPrefab : cubePrefab, spawnPos, Quaternion.identity) as Transform;
                ballTransform.GetComponent<Renderer>().material.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);
                ballTransform.GetComponent<Rigidbody>().drag = Random.Range(1f, 100f);
                ballTransform.parent = transform;

                ballsCount++;
            }
        }

    }
}
