using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.rfilkov.kinect
{
    /// <summary>
    /// This interface needs to be implemented by all Kinect gesture listeners
    /// </summary>
    public interface GestureListenerInterface
    {
        /// <summary>
        /// Invoked when a new user is detected. Here you can start gesture tracking by invoking KinectManager.DetectGesture()-function.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        void UserDetected(ulong userId, int userIndex);

        /// <summary>
        /// Invoked when a user gets lost. All tracked gestures for this user are cleared automatically.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        void UserLost(ulong userId, int userIndex);

        /// <summary>
        /// Invoked when a gesture is in progress.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        /// <param name="gesture">Gesture type</param>
        /// <param name="progress">Gesture progress [0..1]</param>
        /// <param name="joint">Joint type</param>
        /// <param name="screenPos">Normalized viewport position</param>
        void GestureInProgress(ulong userId, int userIndex, GestureType gesture, float progress,
                               KinectInterop.JointType joint, Vector3 screenPos);

        /// <summary>
        /// Invoked if a gesture is completed.
        /// </summary>
        /// <returns><c>true</c>, if the gesture detection must be restarted, <c>false</c> otherwise.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        /// <param name="gesture">Gesture type</param>
        /// <param name="joint">Joint type</param>
        /// <param name="screenPos">Normalized viewport position</param>
        bool GestureCompleted(ulong userId, int userIndex, GestureType gesture,
                              KinectInterop.JointType joint, Vector3 screenPos);

        /// <summary>
        /// Invoked if a gesture is cancelled.
        /// </summary>
        /// <returns><c>true</c>, if the gesture detection must be retarted, <c>false</c> otherwise.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        /// <param name="gesture">Gesture type</param>
        /// <param name="joint">Joint type</param>
        bool GestureCancelled(ulong userId, int userIndex, GestureType gesture,
                              KinectInterop.JointType joint);
    }


    /// <summary>
    /// Kinect gesture types.
    /// </summary>
    public enum GestureType
    {
        None = 0,
        RaiseRightHand,
        RaiseLeftHand,
        Psi,
        Tpose,
        Stop,
        Wave,
        SwipeLeft,
        SwipeRight,
        SwipeUp,
        SwipeDown,
        ZoomIn,
        ZoomOut,
        Wheel,
        Jump,
        Squat,
        Push,
        Pull,
        ShoulderLeftFront,
        ShoulderRightFront,
        LeanLeft,
        LeanRight,
        LeanForward,
        LeanBack,
        KickLeft,
        KickRight,
        Run,

        RaisedRightHorizontalLeftHand,   // by Andrzej W
        RaisedLeftHorizontalRightHand,

        TouchRightElbow,   // suggested by Nayden N.
        TouchLeftElbow,

        UserGesture1 = 101,
        UserGesture2 = 102,
        UserGesture3 = 103,
        UserGesture4 = 104,
        UserGesture5 = 105,
        UserGesture6 = 106,
        UserGesture7 = 107,
        UserGesture8 = 108,
        UserGesture9 = 109,
        UserGesture10 = 110,
    }


    /// <summary>
    /// Kinect gesture manager is the component that tracks and processes the user gestures.
    /// </summary>
    public class KinectGestureManager : MonoBehaviour
    {

        /// <summary>
        /// Programmatic gesture data container.
        /// </summary>
        public struct GestureData
        {
            public ulong userId;
            public GestureType gesture;
            public int state;
            public float timestamp;
            public int joint;
            public Vector3 jointPos;
            public Vector3 screenPos;
            public float tagFloat;
            public Vector3 tagVector;
            public Vector3 tagVector2;
            public float progress;
            public bool complete;
            public bool cancelled;
            public List<GestureType> checkForGestures;
            public float startTrackingAtTime;
        }


        [Tooltip("Minimum time between gesture detections (in seconds).")]
        public float minTimeBetweenGestures = 0.7f;

        [Tooltip("List of the gesture listeners in the scene. If the list is empty, the available gesture listeners will be detected at the scene start up.")]
        public List<MonoBehaviour> gestureListeners = new List<MonoBehaviour>();

        [Tooltip("UI-Text to display the status of the currently tracked gestures.")]
        public UnityEngine.UI.Text gestureDebugText;


        // Gesture related constants, variables and functions
        protected int leftHandIndex;
        protected int rightHandIndex;

        protected int leftFingerIndex;
        protected int rightFingerIndex;

        protected int leftElbowIndex;
        protected int rightElbowIndex;

        protected int leftShoulderIndex;
        protected int rightShoulderIndex;

        protected int leftClavicleIndex;
        protected int rightClavicleIndex;

        protected int hipCenterIndex;
        protected int neckIndex;

        protected int leftHipIndex;
        protected int rightHipIndex;

        protected int leftKneeIndex;
        protected int rightKneeIndex;

        protected int leftAnkleIndex;
        protected int rightAnkleIndex;

        // gestures data and parameters
        protected Dictionary<ulong, List<KinectGestureManager.GestureData>> playerGesturesData = new Dictionary<ulong, List<KinectGestureManager.GestureData>>();
        protected Dictionary<ulong, float> gesturesTrackingAtTime = new Dictionary<ulong, float>();


        /// <summary>
        /// Adds a gesture to the list of detected gestures for the specified user.
        /// </summary>
        /// <param name="UserId">User ID</param>
        /// <param name="gesture">Gesture type</param>
        public void DetectGesture(ulong UserId, GestureType gesture)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : new List<GestureData>();
            int index = GetGestureIndex(gesture, ref gesturesData);

            if (index >= 0)
            {
                DeleteGesture(UserId, gesture);
            }

            GestureData gestureData = new GestureData();

            gestureData.userId = UserId;
            gestureData.gesture = gesture;
            gestureData.state = 0;
            gestureData.joint = 0;
            gestureData.progress = 0f;
            gestureData.complete = false;
            gestureData.cancelled = false;

            gestureData.checkForGestures = new List<GestureType>();
            switch (gesture)
            {
                case GestureType.ZoomIn:
                    gestureData.checkForGestures.Add(GestureType.ZoomOut);
                    gestureData.checkForGestures.Add(GestureType.Wheel);
                    break;

                case GestureType.ZoomOut:
                    gestureData.checkForGestures.Add(GestureType.ZoomIn);
                    gestureData.checkForGestures.Add(GestureType.Wheel);
                    break;

                case GestureType.Wheel:
                    gestureData.checkForGestures.Add(GestureType.ZoomIn);
                    gestureData.checkForGestures.Add(GestureType.ZoomOut);
                    break;
            }

            gesturesData.Add(gestureData);
            playerGesturesData[UserId] = gesturesData;

            if (!gesturesTrackingAtTime.ContainsKey(UserId))
            {
                gesturesTrackingAtTime[UserId] = 0f;
            }
        }

        /// <summary>
        /// Resets the gesture state for the given gesture of the specified user.
        /// </summary>
        /// <returns><c>true</c>, if gesture was reset, <c>false</c> otherwise.</returns>
        /// <param name="UserId">User ID</param>
        /// <param name="gesture">Gesture type</param>
        public bool ResetGesture(ulong UserId, GestureType gesture)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;
            int index = gesturesData != null ? GetGestureIndex(gesture, ref gesturesData) : -1;
            if (index < 0)
                return false;

            GestureData gestureData = gesturesData[index];

            gestureData.state = 0;
            gestureData.joint = 0;
            gestureData.progress = 0f;
            gestureData.complete = false;
            gestureData.cancelled = false;
            gestureData.startTrackingAtTime = Time.realtimeSinceStartup + KinectInterop.Constants.MinTimeBetweenSameGestures;

            gesturesData[index] = gestureData;
            playerGesturesData[UserId] = gesturesData;

            return true;
        }

        /// <summary>
        /// Resets the gesture states for all gestures of the specified user.
        /// </summary>
        /// <param name="UserId">User ID</param>
        public void ResetPlayerGestures(ulong UserId)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;

            if (gesturesData != null)
            {
                int listSize = gesturesData.Count;

                for (int i = 0; i < listSize; i++)
                {
                    ResetGesture(UserId, gesturesData[i].gesture);
                }
            }
        }

        /// <summary>
        /// Deletes the gesture for the specified user.
        /// </summary>
        /// <returns><c>true</c>, if gesture was deleted, <c>false</c> otherwise.</returns>
        /// <param name="UserId">User ID</param>
        /// <param name="gesture">Gesture type</param>
        public bool DeleteGesture(ulong UserId, GestureType gesture)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;
            int index = gesturesData != null ? GetGestureIndex(gesture, ref gesturesData) : -1;
            if (index < 0)
                return false;

            gesturesData.RemoveAt(index);
            playerGesturesData[UserId] = gesturesData;

            return true;
        }

        /// <summary>
        /// Deletes all gestures for the specified user.
        /// </summary>
        /// <param name="UserId">User ID</param>
        public void ClearUserGestures(ulong UserId)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;

            if (gesturesData != null)
            {
                gesturesData.Clear();
                playerGesturesData[UserId] = gesturesData;
            }
        }

        /// <summary>
        /// Gets the list of gestures for the specified user.
        /// </summary>
        /// <returns>The gestures list.</returns>
        /// <param name="UserId">User ID</param>
        public List<GestureType> GetGesturesList(ulong UserId)
        {
            List<GestureType> list = new List<GestureType>();
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;

            if (gesturesData != null)
            {
                foreach (GestureData data in gesturesData)
                    list.Add(data.gesture);
            }

            return list;
        }

        /// <summary>
        /// Gets the gestures count for the specified user.
        /// </summary>
        /// <returns>The gestures count.</returns>
        /// <param name="UserId">User ID</param>
        public int GetGesturesCount(ulong UserId)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;

            if (gesturesData != null)
            {
                return gesturesData.Count;
            }

            return 0;
        }

        /// <summary>
        /// Gets the gesture at the specified index for the given user.
        /// </summary>
        /// <returns>The gesture at specified index.</returns>
        /// <param name="UserId">User ID</param>
        /// <param name="i">Index</param>
        public GestureType GetGestureAtIndex(ulong UserId, int i)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;

            if (gesturesData != null)
            {
                if (i >= 0 && i < gesturesData.Count)
                {
                    return gesturesData[i].gesture;
                }
            }

            return GestureType.None;
        }

        /// <summary>
        /// Determines whether the given gesture is in the list of gestures for the specified user.
        /// </summary>
        /// <returns><c>true</c> if the gesture is in the list of gestures for the specified user; otherwise, <c>false</c>.</returns>
        /// <param name="UserId">User ID</param>
        /// <param name="gesture">Gesture type</param>
        public bool IsTrackingGesture(ulong UserId, GestureType gesture)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;
            int index = gesturesData != null ? GetGestureIndex(gesture, ref gesturesData) : -1;

            return index >= 0;
        }

        /// <summary>
        /// Determines whether the given gesture for the specified user is complete.
        /// </summary>
        /// <returns><c>true</c> if the gesture is complete; otherwise, <c>false</c>.</returns>
        /// <param name="UserId">User ID</param>
        /// <param name="gesture">Gesture type</param>
        /// <param name="bResetOnComplete">If set to <c>true</c>, resets the gesture state.</param>
        public bool IsGestureComplete(ulong UserId, GestureType gesture, bool bResetOnComplete)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;
            int index = gesturesData != null ? GetGestureIndex(gesture, ref gesturesData) : -1;

            if (index >= 0)
            {
                GestureData gestureData = gesturesData[index];

                if (bResetOnComplete && gestureData.complete)
                {
                    ResetPlayerGestures(UserId);
                    return true;
                }

                return gestureData.complete;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the given gesture for the specified user is canceled.
        /// </summary>
        /// <returns><c>true</c> if the gesture is canceled; otherwise, <c>false</c>.</returns>
        /// <param name="UserId">User ID</param>
        /// <param name="gesture">Gesture type</param>
        public bool IsGestureCancelled(ulong UserId, GestureType gesture)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;
            int index = gesturesData != null ? GetGestureIndex(gesture, ref gesturesData) : -1;

            if (index >= 0)
            {
                GestureData gestureData = gesturesData[index];
                return gestureData.cancelled;
            }

            return false;
        }

        /// <summary>
        /// Gets the progress (in range [0, 1]) of the given gesture for the specified user.
        /// </summary>
        /// <returns>The gesture progress.</returns>
        /// <param name="UserId">User ID</param>
        /// <param name="gesture">Gesture type</param>
        public float GetGestureProgress(ulong UserId, GestureType gesture)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;
            int index = gesturesData != null ? GetGestureIndex(gesture, ref gesturesData) : -1;

            if (index >= 0)
            {
                GestureData gestureData = gesturesData[index];
                return gestureData.progress;
            }

            return 0f;
        }

        /// <summary>
        /// Gets the normalized screen position of the given gesture for the specified user.
        /// </summary>
        /// <returns>The normalized screen position.</returns>
        /// <param name="UserId">User ID</param>
        /// <param name="gesture">Gesture type</param>
        public Vector3 GetGestureScreenPos(ulong UserId, GestureType gesture)
        {
            List<GestureData> gesturesData = playerGesturesData.ContainsKey(UserId) ? playerGesturesData[UserId] : null;
            int index = gesturesData != null ? GetGestureIndex(gesture, ref gesturesData) : -1;

            if (index >= 0)
            {
                GestureData gestureData = gesturesData[index];
                return gestureData.screenPos;
            }

            return Vector3.zero;
        }


        /// <summary>
        /// Locate the available gesture listeners.
        /// </summary>
        public void RefreshGestureListeners()
        {
            gestureListeners.Clear();

            MonoBehaviour[] monoScripts = FindObjectsOfType<MonoBehaviour>() as MonoBehaviour[];
            foreach (MonoBehaviour monoScript in monoScripts)
            {
                if ((monoScript is GestureListenerInterface) && monoScript.enabled)
                {
                    //GestureListenerInterface gl = (GestureListenerInterface)monoScript;
                    gestureListeners.Add(monoScript);
                }
            }
        }


        /// <summary>
        /// Invoked when a new user gets detected.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        public void UserWasAdded(ulong userId, int userIndex)
        {
            //Debug.Log("GM - UserAdded: " + userId);

            //// add the gestures to be detected by all users, if any
            //foreach (GestureType gesture in playerCommonGestures)
            //{
            //    DetectGesture(userId, gesture);
            //}

            // notify all gesture listeners of the newly detected user
            foreach (GestureListenerInterface listener in gestureListeners)
            {
                if (listener != null)
                {
                    listener.UserDetected(userId, userIndex);
                }
            }
        }


        /// <summary>
        /// Invoked when user was removed.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        public void UserWasRemoved(ulong userId, int userIndex)
        {
            //Debug.Log("GM - UserRemoved: " + userId);

            // notify all gesture listeners for losing this user
            foreach (GestureListenerInterface listener in gestureListeners)
            {
                if (listener != null)
                {
                    listener.UserLost(userId, userIndex);
                }
            }

            // clear the user gestures
            ClearUserGestures(userId);
        }


        /// <summary>
        /// Updates the progress of the given user's gestures. 
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="kinectManager">Reference to the KinectManager</param>
        public void UpdateUserGestures(ulong userId, KinectManager kinectManager)
        {
            if (!playerGesturesData.ContainsKey(userId))
                return;

            //Debug.Log("GM - UpdateGestures for user: " + userId);

            // Check for player's gestures
            CheckForGestures(userId, kinectManager);

            // Check for complete gestures
            List<GestureData> gesturesData = playerGesturesData[userId];
            int userIndex = kinectManager.GetUserIndexById(userId);

            for (int g = 0; g < gesturesData.Count; g++)
            {
                GestureData gestureData = gesturesData[g];

                if (gestureData.complete)
                {
                    foreach (GestureListenerInterface listener in gestureListeners)
                    {
                        if (listener != null && listener.GestureCompleted(userId, userIndex, gestureData.gesture, (KinectInterop.JointType)gestureData.joint, gestureData.screenPos))
                        {
                            ResetPlayerGestures(userId);
                        }
                    }
                }
                else if (gestureData.cancelled)
                {
                    foreach (GestureListenerInterface listener in gestureListeners)
                    {
                        if (listener != null && listener.GestureCancelled(userId, userIndex, gestureData.gesture, (KinectInterop.JointType)gestureData.joint))
                        {
                            ResetGesture(userId, gestureData.gesture);
                        }
                    }
                }
                else if (gestureData.progress >= 0.1f)
                {
                    foreach (GestureListenerInterface listener in gestureListeners)
                    {
                        if (listener != null)
                        {
                            listener.GestureInProgress(userId, userIndex, gestureData.gesture, gestureData.progress,
                                                       (KinectInterop.JointType)gestureData.joint, gestureData.screenPos);
                        }
                    }
                }

                //gesturesData[g] = gestureData;
            }
        }


        protected void Start()
        {
            // locate the available gesture listeners
            RefreshGestureListeners();
        }


        // Estimates the current state of the defined gestures
        protected void CheckForGestures(ulong UserId, KinectManager kinectManager)
        {
            if (!playerGesturesData.ContainsKey(UserId) || !gesturesTrackingAtTime.ContainsKey(UserId))
                return;

            // check for gestures
            if (Time.realtimeSinceStartup >= gesturesTrackingAtTime[UserId])
            {
                // get joint positions and tracking
                int iAllJointsCount = (int)KinectInterop.JointType.Count;
                bool[] playerJointsTracked = new bool[iAllJointsCount];
                Vector3[] playerJointsPos = new Vector3[iAllJointsCount];

                int[] aiNeededJointIndexes = GetNeededJointIndexes();
                int iNeededJointsCount = aiNeededJointIndexes.Length;

                for (int i = 0; i < iNeededJointsCount; i++)
                {
                    int joint = aiNeededJointIndexes[i];

                    if (joint >= 0)
                    {
                        playerJointsTracked[joint] = kinectManager.IsJointTracked(UserId, joint);
                        playerJointsPos[joint] = kinectManager.GetJointPosition(UserId, joint);
                    }
                }

                // check for gestures
                List<GestureData> gesturesData = playerGesturesData[UserId];

                int listGestureSize = gesturesData.Count;
                float timestampNow = Time.realtimeSinceStartup;
                string sDebugGestures = string.Empty;  // "Tracked Gestures:\n";

                for (int g = 0; g < listGestureSize; g++)
                {
                    GestureData gestureData = gesturesData[g];

                    if ((timestampNow >= gestureData.startTrackingAtTime) &&
                        !IsConflictingGestureInProgress(gestureData, ref gesturesData))
                    {
                        CheckForGesture(UserId, ref gestureData, Time.realtimeSinceStartup, ref playerJointsPos, ref playerJointsTracked);
                        gesturesData[g] = gestureData;

                        if (gestureData.complete)
                        {
                            gesturesTrackingAtTime[UserId] = timestampNow + minTimeBetweenGestures;
                        }

                        if (UserId == kinectManager.GetPrimaryUserID())
                        {
                            sDebugGestures += string.Format("{0} - state: {1}, time: {2:F1}, progress: {3}%\n",
                                                            gestureData.gesture, gestureData.state,
                                                            gestureData.timestamp,
                                                            (int)(gestureData.progress * 100 + 0.5f));
                        }
                    }
                }

                playerGesturesData[UserId] = gesturesData;

                if (gestureDebugText && (UserId == kinectManager.GetPrimaryUserID()))
                {
                    for (int i = 0; i < iNeededJointsCount; i++)
                    {
                        int joint = aiNeededJointIndexes[i];

                        sDebugGestures += string.Format("\n {0}: {1}", (KinectInterop.JointType)joint,
                                                        playerJointsTracked[joint] ? playerJointsPos[joint].ToString() : "");
                    }

                    gestureDebugText.text = sDebugGestures;
                }
            }
        }


        private bool IsConflictingGestureInProgress(GestureData gestureData, ref List<GestureData> gesturesData)
        {
            foreach (GestureType gesture in gestureData.checkForGestures)
            {
                int index = GetGestureIndex(gesture, ref gesturesData);

                if (index >= 0)
                {
                    if (gesturesData[index].progress > 0f)
                        return true;
                }
            }

            return false;
        }


        // return the index of gesture in the list, or -1 if not found
        private int GetGestureIndex(GestureType gesture, ref List<GestureData> gesturesData)
        {
            int listSize = gesturesData.Count;

            for (int i = 0; i < listSize; i++)
            {
                if (gesturesData[i].gesture == gesture)
                    return i;
            }

            return -1;
        }


        /// <summary>
        /// Gets the list of gesture joint indexes.
        /// </summary>
        /// <returns>The needed joint indexes.</returns>
        public virtual int[] GetNeededJointIndexes()
        {
            leftHandIndex = (int)KinectInterop.JointType.HandLeft;
            rightHandIndex = (int)KinectInterop.JointType.HandRight;

            leftFingerIndex = (int)KinectInterop.JointType.HandLeft;
            rightFingerIndex = (int)KinectInterop.JointType.HandRight;

            leftElbowIndex = (int)KinectInterop.JointType.ElbowLeft;
            rightElbowIndex = (int)KinectInterop.JointType.ElbowRight;

            leftShoulderIndex = (int)KinectInterop.JointType.ShoulderLeft;
            rightShoulderIndex = (int)KinectInterop.JointType.ShoulderRight;

            leftClavicleIndex = (int)KinectInterop.JointType.ClavicleLeft;
            rightClavicleIndex = (int)KinectInterop.JointType.ClavicleRight;

            hipCenterIndex = (int)KinectInterop.JointType.Pelvis;
            neckIndex = (int)KinectInterop.JointType.Neck;

            leftHipIndex = (int)KinectInterop.JointType.HipLeft;
            rightHipIndex = (int)KinectInterop.JointType.HipRight;

            leftKneeIndex = (int)KinectInterop.JointType.KneeLeft;
            rightKneeIndex = (int)KinectInterop.JointType.KneeRight;

            leftAnkleIndex = (int)KinectInterop.JointType.AnkleLeft;
            rightAnkleIndex = (int)KinectInterop.JointType.AnkleRight;

            int[] neededJointIndexes = {
                leftHandIndex, rightHandIndex, leftFingerIndex, rightFingerIndex, leftElbowIndex, rightElbowIndex, leftShoulderIndex, rightShoulderIndex,
                leftClavicleIndex, rightClavicleIndex, hipCenterIndex, neckIndex,
                leftHipIndex, rightHipIndex, leftKneeIndex, rightKneeIndex, leftAnkleIndex, rightAnkleIndex
            };

            return neededJointIndexes;
        }


        // sets basic parameters of the gesture data
        protected void SetGestureJoint(ref GestureData gestureData, float timestamp, int joint, Vector3 jointPos)
        {
            gestureData.joint = joint;
            gestureData.jointPos = jointPos;
            gestureData.timestamp = timestamp;
            gestureData.state++;
        }


        // marks the gesture data as cancelled gesture
        protected void SetGestureCancelled(ref GestureData gestureData)
        {
            gestureData.state = 0;
            gestureData.progress = 0f;
            gestureData.cancelled = true;
        }


        // checks if the pose persists for the whole duration set
        protected void CheckPoseComplete(ref GestureData gestureData, float timestamp, Vector3 jointPos, bool isInPose, float durationToComplete)
        {
            if (isInPose)
            {
                float timeLeft = timestamp - gestureData.timestamp;
                gestureData.progress = durationToComplete > 0f ? Mathf.Clamp01(timeLeft / durationToComplete) : 1.0f;

                if (timeLeft >= durationToComplete)
                {
                    gestureData.timestamp = timestamp;
                    gestureData.jointPos = jointPos;
                    gestureData.state++;
                    gestureData.complete = true;
                }
            }
            else
            {
                SetGestureCancelled(ref gestureData);
            }
        }


        // sets gesture data current screen coords (useful for many gestures)
        protected void SetScreenPos(ulong userId, ref GestureData gestureData, ref Vector3[] jointsPos, ref bool[] jointsTracked)
        {
            Vector3 handPos = jointsPos[rightHandIndex];
            bool calculateCoords = false;

            if (gestureData.joint == rightHandIndex)
            {
                if (jointsTracked[rightHandIndex] /**&& jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex]*/)
                {
                    calculateCoords = true;
                }
            }
            else if (gestureData.joint == leftHandIndex)
            {
                if (jointsTracked[leftHandIndex] /**&& jointsTracked[leftElbowIndex] && jointsTracked[leftShoulderIndex]*/)
                {
                    handPos = jointsPos[leftHandIndex];
                    calculateCoords = true;
                }
            }

            if (calculateCoords)
            {
                if (jointsTracked[hipCenterIndex] && jointsTracked[leftClavicleIndex] && jointsTracked[rightClavicleIndex] &&
                    jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex])
                {
                    Vector3 shoulderCenterPos = (jointsPos[leftClavicleIndex] + jointsPos[rightClavicleIndex]) / 2f;
                    Vector3 shoulderToHips = shoulderCenterPos - jointsPos[hipCenterIndex];
                    Vector3 rightToLeft = jointsPos[rightShoulderIndex] - jointsPos[leftShoulderIndex];

                    gestureData.tagVector2.x = rightToLeft.x; // * 1.2f;
                    gestureData.tagVector2.y = shoulderToHips.y; // * 1.2f;

                    if (gestureData.joint == rightHandIndex)
                    {
                        gestureData.tagVector.x = jointsPos[rightShoulderIndex].x - gestureData.tagVector2.x / 2f;
                        gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
                    }
                    else
                    {
                        gestureData.tagVector.x = jointsPos[leftShoulderIndex].x - gestureData.tagVector2.x / 2f;
                        gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
                    }
                }

                if (gestureData.tagVector2.x != 0 && gestureData.tagVector2.y != 0)
                {
                    Vector3 relHandPos = handPos - gestureData.tagVector;
                    gestureData.screenPos.x = Mathf.Clamp01(relHandPos.x / gestureData.tagVector2.x);
                    gestureData.screenPos.y = Mathf.Clamp01(relHandPos.y / gestureData.tagVector2.y);
                }

            }
        }


        // sets the zoom factor value as screenPos.z (for zoom-in and zoom-out gestures)
        protected void SetZoomFactor(ulong userId, ref GestureData gestureData, float initialZoom, ref Vector3[] jointsPos, ref bool[] jointsTracked)
        {
            Vector3 vectorZooming = jointsPos[rightHandIndex] - jointsPos[leftHandIndex];

            if (gestureData.tagFloat == 0f || gestureData.userId != userId)
            {
                gestureData.tagFloat = 0.5f; // this is 100%
            }

            float distZooming = vectorZooming.magnitude;
            gestureData.screenPos.z = initialZoom + (distZooming / gestureData.tagFloat);
        }


        // sets the wheel rotation value as screenPos.z (for wheel-gesture)
        protected void SetWheelRotation(ulong userId, ref GestureData gestureData, Vector3 initialPos, Vector3 currentPos)
        {
            float angle = Vector3.Angle(initialPos, currentPos) * Mathf.Sign(currentPos.y - initialPos.y);
            gestureData.screenPos.z = angle;
        }


        /// <summary>
        /// Estimate the state and progress of the given gesture.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="gestureData">Gesture-data structure</param>
        /// <param name="timestamp">Current time</param>
        /// <param name="jointsPos">Joints-position array</param>
        /// <param name="jointsTracked">Joints-tracked array</param>
        public virtual void CheckForGesture(ulong userId, ref GestureData gestureData, float timestamp, ref Vector3[] jointsPos, ref bool[] jointsTracked)
        {
            if (gestureData.complete)
                return;

            float bandTopY = jointsPos[rightShoulderIndex].y > jointsPos[leftShoulderIndex].y ? jointsPos[rightShoulderIndex].y : jointsPos[leftShoulderIndex].y;
            float bandBotY = jointsPos[rightHipIndex].y < jointsPos[leftHipIndex].y ? jointsPos[rightHipIndex].y : jointsPos[leftHipIndex].y;

            float bandCenter = (bandTopY + bandBotY) / 2f;
            float bandSize = (bandTopY - bandBotY);

            float gestureTop = bandCenter + bandSize * 1.2f / 2f;
            float gestureBottom = bandCenter - bandSize * 1.3f / 4f;
            float gestureRight = jointsPos[rightHipIndex].x;
            float gestureLeft = jointsPos[leftHipIndex].x;

            switch (gestureData.gesture)
            {
                // check for RaiseRightHand
                case GestureType.RaiseRightHand:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection
                            if (jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f &&
                                   (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            }
                            break;

                        case 1:  // gesture complete
                            bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0f;

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                            break;
                    }
                    break;

                // check for RaiseLeftHand
                case GestureType.RaiseLeftHand:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection
                            if (jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
                                   (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            }
                            break;

                        case 1:  // gesture complete
                            bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
                                (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0f;

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                            break;
                    }
                    break;

                // check for Psi
                case GestureType.Psi:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection
                            if (jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftClavicleIndex] && jointsTracked[rightClavicleIndex] &&
                               (jointsPos[rightHandIndex].y - jointsPos[rightClavicleIndex].y) > 0.1f &&
                               (jointsPos[leftHandIndex].y - jointsPos[leftClavicleIndex].y) > 0.1f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            }
                            break;

                        case 1:  // gesture complete
                            bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftClavicleIndex] && jointsTracked[rightClavicleIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[rightClavicleIndex].y) > 0.1f &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftClavicleIndex].y) > 0.1f;

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                            break;
                    }
                    break;

                // check for Tpose
                case GestureType.Tpose:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection
                            if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] &&
                                Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.07f
                                Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) < 0.1f &&  // 0.7f
                                jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                Mathf.Abs(jointsPos[leftElbowIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f &&
                                Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) < 0.1f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            }
                            break;

                        case 1:  // gesture complete
                            bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] &&
                                Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.7f
                                Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) < 0.1f &&  // 0.7f
                                jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                Mathf.Abs(jointsPos[leftElbowIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f &&
                                Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) < 0.1f;

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                            break;
                    }
                    break;

                // check for Stop
                case GestureType.Stop:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection
                            if (jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
                               (jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) < 0.2f &&
                                  (jointsPos[rightHandIndex].x - jointsPos[rightHipIndex].x) >= 0.4f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            }
                            else if (jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
                               (jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) < 0.2f &&
                               (jointsPos[leftHandIndex].x - jointsPos[leftHipIndex].x) <= -0.4f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            }
                            break;

                        case 1:  // gesture complete
                            bool isInPose = (gestureData.joint == rightHandIndex) ?
                                (jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
                                (jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) < 0.2f &&
                                 (jointsPos[rightHandIndex].x - jointsPos[rightHipIndex].x) >= 0.4f) :
                                (jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
                                (jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) < 0.2f &&
                                 (jointsPos[leftHandIndex].x - jointsPos[leftHipIndex].x) <= -0.4f);

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                            break;
                    }
                    break;

                // check for raised right hand & horizontal left hand 
                case GestureType.RaisedRightHorizontalLeftHand:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection
                            if (jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] && // check right hand is straight up 
                               (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.5f &&  // ensure right hand is higher than shoulder 
                               Mathf.Abs(jointsPos[rightHandIndex].z - jointsPos[rightShoulderIndex].z) < 0.35f &&   // ensue hand is vertical straight enough 
                               Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.35f &&   // ensue hand is vertical straight enough                             
                               jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&  // check left hand is straight flat 
                               Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0.25f &&       // ensure hand and shoulder are on close height 
                               (jointsPos[leftHandIndex] - jointsPos[leftShoulderIndex]).sqrMagnitude > 0.25f)  // ensure hand and shoulder are horizontal straight enough 						
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            }
                            break;


                        case 1:  // gesture complete
                            bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] && // check right hand is straight up 
                               (jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.5f &&  // ensure right hand is higher than shoulder 
                               Mathf.Abs(jointsPos[rightHandIndex].z - jointsPos[rightShoulderIndex].z) < 0.35f &&   // ensue hand is vertical straight enough 
                               Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.35f &&   // ensue hand is vertical straight enough                             
                               jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&  // check left hand is straight flat 
                               Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0.25f &&       // ensure hand and shoulder are on close height 
                               (jointsPos[leftHandIndex] - jointsPos[leftShoulderIndex]).sqrMagnitude > 0.25f;  // ensure hand and shoulder are horizontal straight enough

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                            break;
                    }
                    break;

                // check for raised left hand & horizontal right hand 
                case GestureType.RaisedLeftHorizontalRightHand:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection
                            if (jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] && // check left hand is straight up 
                               (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.5f &&  // ensure left hand is higher than shoulder 
                               Mathf.Abs(jointsPos[leftHandIndex].z - jointsPos[leftShoulderIndex].z) < 0.35f &&   // ensue hand is vertical straight enough 
                               Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.35f &&   // ensue hand is vertical straight enough                             
                               jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&  // check right hand is straight flat 
                               Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0.25f &&       // ensure hand and shoulder are on close height 
                               (jointsPos[rightHandIndex] - jointsPos[rightShoulderIndex]).sqrMagnitude > 0.25f)  // ensure hand and shoulder are horizontal straight enough 
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            }
                            break;

                        case 1:  // gesture complete
                            bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] && // check left hand is straight up 
                               (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.5f &&  // ensure left hand is higher than shoulder 
                               Mathf.Abs(jointsPos[leftHandIndex].z - jointsPos[leftShoulderIndex].z) < 0.35f &&   // ensue hand is vertical straight enough 
                               Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.35f &&   // ensue hand is vertical straight enough                             
                               jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&  // check right hand is straight flat 
                               Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0.25f &&       // ensure hand and shoulder are on close height 
                               (jointsPos[rightHandIndex] - jointsPos[rightShoulderIndex]).sqrMagnitude > 0.25f;  // ensure hand and shoulder are horizontal straight enough

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                            break;
                    }
                    break;

                // check for TouchedRightElbow
                case GestureType.TouchRightElbow:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection
                            if (jointsTracked[leftFingerIndex] && jointsTracked[rightElbowIndex] &&
                                Vector3.Distance(jointsPos[leftFingerIndex], jointsPos[rightElbowIndex]) <= 0.12f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftFingerIndex, jointsPos[leftFingerIndex]);
                            }

                            //Debug.Log ("TRE0 - Distance: " + Vector3.Distance(jointsPos[leftFingerIndex], jointsPos[rightElbowIndex]));
                            break;

                        case 1:  // gesture complete
                            bool isInPose = jointsTracked[leftFingerIndex] && jointsTracked[rightElbowIndex] &&
                                Vector3.Distance(jointsPos[leftFingerIndex], jointsPos[rightElbowIndex]) <= 0.12f;

                            //Debug.Log ("TRE1 - Distance: " + Vector3.Distance(jointsPos[leftFingerIndex], jointsPos[rightElbowIndex]) + ", progress: " + gestureData.progress);

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 1.5f /**KinectInterop.Constants.PoseCompleteDuration*/);
                            break;
                    }
                    break;

                // check for TouchedLeftElbow
                case GestureType.TouchLeftElbow:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection
                            if (jointsTracked[rightFingerIndex] && jointsTracked[leftElbowIndex] &&
                                Vector3.Distance(jointsPos[rightFingerIndex], jointsPos[leftElbowIndex]) <= 0.12f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightFingerIndex, jointsPos[rightFingerIndex]);
                            }

                            //Debug.Log ("TLE0 - Distance: " + Vector3.Distance(jointsPos[rightFingerIndex], jointsPos[leftElbowIndex]));
                            break;

                        case 1:  // gesture complete
                            bool isInPose = jointsTracked[rightFingerIndex] && jointsTracked[leftElbowIndex] &&
                                Vector3.Distance(jointsPos[rightFingerIndex], jointsPos[leftElbowIndex]) <= 0.12f;

                            //Debug.Log ("TLE1- Distance: " + Vector3.Distance(jointsPos[rightFingerIndex], jointsPos[leftElbowIndex]));

                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 1.5f /**KinectInterop.Constants.PoseCompleteDuration*/);
                            break;
                    }
                    break;

                // check for Wave
                case GestureType.Wave:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
                               (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f &&
                               (jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) > 0.05f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                                gestureData.progress = 0.3f;
                            }
                            else if (jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
                                    (jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) < -0.05f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                                gestureData.progress = 0.3f;
                            }
                            break;

                        case 1:  // gesture - phase 2
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = gestureData.joint == rightHandIndex ?
                                    jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
                                    (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f &&
                                    (jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) < -0.05f :
                                    jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
                                    (jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) > 0.05f;

                                if (isInPose)
                                {
                                    gestureData.timestamp = timestamp;
                                    gestureData.state++;
                                    gestureData.progress = 0.7f;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;

                        case 2:  // gesture phase 3 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = gestureData.joint == rightHandIndex ?
                                    jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
                                    (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f &&
                                    (jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) > 0.05f :
                                    jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
                                    (jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) < -0.05f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for SwipeLeft
                case GestureType.SwipeLeft:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                   jointsPos[rightHandIndex].x >= gestureRight /**&& jointsPos[rightHandIndex].x > gestureLeft*/)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                                gestureData.progress = 0.1f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) <= 1.0f)
                            {
                                bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                        jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                        jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                        jointsPos[rightHandIndex].x <= gestureLeft;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                                else if (jointsPos[rightHandIndex].x <= gestureRight)
                                {
                                    float gestureSize = gestureRight - gestureLeft;
                                    gestureData.progress = gestureSize > 0.01f ? (gestureRight - jointsPos[rightHandIndex].x) / gestureSize : 0f;
                                }

                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for SwipeRight
                case GestureType.SwipeRight:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                   jointsPos[leftHandIndex].x <= gestureLeft /**&& jointsPos[leftHandIndex].x < gestureRight*/)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                                gestureData.progress = 0.1f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) <= 1.0f)
                            {
                                bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                        jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                        jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                        jointsPos[leftHandIndex].x >= gestureRight;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                                else if (jointsPos[leftHandIndex].x >= gestureLeft)
                                {
                                    float gestureSize = gestureRight - gestureLeft;
                                    gestureData.progress = gestureSize > 0.01f ? (jointsPos[leftHandIndex].x - gestureLeft) / gestureSize : 0f;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for SwipeUp
                case GestureType.SwipeUp:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] &&
                               (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) < 0f &&
                               (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.15f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                                gestureData.progress = 0.5f;
                            }
                            else if (jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) < 0f &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.15f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = gestureData.joint == rightHandIndex ?
                                    jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] &&
                                    (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > -0.05f &&
                                    Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) <= 0.15f :
                                    jointsTracked[leftHandIndex] && jointsTracked[rightShoulderIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > -0.05f &&
                                    Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) <= 0.15f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for SwipeDown
                case GestureType.SwipeDown:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] &&
                               (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) >= 0.05f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                                gestureData.progress = 0.5f;
                            }
                            else if (jointsTracked[leftHandIndex] && jointsTracked[rightShoulderIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) >= 0.05f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = gestureData.joint == rightHandIndex ?
                                    jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] &&
                                    (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) < -0.15f &&
                                    Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) <= 0.15f :
                                    jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) < -0.15f &&
                                    Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) <= 0.15f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for ZoomIn
                case GestureType.ZoomIn:
                    Vector3 vectorZoomOut = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
                    float distZoomOut = vectorZoomOut.magnitude;

                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                    jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                    jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                    jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                    distZoomOut < 0.3f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                                gestureData.tagVector = Vector3.right;
                                gestureData.tagFloat = 0f;
                                gestureData.progress = 0.3f;
                            }
                            break;

                        case 1:  // gesture phase 2 = zooming
                            if ((timestamp - gestureData.timestamp) < 1.0f)
                            {
                                float angleZoomOut = Vector3.Angle(gestureData.tagVector, vectorZoomOut) * Mathf.Sign(vectorZoomOut.y - gestureData.tagVector.y);
                                bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                        jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                        jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                        jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                        distZoomOut < 1.5f && Mathf.Abs(angleZoomOut) < 20f;

                                if (isInPose)
                                {
                                    SetZoomFactor(userId, ref gestureData, 1.0f, ref jointsPos, ref jointsTracked);
                                    gestureData.timestamp = timestamp;
                                    gestureData.progress = 0.7f;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for ZoomOut
                case GestureType.ZoomOut:
                    Vector3 vectorZoomIn = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
                    float distZoomIn = vectorZoomIn.magnitude;

                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                distZoomIn >= 0.7f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                                gestureData.tagVector = Vector3.right;
                                gestureData.tagFloat = distZoomIn;
                                gestureData.progress = 0.3f;
                            }
                            break;

                        case 1:  // gesture phase 2 = zooming
                            if ((timestamp - gestureData.timestamp) < 1.0f)
                            {
                                float angleZoomIn = Vector3.Angle(gestureData.tagVector, vectorZoomIn) * Mathf.Sign(vectorZoomIn.y - gestureData.tagVector.y);
                                bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                        jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                        jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                        jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                        distZoomIn >= 0.2f && Mathf.Abs(angleZoomIn) < 20f;

                                if (isInPose)
                                {
                                    SetZoomFactor(userId, ref gestureData, 0.0f, ref jointsPos, ref jointsTracked);
                                    gestureData.timestamp = timestamp;
                                    gestureData.progress = 0.7f;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for Wheel
                case GestureType.Wheel:
                    Vector3 vectorWheel = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
                    float distWheel = vectorWheel.magnitude;

                    //				Debug.Log(string.Format("{0}. Dist: {1:F1}, Tag: {2:F1}, Diff: {3:F1}", gestureData.state,
                    //				                        distWheel, gestureData.tagFloat, Mathf.Abs(distWheel - gestureData.tagFloat)));

                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                distWheel >= 0.3f && distWheel < 0.7f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                                gestureData.tagVector = Vector3.right;
                                gestureData.tagFloat = distWheel;
                                gestureData.progress = 0.3f;
                            }
                            break;

                        case 1:  // gesture phase 2 = rotating
                            if ((timestamp - gestureData.timestamp) < 0.5f)
                            {
                                float angle = Vector3.Angle(gestureData.tagVector, vectorWheel) * Mathf.Sign(vectorWheel.y - gestureData.tagVector.y);
                                bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && 
                                    jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
                                    jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
                                    jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
                                    distWheel >= 0.3f && distWheel < 0.7f &&
                                    Mathf.Abs(distWheel - gestureData.tagFloat) < 0.1f;

                                if (isInPose)
                                {
                                    //SetWheelRotation(userId, ref gestureData, gestureData.tagVector, vectorWheel);
                                    gestureData.screenPos.z = angle;  // wheel angle
                                    gestureData.timestamp = timestamp;
                                    gestureData.tagFloat = distWheel;
                                    gestureData.progress = 0.7f;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for Jump
                case GestureType.Jump:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[hipCenterIndex] &&
                                (jointsPos[hipCenterIndex].y > 0.6f) && (jointsPos[hipCenterIndex].y < 1.2f))
                            {
                                SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = jointsTracked[hipCenterIndex] &&
                                    (jointsPos[hipCenterIndex].y - gestureData.jointPos.y) > 0.15f &&
                                    Mathf.Abs(jointsPos[hipCenterIndex].x - gestureData.jointPos.x) < 0.2f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for Squat
                case GestureType.Squat:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[hipCenterIndex] &&
                                (jointsPos[hipCenterIndex].y <= 0.7f))
                            {
                                SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = jointsTracked[hipCenterIndex] &&
                                    (jointsPos[hipCenterIndex].y - gestureData.jointPos.y) < -0.15f &&
                                    Mathf.Abs(jointsPos[hipCenterIndex].x - gestureData.jointPos.x) < 0.2f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for Push
                case GestureType.Push:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
                                   (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
                                   Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.2f &&
                                   (jointsPos[rightHandIndex].z - jointsPos[leftElbowIndex].z) < -0.2f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                                gestureData.progress = 0.5f;
                            }
                            else if (jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
                                    Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.2f &&
                                    (jointsPos[leftHandIndex].z - jointsPos[rightElbowIndex].z) < -0.2f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = gestureData.joint == rightHandIndex ?
                                    jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
                                    (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
                                    Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.2f &&
                                    (jointsPos[rightHandIndex].z - gestureData.jointPos.z) < -0.2f :
                                    jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
                                    Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.2f &&
                                    (jointsPos[leftHandIndex].z - gestureData.jointPos.z) < -0.2f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for Pull
                case GestureType.Pull:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
                               (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
                               Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.2f &&
                               (jointsPos[rightHandIndex].z - jointsPos[leftElbowIndex].z) < -0.3f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                                gestureData.progress = 0.5f;
                            }
                            else if (jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
                                    Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.2f &&
                                    (jointsPos[leftHandIndex].z - jointsPos[rightElbowIndex].z) < -0.3f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = gestureData.joint == rightHandIndex ?
                                    jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
                                    (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
                                    Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.2f &&
                                    (jointsPos[rightHandIndex].z - gestureData.jointPos.z) > 0.25f :
                                    jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
                                    (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
                                    Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.2f &&
                                    (jointsPos[leftHandIndex].z - gestureData.jointPos.z) > 0.25f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for ShoulderLeftFron
                case GestureType.ShoulderLeftFront:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && jointsTracked[leftHipIndex] &&
                                  (jointsPos[rightShoulderIndex].z - jointsPos[leftHipIndex].z) < 0f &&
                               (jointsPos[rightShoulderIndex].z - jointsPos[leftShoulderIndex].z) > -0.15f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightShoulderIndex, jointsPos[rightShoulderIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && jointsTracked[leftHipIndex] &&
                                        (jointsPos[rightShoulderIndex].z - jointsPos[leftShoulderIndex].z) < -0.2f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for ShoulderRightFront
                case GestureType.ShoulderRightFront:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && jointsTracked[rightHipIndex] &&
                               (jointsPos[leftShoulderIndex].z - jointsPos[rightHipIndex].z) < 0f &&
                               (jointsPos[leftShoulderIndex].z - jointsPos[rightShoulderIndex].z) > -0.15f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftShoulderIndex, jointsPos[leftShoulderIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && jointsTracked[rightHipIndex] &&
                                        (jointsPos[leftShoulderIndex].z - jointsPos[rightShoulderIndex].z) < -0.2f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for LeanLeft
                case GestureType.LeanLeft:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1  (right shoulder is left of the right hip, means leaning left)
                            if (jointsTracked[rightShoulderIndex] && jointsTracked[rightHipIndex] && jointsTracked[neckIndex] &&
                               (jointsPos[rightShoulderIndex].x - jointsPos[rightHipIndex].x) < 0f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightShoulderIndex, jointsPos[rightShoulderIndex]);
                                gestureData.progress = 0.3f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 0.5f)
                            {
                                // check if right shoulder is still left of the right hip (leaning left)
                                bool isInPose = jointsTracked[rightShoulderIndex] && jointsTracked[rightHipIndex] && jointsTracked[neckIndex] &&
                                    (jointsPos[rightShoulderIndex].x - jointsPos[rightHipIndex].x) < 0f;

                                if (isInPose)
                                {
                                    // calculate lean angle
                                    Vector3 vSpineLL = jointsPos[neckIndex] - jointsPos[hipCenterIndex];
                                    gestureData.screenPos.z = Vector3.Angle(Vector3.up, vSpineLL);

                                    gestureData.timestamp = timestamp;
                                    gestureData.progress = 0.7f;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for LeanRight
                case GestureType.LeanRight:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1 (left shoulder is right of the left hip, means leaning right)
                            if (jointsTracked[leftShoulderIndex] && jointsTracked[leftHipIndex] && jointsTracked[neckIndex] &&
                               (jointsPos[leftShoulderIndex].x - jointsPos[leftHipIndex].x) > 0f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftShoulderIndex, jointsPos[leftShoulderIndex]);
                                gestureData.progress = 0.3f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 0.5f)
                            {
                                // check if left shoulder is still right of the left hip (leaning right)
                                bool isInPose = jointsTracked[leftShoulderIndex] && jointsTracked[leftHipIndex] && jointsTracked[neckIndex] &&
                                    (jointsPos[leftShoulderIndex].x - jointsPos[leftHipIndex].x) > 0f;

                                if (isInPose)
                                {
                                    // calculate lean angle
                                    Vector3 vSpineLR = jointsPos[neckIndex] - jointsPos[hipCenterIndex];
                                    gestureData.screenPos.z = Vector3.Angle(Vector3.up, vSpineLR);

                                    gestureData.timestamp = timestamp;
                                    gestureData.progress = 0.7f;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for LeanForward
                case GestureType.LeanForward:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1 (shoulder center in front of hip center, means leaning forward)
                            if (jointsTracked[neckIndex] && jointsTracked[hipCenterIndex] &&
                                (jointsPos[neckIndex].z - jointsPos[hipCenterIndex].z) < -0.1f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, neckIndex, jointsPos[neckIndex]);
                                gestureData.progress = 0.3f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 0.5f)
                            {
                                // check if shoulder center is still in front of the hip center (leaning forward)
                                bool isInPose = jointsTracked[neckIndex] && jointsTracked[hipCenterIndex] &&
                                    (jointsPos[neckIndex].z - jointsPos[leftHipIndex].z) < -0.1f;

                                if (isInPose)
                                {
                                    // calculate lean angle
                                    Vector3 vSpineLL = jointsPos[neckIndex] - jointsPos[hipCenterIndex];
                                    gestureData.screenPos.z = Vector3.Angle(Vector3.up, vSpineLL);

                                    gestureData.timestamp = timestamp;
                                    gestureData.progress = 0.7f;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for LeanBack
                case GestureType.LeanBack:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1 (shoulder center behind hip center, means leaning back)
                            if (jointsTracked[neckIndex] && jointsTracked[hipCenterIndex] &&
                                (jointsPos[neckIndex].z - jointsPos[hipCenterIndex].z) > 0.1f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, neckIndex, jointsPos[neckIndex]);
                                gestureData.progress = 0.3f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 0.5f)
                            {
                                // check if shoulder center is still behind of the hip center (leaning back)
                                bool isInPose = jointsTracked[neckIndex] && jointsTracked[hipCenterIndex] &&
                                    (jointsPos[neckIndex].z - jointsPos[leftHipIndex].z) > 0.1f;

                                if (isInPose)
                                {
                                    // calculate lean angle
                                    Vector3 vSpineLR = jointsPos[neckIndex] - jointsPos[hipCenterIndex];
                                    gestureData.screenPos.z = Vector3.Angle(Vector3.up, vSpineLR);

                                    gestureData.timestamp = timestamp;
                                    gestureData.progress = 0.7f;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for KickLeft
                case GestureType.KickLeft:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[leftAnkleIndex] && jointsTracked[rightAnkleIndex] && jointsTracked[leftHipIndex] &&
                               (jointsPos[leftAnkleIndex].z - jointsPos[leftHipIndex].z) < 0f &&
                               (jointsPos[leftAnkleIndex].z - jointsPos[rightAnkleIndex].z) > -0.2f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftAnkleIndex, jointsPos[leftAnkleIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = jointsTracked[leftAnkleIndex] && jointsTracked[rightAnkleIndex] && jointsTracked[leftHipIndex] &&
                                    (jointsPos[leftAnkleIndex].z - jointsPos[rightAnkleIndex].z) < -0.4f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                // check for KickRight
                case GestureType.KickRight:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                            if (jointsTracked[leftAnkleIndex] && jointsTracked[rightAnkleIndex] && jointsTracked[rightHipIndex] &&
                               (jointsPos[rightAnkleIndex].z - jointsPos[rightHipIndex].z) < 0f &&
                               (jointsPos[rightAnkleIndex].z - jointsPos[leftAnkleIndex].z) > -0.2f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, rightAnkleIndex, jointsPos[rightAnkleIndex]);
                                gestureData.progress = 0.5f;
                            }
                            break;

                        case 1:  // gesture phase 2 = complete
                            if ((timestamp - gestureData.timestamp) < 1.5f)
                            {
                                bool isInPose = jointsTracked[leftAnkleIndex] && jointsTracked[rightAnkleIndex] && jointsTracked[rightHipIndex] &&
                                    (jointsPos[rightAnkleIndex].z - jointsPos[leftAnkleIndex].z) < -0.4f;

                                if (isInPose)
                                {
                                    Vector3 jointPos = jointsPos[gestureData.joint];
                                    CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                case GestureType.Run:
                    switch (gestureData.state)
                    {
                        case 0:  // gesture detection - phase 1
                                 // check if the left knee is up
                            if (jointsTracked[leftKneeIndex] && jointsTracked[rightKneeIndex] &&
                               (jointsPos[leftKneeIndex].y - jointsPos[rightKneeIndex].y) > 0.1f)
                            {
                                SetGestureJoint(ref gestureData, timestamp, leftKneeIndex, jointsPos[leftKneeIndex]);
                                gestureData.progress = 0.3f;
                            }
                            break;

                        case 1:  // gesture complete
                            if ((timestamp - gestureData.timestamp) < 1.0f)
                            {
                                // check if the right knee is up
                                bool isInPose = jointsTracked[rightKneeIndex] && jointsTracked[leftKneeIndex] &&
                                    (jointsPos[rightKneeIndex].y - jointsPos[leftKneeIndex].y) > 0.1f;

                                if (isInPose)
                                {
                                    // go to state 2
                                    gestureData.timestamp = timestamp;
                                    gestureData.progress = 0.7f;
                                    gestureData.state = 2;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;

                        case 2:  // gesture complete
                            if ((timestamp - gestureData.timestamp) < 1.0f)
                            {
                                // check if the left knee is up again
                                bool isInPose = jointsTracked[leftKneeIndex] && jointsTracked[rightKneeIndex] &&
                                    (jointsPos[leftKneeIndex].y - jointsPos[rightKneeIndex].y) > 0.1f;

                                if (isInPose)
                                {
                                    // go back to state 1
                                    gestureData.timestamp = timestamp;
                                    gestureData.progress = 0.8f;
                                    gestureData.state = 1;
                                }
                            }
                            else
                            {
                                // cancel the gesture
                                SetGestureCancelled(ref gestureData);
                            }
                            break;
                    }
                    break;

                    // here come more gesture-cases
            }
        }

    }
}