using UnityEngine;
using System.Collections;
using System;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// JointOverlayer overlays the given body joint with the given virtual object.
    /// </summary>
    public class JointOverlayer : MonoBehaviour
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("Kinect joint that is going to be overlayed.")]
        public KinectInterop.JointType trackedJoint = KinectInterop.JointType.HandRight;

        [Tooltip("Game object used to overlay the joint.")]
        public Transform overlayObject;

        [Tooltip("Whether to rotate the overlay object, according to the joint rotation.")]
        public bool rotateObject = true;

        [Tooltip("Smooth factor used for joint rotation.")]
        [Range(0f, 10f)]
        public float rotationSmoothFactor = 10f;

        [Tooltip("Depth sensor index - 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Tooltip("Camera that will be used to overlay the 3D-objects over the background.")]
        public Camera foregroundCamera;

        [Tooltip("Horizontal offset in the object's position with respect to the object's x-axis.")]
        [Range(-0.5f, 0.5f)]
        public float horizontalOffset = 0f;

        [Tooltip("Vertical offset in the object's position with respect to the object's y-axis.")]
        [Range(-0.5f, 0.5f)]
        public float verticalOffset = 0f;

        [Tooltip("Forward offset in the object's position with respect to the object's z-axis.")]
        [Range(-0.5f, 0.5f)]
        public float forwardOffset = 0f;

        //public UnityEngine.UI.Text debugText;

        [NonSerialized]
        public Quaternion initialRotation = Quaternion.identity;
        private bool objMirrored = false;

        // reference to KM
        private KinectManager kinectManager = null;


        public void Start()
        {
            // get reference to KM
            kinectManager = KinectManager.Instance;

            if (!foregroundCamera)
            {
                // by default - the main camera
                foregroundCamera = Camera.main;
            }

            if(overlayObject == null)
            {
                // by default - the current object
                overlayObject = transform;
            }

            if (rotateObject && overlayObject)
            {
                // always mirrored
                initialRotation = overlayObject.rotation; // Quaternion.Euler(new Vector3(0f, 180f, 0f));

                Vector3 vForward = foregroundCamera ? foregroundCamera.transform.forward : Vector3.forward;
                objMirrored = (Vector3.Dot(overlayObject.forward, vForward) < 0);

                overlayObject.rotation = Quaternion.identity;
            }
        }

        void Update()
        {
            if (kinectManager && kinectManager.IsInitialized() && foregroundCamera)
            {
                // get the background rectangle (use the portrait background, if available)
                Rect backgroundRect = foregroundCamera.pixelRect;
                PortraitBackground portraitBack = PortraitBackground.Instance;

                if (portraitBack && portraitBack.enabled)
                {
                    backgroundRect = portraitBack.GetBackgroundRect();
                }

                // overlay the joint
                ulong userId = kinectManager.GetUserIdByIndex(playerIndex);

                int iJointIndex = (int)trackedJoint;
                if (kinectManager.IsJointTracked(userId, iJointIndex))
                {
                    Vector3 posJoint = kinectManager.GetJointPosColorOverlay(userId, iJointIndex, sensorIndex, foregroundCamera, backgroundRect);

                    if (posJoint != Vector3.zero && overlayObject)
                    {
                        if (horizontalOffset != 0f)
                        {
                            // add the horizontal offset
                            Vector3 dirHorizOfs = overlayObject.InverseTransformDirection(new Vector3(horizontalOffset, 0, 0));
                            posJoint += dirHorizOfs;
                        }

                        if (verticalOffset != 0f)
                        {
                            // add the vertical offset
                            Vector3 dirVertOfs = overlayObject.InverseTransformDirection(new Vector3(0, verticalOffset, 0));
                            posJoint += dirVertOfs;
                        }

                        if (forwardOffset != 0f)
                        {
                            // add the forward offset
                            Vector3 dirFwdOfs = overlayObject.InverseTransformDirection(new Vector3(0, 0, forwardOffset));
                            posJoint += dirFwdOfs;
                        }

                        overlayObject.position = posJoint;

                        if(rotateObject)
                        {
                            Quaternion rotJoint = kinectManager.GetJointOrientation(userId, iJointIndex, !objMirrored);
                            rotJoint = initialRotation * rotJoint;

                            overlayObject.rotation = rotationSmoothFactor > 0f ?
                                Quaternion.Slerp(overlayObject.rotation, rotJoint, rotationSmoothFactor * Time.deltaTime) : rotJoint;
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
