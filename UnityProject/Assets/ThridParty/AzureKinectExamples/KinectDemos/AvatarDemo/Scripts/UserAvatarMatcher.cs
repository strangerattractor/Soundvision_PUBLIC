using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    public class UserAvatarMatcher : MonoBehaviour
    {

        [Tooltip("Humanoid model used for avatar instatiation.")]
        public GameObject avatarModel;

        [Tooltip("Smooth factor used by the avatar controller.")]
        public float smoothFactor = 10f;

        [Tooltip("If enabled, makes the avatar position relative to this camera to be the same as the player's position to the sensor.")]
        public Camera posRelativeToCamera;

        [Tooltip("Whether the avatar is facing the player or not.")]
        public bool mirroredMovement = true;

        [Tooltip("Whether the avatar is allowed to move vertically or not.")]
        public bool verticalMovement = true;

        [Tooltip("Whether the avatar's feet must stick to the ground.")]
        public bool groundedFeet = false;

        [Tooltip("Whether to apply the humanoid model's muscle limits or not.")]
        public bool applyMuscleLimits = false;


        private KinectManager kinectManager;
        private int maxUserCount = 0;

        private ulong userChecksum = 0;
        private Dictionary<ulong, GameObject> alUserAvatars = new Dictionary<ulong, GameObject>();


        void Start()
        {
            kinectManager = KinectManager.Instance;
        }

        void Update()
        {
            ulong checksum = GetUserChecksum(out maxUserCount);

            if (userChecksum != checksum)
            {
                userChecksum = checksum;
                List<ulong> alAvatarToRemove = new List<ulong>(alUserAvatars.Keys);

                for (int i = 0; i < maxUserCount; i++)
                {
                    ulong userId = kinectManager.GetUserIdByIndex(i);
                    if (userId == 0)
                        continue;

                    if (alAvatarToRemove.Contains(userId))
                        alAvatarToRemove.Remove(userId);

                    if (!alUserAvatars.ContainsKey(userId))
                    {
                        Debug.Log("Creating avatar for userId: " + userId + ", Time: " + Time.realtimeSinceStartup);

                        // create avatar for the user
                        int userIndex = kinectManager.GetUserIndexById(userId);
                        GameObject avatarObj = CreateUserAvatar(userId, userIndex);

                        alUserAvatars[userId] = avatarObj;
                    }
                }

                // remove the missing users from the list
                foreach (ulong userId in alAvatarToRemove)
                {
                    if (alUserAvatars.ContainsKey(userId))
                    {
                        Debug.Log("Destroying avatar for userId: " + userId + ", Time: " + Time.realtimeSinceStartup);

                        GameObject avatarObj = alUserAvatars[userId];
                        alUserAvatars.Remove(userId);

                        // destroy the user's avatar
                        DestroyUserAvatar(avatarObj);
                    }
                }

            }
        }

        // returns the checksum of current users
        private ulong GetUserChecksum(out int maxUserCount)
        {
            maxUserCount = 0;
            ulong checksum = 0;

            if (kinectManager && kinectManager.IsInitialized())
            {
                maxUserCount = kinectManager.GetMaxBodyCount();
                //ulong csMask = 0xFFFFFFFFFFFFFFF;

                for (int i = 0; i < maxUserCount; i++)
                {
                    ulong userId = kinectManager.GetUserIdByIndex(i);
                    //userId &= csMask;

                    if (userId != 0)
                    {
                        checksum += userId;
                        //checksum &= csMask;
                    }
                }
            }

            return checksum;
        }


        // creates avatar for the given user
        private GameObject CreateUserAvatar(ulong userId, int userIndex)
        {
            GameObject avatarObj = null;

            if (avatarModel)
            {
                Vector3 userPos = new Vector3(userIndex, 0, 0);
                Quaternion userRot = Quaternion.Euler(!mirroredMovement ? Vector3.zero : new Vector3(0, 180, 0));

                avatarObj = Instantiate(avatarModel, userPos, userRot);
                avatarObj.name = "User-" + userId;

                AvatarController ac = avatarObj.GetComponent<AvatarController>();
                if (ac == null)
                {
                    ac = avatarObj.AddComponent<AvatarController>();
                    ac.playerIndex = userIndex;

                    ac.smoothFactor = smoothFactor;
                    ac.posRelativeToCamera = posRelativeToCamera;

                    ac.mirroredMovement = mirroredMovement;
                    ac.verticalMovement = verticalMovement;

                    ac.groundedFeet = groundedFeet;
                    ac.applyMuscleLimits = applyMuscleLimits;
                }
            }

            return avatarObj;
        }

        // destroys the avatar and refreshes the list of avatar controllers
        private void DestroyUserAvatar(GameObject avatarObj)
        {
            if (avatarObj)
            {
                Destroy(avatarObj);
            }
        }

    }
}
