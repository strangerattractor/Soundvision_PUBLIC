using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using com.rfilkov.kinect;

namespace com.rfilkov.components
{
    /// <summary>
    /// This component loads the first real scene of the game, after the startup scene.
    /// </summary>
    public class LoadFirstLevel : MonoBehaviour
    {
        // prevents multiple loads
        private bool levelLoaded = false;


        void Update()
        {
            KinectManager kinectManager = KinectManager.Instance;

            if (!levelLoaded && kinectManager && kinectManager.IsInitialized())
            {
                levelLoaded = true;
                SceneManager.LoadScene(1);
            }
        }

    }
}
