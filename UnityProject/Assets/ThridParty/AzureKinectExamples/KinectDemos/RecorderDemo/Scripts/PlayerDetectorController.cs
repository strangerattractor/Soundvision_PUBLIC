using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// PlayerDetectorController plays saved recording, when no user is detected for given amount of time.
    /// </summary>
    public class PlayerDetectorController : MonoBehaviour
    {
        [Tooltip("Depth sensor index - 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Tooltip("Delay in seconds when the replay starts, after no more users have been detected.")]
        public float userLostMaxTime = 5f;

        private KinectManager kinectManager = null;
        private BodyDataRecorderPlayer saverPlayer = null;
        private KinectInterop.SensorData sensorData = null;

        private float lastUserTime = 0f;


        void Start()
        {
            kinectManager = KinectManager.Instance;
            saverPlayer = BodyDataRecorderPlayer.Instance;

            sensorData = kinectManager != null ? kinectManager.GetSensorData(sensorIndex) : null;
        }

        void Update()
        {
            if (!kinectManager || !saverPlayer)
                return;

            // check if the body-data is playing
            bool bPlayerActive = saverPlayer.IsPlaying();

            if (sensorData != null)
            {
                // check for users while playing
                if (sensorData.trackedBodiesCount > 0)
                {
                    lastUserTime = Time.realtimeSinceStartup;
                }
            }

            bool bUserFound = (Time.realtimeSinceStartup - lastUserTime) < userLostMaxTime;

            if (!bPlayerActive && !bUserFound)
            {
                saverPlayer.StartPlaying();
            }
            else if (bPlayerActive && bUserFound)
            {
                saverPlayer.StopRecordingOrPlaying();
                kinectManager.ClearKinectUsers();
            }
        }

    }
}
