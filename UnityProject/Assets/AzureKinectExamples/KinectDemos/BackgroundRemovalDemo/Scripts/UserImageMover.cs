using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// UserImageMover moves the BR image, according to the distance to the user.
    /// </summary>
    public class UserImageMover : MonoBehaviour
    {
        [Tooltip("Index of the player, tracked by this component. 0 - the 1st player, 1 - the 2nd player, etc.")]
        public int playerIndex = 0;

        [Tooltip("Smooth factor used for the transform movement.")]
        public float smoothFactor = 20f;


        private KinectManager kinectManager = null;
        private ulong lastUserId = 0;

        private ulong userId = 0;
        private Vector3 initialPlanePos = Vector3.zero;
        private Vector3 currentUserPos = Vector3.zero;


        void Start()
        {
            kinectManager = KinectManager.Instance;
            initialPlanePos = transform.position;
        }

        void Update()
        {
            if (kinectManager == null || !kinectManager.IsInitialized())
                return;

            userId = kinectManager.GetUserIdByIndex(playerIndex);
            currentUserPos = kinectManager.GetUserPosition(userId);

            if (userId != 0 && userId != lastUserId)
            {
                lastUserId = userId;
            }

            if (userId != 0)
            {
                Vector3 deltaUserPos = currentUserPos; // relToInitialPos ? (currentUserPos - initialUserPos) : currentUserPos;
                Vector3 newPlanePos = initialPlanePos + new Vector3(0f, 0f, deltaUserPos.z);

                transform.position = Vector3.Lerp(transform.position, newPlanePos, smoothFactor * Time.deltaTime);
            }
            else
            {
                lastUserId = 0;

                //gameObject.SetActive(false);
                transform.position = initialPlanePos;
                //gameObject.SetActive(true);
            }
        }

    }
}
