using UnityEngine;
using System.Collections;
using System;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// ColorImageJointOverlayer overlays the given user's joint with virtual object over the user's BR image.
    /// </summary>
    public class ColorImageJointOverlayer : MonoBehaviour
    {
        [Tooltip("Depth sensor index - 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("Kinect joint that is going to be overlayed.")]
        public KinectInterop.JointType trackedJoint = KinectInterop.JointType.HandRight;

        [Tooltip("Plane game-object rendering the color camera image.")]
        public Transform planeObject;

        [Tooltip("Game object used to overlay the joint over the plane.")]
        public Transform overlayObject;

        //[Tooltip("Smoothing factor used for joint rotation.")]
        //public float smoothFactor = 10f;

        private KinectManager kinectManager = null;
        private Rect planeRect = new Rect();
        private bool planeRectSet = false;


        public void Start()
        {
            kinectManager = KinectManager.Instance;

            if(overlayObject == null)
            {
                overlayObject = transform;
            }

            if (overlayObject != null)
            {
                overlayObject.rotation = Quaternion.identity;
            }
        }

        void Update()
        {
            if (kinectManager && kinectManager.IsInitialized())
            {
                // get the plane rectangle to be used for object overlay
                if (!planeRectSet && planeObject)
                {
                    planeRectSet = true;

                    planeRect.width = 10f * Mathf.Abs(planeObject.localScale.x);
                    planeRect.height = 10f * Mathf.Abs(planeObject.localScale.z);
                    planeRect.x = planeObject.position.x - planeRect.width / 2f;
                    planeRect.y = planeObject.position.y - planeRect.height / 2f;
                }

                // overlay the object
                ulong userId = kinectManager.GetUserIdByIndex(playerIndex);

                int iJointIndex = (int)trackedJoint;
                if (planeObject && kinectManager.IsJointTracked(userId, iJointIndex))
                {
                    //Vector3 posJoint = manager.GetJointPosColorOverlay(userId, iJointIndex, foregroundCamera, backgroundRect);
                    Vector3 posJoint = kinectManager.GetJointPosColorOverlay(userId, iJointIndex, sensorIndex, planeRect);
                    posJoint.z = planeObject.position.z;

                    if (posJoint != Vector3.zero)
                    {
                        if (overlayObject)
                        {
                            overlayObject.position = posJoint;
                        }
                    }
                }
                else
                {
                    // make the overlay object invisible
                    if (overlayObject && overlayObject.position.z > 0f)
                    {
                        Vector3 posJoint = overlayObject.position;
                        posJoint.z = -10f;
                        overlayObject.position = posJoint;
                    }
                }

            }
        }

    }
}
