using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// ForegroundToRawImage sets the texture of the RawImage-component to be the BRM's foreground texture.
    /// </summary>
    public class ForegroundToRawImage : MonoBehaviour
    {
        private RawImage rawImage;
        private KinectManager kinectManager = null;
        private BackgroundRemovalManager backManager = null;


        void Start()
        {
            rawImage = GetComponent<RawImage>();

            kinectManager = KinectManager.Instance;
            backManager = FindObjectOfType<BackgroundRemovalManager>();
        }


        void Update()
        {
            if (rawImage && rawImage.texture == null)
            {
                if (kinectManager && backManager && backManager.enabled /**&& backManager.IsBackgroundRemovalInitialized()*/)
                {
                    rawImage.texture = backManager.GetForegroundTex();  // user's foreground texture
                    rawImage.rectTransform.localScale = kinectManager.GetColorImageScale(backManager.sensorIndex);
                    rawImage.color = Color.white;

                }
            }
            //else if (rawImage && rawImage.texture != null)
            //{
            //    if (KinectManager.Instance == null)
            //    {
            //        rawImage.texture = null;
            //        rawImage.color = Color.clear;
            //    }
            //}
        }


        void OnApplicationPause(bool isPaused)
        {
            // fix for app pause & restore (UWP)
            if (isPaused && rawImage && rawImage.texture != null)
            {
                rawImage.texture = null;
                rawImage.color = Color.clear;
            }
        }

    }
}
