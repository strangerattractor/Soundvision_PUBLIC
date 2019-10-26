using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    public class HandColorOverlayer : MonoBehaviour
    {
        [Tooltip("Camera used to estimate the overlay positions of 3D-objects over the background. By default it is the main camera.")]
        public Camera foregroundCamera;

        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("Game object used to overlay the left hand.")]
        public Transform leftHandOverlay;

        [Tooltip("Game object used to overlay the right hand.")]
        public Transform rightHandOverlay;

        //public float smoothFactor = 10f;

        // reference to KinectManager
        private KinectManager kinectManager;


        void Update()
        {
            if (foregroundCamera == null)
            {
                // by default use the main camera
                foregroundCamera = Camera.main;
            }

            if (kinectManager == null)
            {
                kinectManager = KinectManager.Instance;
            }

            if (kinectManager && kinectManager.IsInitialized() && foregroundCamera)
            {
                // get the background rectangle (use the portrait background, if available)
                Rect backgroundRect = foregroundCamera.pixelRect;
                PortraitBackground portraitBack = PortraitBackground.Instance;

                if (portraitBack && portraitBack.enabled)
                {
                    backgroundRect = portraitBack.GetBackgroundRect();
                }

                // overlay the joints
                if (kinectManager.IsUserDetected(playerIndex))
                {
                    ulong userId = kinectManager.GetUserIdByIndex(playerIndex);

                    OverlayJoint(userId, (int)KinectInterop.JointType.HandLeft, leftHandOverlay, backgroundRect);
                    OverlayJoint(userId, (int)KinectInterop.JointType.HandRight, rightHandOverlay, backgroundRect);
                }

            }
        }


        // overlays the given object over the given user joint
        private void OverlayJoint(ulong userId, int jointIndex, Transform overlayObj, Rect imageRect)
        {
            if (kinectManager.IsJointTracked(userId, jointIndex))
            {
                Vector3 posJoint = kinectManager.GetJointKinectPosition(userId, jointIndex, false);

                if (posJoint != Vector3.zero)
                {
                    int sensorIndex = kinectManager.GetPrimaryBodySensorIndex();
                    KinectInterop.SensorData sensorData = kinectManager.GetSensorData(sensorIndex);

                    // 3d position to depth
                    Vector2 posDepth = kinectManager.MapSpacePointToDepthCoords(sensorIndex, posJoint);
                    ushort depthValue = kinectManager.GetDepthForPixel(sensorIndex, (int)posDepth.x, (int)posDepth.y);

                    if (posDepth != Vector2.zero && depthValue > 0 && sensorData != null)
                    {
                        // depth pos to color pos
                        Vector2 posColor = kinectManager.MapDepthPointToColorCoords(sensorIndex, posDepth, depthValue);

                        if (!float.IsInfinity(posColor.x) && !float.IsInfinity(posColor.y))
                        {
                            float xScaled = (float)posColor.x * imageRect.width / sensorData.colorImageWidth;
                            float yScaled = (float)posColor.y * imageRect.height / sensorData.colorImageHeight;

                            float xScreen = imageRect.x + (sensorData.colorImageScale.x > 0 ? xScaled : imageRect.width - xScaled);
                            float yScreen = imageRect.y + (sensorData.colorImageScale.y > 0 ? yScaled : imageRect.height - yScaled);

                            if (overlayObj && foregroundCamera)
                            {
                                float distanceToCamera = overlayObj.position.z - foregroundCamera.transform.position.z;
                                posJoint = foregroundCamera.ScreenToWorldPoint(new Vector3(xScreen, yScreen, distanceToCamera));

                                overlayObj.position = posJoint;
                            }
                        }
                    }
                }

            }
        }

    }
}
