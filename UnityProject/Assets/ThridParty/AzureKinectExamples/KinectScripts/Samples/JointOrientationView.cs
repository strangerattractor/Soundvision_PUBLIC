using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    public class JointOrientationView : MonoBehaviour
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("The Kinect joint we want to track.")]
        public KinectInterop.JointType trackedJoint = KinectInterop.JointType.Pelvis;

        [Tooltip("Whether the joint view is mirrored or not.")]
        public bool mirroredView = false;

        [Tooltip("Smooth factor used for the joint orientation smoothing.")]
        public float smoothFactor = 5f;

        [Tooltip("UI-Text to display the current joint rotation.")]
        public UnityEngine.UI.Text debugText;

        private Quaternion initialRotation = Quaternion.identity;


        void Start()
        {
            initialRotation = transform.rotation;
            //transform.rotation = Quaternion.identity;
        }

        void Update()
        {
            KinectManager kinectManager = KinectManager.Instance;

            if (kinectManager && kinectManager.IsInitialized())
            {
                int iJointIndex = (int)trackedJoint;

                if (kinectManager.IsUserDetected(playerIndex))
                {
                    ulong userId = kinectManager.GetUserIdByIndex(playerIndex);

                    if (kinectManager.IsJointTracked(userId, iJointIndex))
                    {
                        Quaternion qRotObject = kinectManager.GetJointOrientation(userId, iJointIndex, !mirroredView);
                        qRotObject = initialRotation * qRotObject;

                        if (debugText)
                        {
                            Vector3 vRotAngles = qRotObject.eulerAngles;
                            debugText.text = string.Format("{0} - R({1:000}, {2:000}, {3:000})", trackedJoint,
                                                                   vRotAngles.x, vRotAngles.y, vRotAngles.z);
                        }

                        if (smoothFactor != 0f)
                            transform.rotation = Quaternion.Slerp(transform.rotation, qRotObject, smoothFactor * Time.deltaTime);
                        else
                            transform.rotation = qRotObject;
                    }

                }

            }
        }

    }
}
