using UnityEngine;
using System.Collections;
using System.IO;
using System;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// PhotoShooter provides public functions to take pictures of the currently rendered scene.
    /// </summary>
    public class PhotoShooter : MonoBehaviour
    {
        [Tooltip("Camera used to render the scene. If not set, the main camera will be used.")]
        public Camera foregroundCamera;

        [Tooltip("Array of sprite transforms that will be used for displaying the countdown until image shot.")]
        public Transform[] countdown;

        [Tooltip("UI-Text used to display information messages.")]
        public UnityEngine.UI.Text infoText;


        /// <summary>
        /// Counts down (from 3 for instance), then takes a picture and opens it
        /// </summary>
        public void CountdownAndMakePhoto()
        {
            StartCoroutine(CoCountdownAndMakePhoto());
        }


        // counts down (from 3 for instance), then makes a photo and opens it
        private IEnumerator CoCountdownAndMakePhoto()
        {
            if (countdown != null && countdown.Length > 0)
            {
                for (int i = 0; i < countdown.Length; i++)
                {
                    if (countdown[i])
                        countdown[i].gameObject.SetActive(true);

                    yield return new WaitForSeconds(1f);

                    if (countdown[i])
                        countdown[i].gameObject.SetActive(false);
                }
            }

            MakePhoto();
            yield return null;
        }


        /// <summary>
        /// Saves the screen image as png picture, and then opens the saved file.
        /// </summary>
        public void MakePhoto()
        {
            MakePhoto(true);
        }

        /// <summary>
        /// Saves the screen image as png picture, and optionally opens the saved file.
        /// </summary>
        /// <returns>The file name.</returns>
        /// <param name="openIt">If set to <c>true</c> opens the saved file.</param>
        public string MakePhoto(bool openIt)
        {
            int resWidth = Screen.width;
            int resHeight = Screen.height;

            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false); //Create new texture
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);

            // hide the info-text, if any
            if (infoText)
            {
                infoText.text = string.Empty;
            }

            if(!foregroundCamera)
            {
                foregroundCamera = Camera.main;
            }

            // render the camera
            if (foregroundCamera && foregroundCamera.enabled)
            {
                foregroundCamera.targetTexture = rt;
                foregroundCamera.Render();
                foregroundCamera.targetTexture = null;
            }

            // get the screenshot
            RenderTexture prevActiveTex = RenderTexture.active;
            RenderTexture.active = rt;

            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);

            // clean-up
            RenderTexture.active = prevActiveTex;
            Destroy(rt);

            byte[] btScreenShot = screenShot.EncodeToJPG();
            Destroy(screenShot);

            // save the screenshot as jpeg file
            string sDirName = Application.persistentDataPath + "/Screenshots";
            if (!Directory.Exists(sDirName))
                Directory.CreateDirectory(sDirName);

            string sFileName = sDirName + "/" + string.Format("{0:F0}", Time.realtimeSinceStartup * 10f) + ".jpg";
            File.WriteAllBytes(sFileName, btScreenShot);

            Debug.Log("Photo saved to: " + sFileName);
            if (infoText)
            {
                infoText.text = "Saved to: " + sFileName;
            }

            // open file
            if (openIt)
            {
                System.Diagnostics.Process.Start(sFileName);
            }

            return sFileName;
        }

    }
}
