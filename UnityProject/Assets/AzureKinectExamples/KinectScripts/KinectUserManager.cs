using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.rfilkov.kinect
{
    /// <summary>
    /// Kinect user manager is the component that tracks the users in front of the sensor.
    /// </summary>
    public class KinectUserManager : MonoBehaviour
    {

        [System.Serializable]
        public class KinectUserEvent : UnityEvent<ulong, int> { }


        /// <summary>
        /// Fired when new user gets detected.
        /// </summary>
        //public event System.Action<ulong, int> OnUserAdded;
        public KinectUserEvent OnUserAdded;

        /// <summary>
        /// Fired when user gets removed.
        /// </summary>
        //public event System.Action<ulong, int> OnUserRemoved;
        public KinectUserEvent OnUserRemoved;


        // List of all users
        internal List<ulong> alUserIds = new List<ulong>();
        internal Dictionary<ulong, int> dictUserIdToIndex = new Dictionary<ulong, int>();
        internal ulong[] aUserIndexIds = new ulong[KinectInterop.Constants.MaxBodyCount];
        internal Dictionary<ulong, float> dictUserIdToTime = new Dictionary<ulong, float>();

        // Primary (first or closest) user ID
        internal ulong liPrimaryUserId = 0;

        // Calibration gesture data for each player
        internal Dictionary<ulong, KinectGestureManager.GestureData> playerCalibrationData = new Dictionary<ulong, KinectGestureManager.GestureData>();

        // reference to KM
        private KinectManager kinectManager = null;


        protected virtual void Start()
        {
            kinectManager = KinectManager.Instance;
        }


        /// <summary>
        /// Rearranges the user indices, according to the current criteria
        /// </summary>
        public virtual void RearrangeUserIndices(KinectManager.UserDetectionOrder userDetectionOrder)
        {

            if (userDetectionOrder != KinectManager.UserDetectionOrder.Appearance)
            {
                // get current user positions
                Vector3[] userPos = new Vector3[aUserIndexIds.Length];
                for (int i = 0; i < aUserIndexIds.Length; i++)
                {
                    ulong userId = aUserIndexIds[i];
                    userPos[i] = userId != 0 ? kinectManager.GetUserPosition(userId) : Vector3.zero;
                }

                // bubble sort
                bool reorderDone = false;
                for (int i = aUserIndexIds.Length - 1; i >= 1; i--)
                {
                    bool switchDone = false;

                    for (int j = 0; j < i; j++)
                    {
                        float userDist1 = 0f;
                        if (userDetectionOrder == KinectManager.UserDetectionOrder.Distance)
                            userDist1 = Mathf.Abs(userPos[j].x) + Mathf.Abs(userPos[j].z);
                        else if (userDetectionOrder == KinectManager.UserDetectionOrder.LeftToRight)
                            userDist1 = userPos[j].x;

                        if (Mathf.Abs(userDist1) < 0.01f)
                            userDist1 = 1000f;  // far away

                        float userDist2 = 0f;
                        if (userDetectionOrder == KinectManager.UserDetectionOrder.Distance)
                            userDist2 = Mathf.Abs(userPos[j + 1].x) + Mathf.Abs(userPos[j + 1].z);
                        else if (userDetectionOrder == KinectManager.UserDetectionOrder.LeftToRight)
                            userDist2 = userPos[j + 1].x;

                        if (Mathf.Abs(userDist2) < 0.01f)
                            userDist2 = 1000f;  // far away

                        if (userDist1 > userDist2)
                        {
                            // switch them
                            ulong tmpUserId = aUserIndexIds[j];
                            aUserIndexIds[j] = aUserIndexIds[j + 1];
                            aUserIndexIds[j + 1] = tmpUserId;

                            reorderDone = switchDone = true;
                        }
                    }

                    if (!switchDone)  // check for sorted array
                        break;
                }

                if (reorderDone)
                {
                    System.Text.StringBuilder sbUsersOrder = new System.Text.StringBuilder();
                    sbUsersOrder.Append("Users reindexed: ");

                    for (int i = 0; i < aUserIndexIds.Length; i++)
                    {
                        if (aUserIndexIds[i] != 0)
                        {
                            sbUsersOrder.Append(i).Append(":").Append(aUserIndexIds[i]).Append("  ");
                        }
                    }

                    Debug.Log(sbUsersOrder.ToString());
                }
            }

        }


        // Returns empty user slot for the given user Id
        protected virtual int GetEmptyUserSlot(ulong userId, int bodyIndex, ref KinectInterop.BodyData[] alTrackedBodies, KinectManager.UserDetectionOrder userDetectionOrder)
        {
            // rearrange current users
            RearrangeUserIndices(userDetectionOrder);
            int uidIndex = -1;

            if (userDetectionOrder != KinectManager.UserDetectionOrder.Appearance)
            {
                // add the new user, depending on the distance
                Vector3 userPos = alTrackedBodies[bodyIndex].position;

                float userDist = 0f;
                if (userDetectionOrder == KinectManager.UserDetectionOrder.Distance)
                    userDist = Mathf.Abs(userPos.x) + Mathf.Abs(userPos.z);
                else if (userDetectionOrder == KinectManager.UserDetectionOrder.LeftToRight)
                    userDist = userPos.x;

                for (int i = 0; i < aUserIndexIds.Length; i++)
                {
                    if (aUserIndexIds[i] == 0)
                    {
                        // free user slot
                        uidIndex = i;
                        break;
                    }
                    else
                    {
                        ulong uidUserId = aUserIndexIds[i];
                        Vector3 uidUserPos = kinectManager.GetUserPosition(uidUserId);

                        float uidUserDist = 0;
                        if (userDetectionOrder == KinectManager.UserDetectionOrder.Distance)
                            uidUserDist = Mathf.Abs(uidUserPos.x) + Mathf.Abs(uidUserPos.z);
                        else if (userDetectionOrder == KinectManager.UserDetectionOrder.LeftToRight)
                            uidUserDist = uidUserPos.x;

                        if (userDist <= uidUserDist)
                        {
                            // current user is left to the compared one
                            for (int u = (aUserIndexIds.Length - 2); u >= i; u--)
                            {
                                aUserIndexIds[u + 1] = aUserIndexIds[u];

                                if (aUserIndexIds[u] != 0)
                                {
                                    Debug.Log(string.Format("Reindexing user {0} to {1}, ID: {2}.", u, u + 1, aUserIndexIds[u]));
                                }
                            }

                            aUserIndexIds[i] = 0; // cleanup current index
                            uidIndex = i;
                            break;
                        }
                    }
                }

            }
            else
            {
                // look for the 1st available slot
                for (int i = 0; i < aUserIndexIds.Length; i++)
                {
                    if (aUserIndexIds[i] == 0)
                    {
                        uidIndex = i;
                        break;
                    }
                }
            }

            return uidIndex;
        }


        // releases the user slot. rearranges the remaining users.
        protected virtual void FreeEmptyUserSlot(int uidIndex, KinectManager.UserDetectionOrder userDetectionOrder)
        {
            aUserIndexIds[uidIndex] = 0;

            if (userDetectionOrder != KinectManager.UserDetectionOrder.Appearance)
            {
                // rearrange the remaining users
                for (int u = uidIndex; u < (aUserIndexIds.Length - 1); u++)
                {
                    aUserIndexIds[u] = aUserIndexIds[u + 1];

                    if (aUserIndexIds[u + 1] != 0)
                    {
                        Debug.Log(string.Format("Reindexing user {0} to {1}, ID: {2}.", u + 1, u, aUserIndexIds[u + 1]));
                    }
                }

                // make sure the last slot is free
                aUserIndexIds[aUserIndexIds.Length - 1] = 0;
            }

            // rearrange the remaining users
            RearrangeUserIndices(userDetectionOrder);
        }


        // Adds UserId to the list of users
        public virtual int CalibrateUser(ulong userId, int bodyIndex, ref KinectInterop.BodyData[] alTrackedBodies, 
            KinectManager.UserDetectionOrder userDetectionOrder, GestureType playerCalibrationPose, KinectGestureManager gestureManager)
        {
            if (!alUserIds.Contains(userId))
            {
                if (CheckForCalibrationPose(userId, bodyIndex, playerCalibrationPose, gestureManager, ref alTrackedBodies))
                {
                    //int uidIndex = alUserIds.Count;
                    int uidIndex = GetEmptyUserSlot(userId, bodyIndex, ref alTrackedBodies, userDetectionOrder);

                    if (uidIndex >= 0)
                    {
                        aUserIndexIds[uidIndex] = userId;
                    }
                    else
                    {
                        // no empty user-index slot
                        return -1;
                    }

                    dictUserIdToIndex[userId] = bodyIndex;
                    dictUserIdToTime[userId] = Time.time;
                    alUserIds.Add(userId);

                    // set primary user-id, if there is none
                    if (liPrimaryUserId == 0 && aUserIndexIds.Length > 0)
                    {
                        liPrimaryUserId = aUserIndexIds[0];  // userId
                    }

                    return uidIndex;
                }
            }

            return -1;
        }


        // fires the OnUserAdded-event
        internal void FireOnUserAdded(ulong userId, int userIndex)
        {
            OnUserAdded?.Invoke(userId, userIndex);
        }


        // Remove a lost UserId
        public virtual int RemoveUser(ulong userId, KinectManager.UserDetectionOrder userDetectionOrder)
        {
            //int uidIndex = alUserIds.IndexOf(userId);
            int uidIndex = System.Array.IndexOf(aUserIndexIds, userId);

            // clear calibration data for this user
            if (playerCalibrationData.ContainsKey(userId))
            {
                playerCalibrationData.Remove(userId);
            }

            // clean up the outdated calibration data in the data dictionary
            List<ulong> alCalDataKeys = new List<ulong>(playerCalibrationData.Keys);

            foreach (ulong calUserID in alCalDataKeys)
            {
                KinectGestureManager.GestureData gestureData = playerCalibrationData[calUserID];

                if ((gestureData.timestamp + 60f) < Time.realtimeSinceStartup)
                {
                    playerCalibrationData.Remove(calUserID);
                }
            }

            alCalDataKeys.Clear();

            // remove user-id from the global users lists
            dictUserIdToIndex.Remove(userId);
            dictUserIdToTime.Remove(userId);
            alUserIds.Remove(userId);

            if (uidIndex >= 0)
            {
                FreeEmptyUserSlot(uidIndex, userDetectionOrder);
            }

            // if this was the primary user, update the primary user-id
            if (liPrimaryUserId == userId)
            {
                if (aUserIndexIds.Length > 0)
                {
                    liPrimaryUserId = aUserIndexIds[0];
                }
                else
                {
                    liPrimaryUserId = 0;
                }
            }

            if (alUserIds.Count == 0)
            {
                //Debug.Log("Waiting for users.");
            }

            return uidIndex;
        }


        // fires the OnUserRemoved-event
        internal void FireOnUserRemoved(ulong userId, int userIndex)
        {
            OnUserRemoved?.Invoke(userId, userIndex);
        }


        // check if the calibration pose is complete for given user
        protected virtual bool CheckForCalibrationPose(ulong UserId, int bodyIndex, GestureType calibrationGesture, 
            KinectGestureManager gestureManager, ref KinectInterop.BodyData[] alTrackedBodies)
        {
            if (calibrationGesture == GestureType.None)
                return true;
            if (!gestureManager)
                return false;

            KinectGestureManager.GestureData gestureData = playerCalibrationData.ContainsKey(UserId) ?
                playerCalibrationData[UserId] : new KinectGestureManager.GestureData();

            // init gesture data if needed
            if (gestureData.userId != UserId)
            {
                gestureData.userId = UserId;
                gestureData.gesture = calibrationGesture;
                gestureData.state = 0;
                gestureData.timestamp = Time.realtimeSinceStartup;
                gestureData.joint = 0;
                gestureData.progress = 0f;
                gestureData.complete = false;
                gestureData.cancelled = false;
            }

            // get joint positions and tracking
            int iAllJointsCount = (int)KinectInterop.JointType.Count;
            bool[] playerJointsTracked = new bool[iAllJointsCount];
            Vector3[] playerJointsPos = new Vector3[iAllJointsCount];

            int[] aiNeededJointIndexes = gestureManager.GetNeededJointIndexes();
            int iNeededJointsCount = aiNeededJointIndexes.Length;

            for (int i = 0; i < iNeededJointsCount; i++)
            {
                int joint = aiNeededJointIndexes[i];

                if (joint >= 0)
                {
                    KinectInterop.JointData jointData = alTrackedBodies[bodyIndex].joint[joint];

                    playerJointsTracked[joint] = jointData.trackingState != KinectInterop.TrackingState.NotTracked;
                    playerJointsPos[joint] = jointData.kinectPos;

                    if (!playerJointsTracked[joint] && (joint == (int)KinectInterop.JointType.Neck))
                    {
                        KinectInterop.JointData lShoulderData = alTrackedBodies[bodyIndex].joint[(int)KinectInterop.JointType.ShoulderLeft];
                        KinectInterop.JointData rShoulderData = alTrackedBodies[bodyIndex].joint[(int)KinectInterop.JointType.ShoulderRight];

                        if (lShoulderData.trackingState != KinectInterop.TrackingState.NotTracked && rShoulderData.trackingState != KinectInterop.TrackingState.NotTracked)
                        {
                            playerJointsTracked[joint] = true;
                            playerJointsPos[joint] = (lShoulderData.kinectPos + rShoulderData.kinectPos) / 2f;
                        }
                    }
                }
            }

            // estimate the gesture progess
            gestureManager.CheckForGesture(UserId, ref gestureData, Time.realtimeSinceStartup,
                ref playerJointsPos, ref playerJointsTracked);
            playerCalibrationData[UserId] = gestureData;

            // check if gesture is complete
            if (gestureData.complete)
            {
                gestureData.userId = 0;
                playerCalibrationData[UserId] = gestureData;

                return true;
            }

            return false;
        }

    }
}

