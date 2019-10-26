using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// ForegroundToRenderer sets the texture of the Renderer-component to be the BRM's foreground texture.
    /// </summary>
    public class ForegroundToRenderer : MonoBehaviour
    {
        private Renderer thisRenderer = null;
        private KinectManager kinectManager = null;
        private BackgroundRemovalManager backManager = null;

        void Start()
        {
            thisRenderer = GetComponent<Renderer>();

            kinectManager = KinectManager.Instance;
            backManager = FindObjectOfType<BackgroundRemovalManager>();

            if (kinectManager && kinectManager.IsInitialized() && backManager && backManager.enabled)
            {
                Vector3 localScale = transform.localScale;
                localScale.z = localScale.x * kinectManager.GetColorImageHeight(backManager.sensorIndex) / kinectManager.GetColorImageWidth(backManager.sensorIndex);
                //localScale.x = -localScale.x;

                //// apply color image scale
                //Vector3 colorImageScale = kinectManager.GetColorImageScale(backManager.sensorIndex);
                //if (colorImageScale.x < 0f)
                //    localScale.x = -localScale.x;
                //if (colorImageScale.y < 0f)
                //    localScale.z = -localScale.z;

                transform.localScale = localScale;
            }
        }


        void Update()
        {
            if (thisRenderer && thisRenderer.sharedMaterial.mainTexture == null)
            {
                if (kinectManager && backManager && backManager.enabled /**&& backManager.IsBackgroundRemovalInitialized()*/)
                {
                    thisRenderer.sharedMaterial.mainTexture = backManager.GetForegroundTex();
                }
            }
            //else if (thisRenderer && thisRenderer.sharedMaterial.mainTexture != null)
            //{
            //    if (KinectManager.Instance == null)
            //    {
            //        thisRenderer.sharedMaterial.mainTexture = null;
            //    }
            //}
        }


        void OnApplicationPause(bool isPaused)
        {
            // fix for app pause & restore (UWP)
            if (isPaused && thisRenderer && thisRenderer.sharedMaterial.mainTexture != null)
            {
                thisRenderer.sharedMaterial.mainTexture = null;
            }
        }

    }
}
