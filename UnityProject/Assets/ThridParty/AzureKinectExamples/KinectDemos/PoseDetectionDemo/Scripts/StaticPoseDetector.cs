using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// Static pose detector check whether the user's pose matches a predefined, static model's pose.
    /// </summary>
    public class StaticPoseDetector : MonoBehaviour
    {
        [Tooltip("User avatar model, who needs to reach the target pose.")]
        public PoseModelHelper avatarModel;

        [Tooltip("Model in pose that need to be reached by the user.")]
        public PoseModelHelper poseModel;

        [Tooltip("List of joints to compare.")]
        public List<KinectInterop.JointType> poseJoints = new List<KinectInterop.JointType>();

        [Tooltip("Allowed delay in pose match, in seconds. 0 means no delay allowed.")]
        [Range(0f, 10f)]
        public float delayAllowed = 2f;

        [Tooltip("Time between pose-match checks, in seconds. 0 means check each frame.")]
        [Range(0f, 1f)]
        public float timeBetweenChecks = 0.1f;

        [Tooltip("Threshold, above which we consider the pose is matched.")]
        [Range(0.5f, 1f)]
        public float matchThreshold = 0.7f;

        [Tooltip("GUI-Text to display information messages.")]
        public UnityEngine.UI.Text infoText;

        // whether the pose is matched or not
        private bool bPoseMatched = false;
        // match percent (between 0 and 1)
        private float fMatchPercent = 0f;
        // pose-time with best matching
        private float fMatchPoseTime = 0f;

        // initial rotation
        private Quaternion initialAvatarRotation = Quaternion.identity;
        private Quaternion initialPoseRotation = Quaternion.identity;

        // reference to the avatar controller
        private AvatarController avatarController = null;

        // uncomment to get debug info
        private StringBuilder sbDebug = null; // new StringBuilder();



        // data for each saved pose
        public class PoseModelData
        {
            public float fTime;
            public Vector3[] avBoneDirs;
        }

        // list of saved pose data
        private List<PoseModelData> alSavedPoses = new List<PoseModelData>();

        // current avatar pose
        private PoseModelData poseAvatar = new PoseModelData();

        // last time the model pose was saved 
        private float lastPoseSavedTime = 0f;


        /// <summary>
        /// Determines whether the target pose is matched or not.
        /// </summary>
        /// <returns><c>true</c> if the target pose is matched; otherwise, <c>false</c>.</returns>
        public bool IsPoseMatched()
        {
            return bPoseMatched;
        }


        /// <summary>
        /// Gets the pose match percent.
        /// </summary>
        /// <returns>The match percent (value between 0 and 1).</returns>
        public float GetMatchPercent()
        {
            return fMatchPercent;
        }


        /// <summary>
        /// Gets the time of the best matching pose.
        /// </summary>
        /// <returns>Time of the best matching pose.</returns>
        public float GetMatchPoseTime()
        {
            return fMatchPoseTime;
        }


        /// <summary>
        /// Gets the last check time.
        /// </summary>
        /// <returns>The last check time.</returns>
        public float GetPoseCheckTime()
        {
            return lastPoseSavedTime;
        }


        private void Awake()
        {
            if(avatarModel)
            {
                initialAvatarRotation = avatarModel.transform.rotation;
                avatarController = avatarModel.gameObject.GetComponent<AvatarController>();
            }

            if(poseModel)
            {
                initialPoseRotation = poseModel.transform.rotation;
            }
        }


        void Update()
        {
            KinectManager kinectManager = KinectManager.Instance;

            // get mirrored state
            bool isMirrored = avatarController ? avatarController.mirroredMovement : true;  // true by default

            // current time
            float fCurrentTime = Time.realtimeSinceStartup;

            // save model pose, if needed
            if ((fCurrentTime - lastPoseSavedTime) >= timeBetweenChecks)
            {
                lastPoseSavedTime = fCurrentTime;

                // remove old poses and save current one
                RemoveOldSavedPoses(fCurrentTime);
                AddCurrentPoseToSaved(fCurrentTime, isMirrored);
            }

            if(kinectManager != null && kinectManager.IsInitialized())
            {
                if (avatarModel != null && avatarController && kinectManager.IsUserTracked(avatarController.playerId))
                {
                    // get current avatar pose
                    GetAvatarPose(fCurrentTime, isMirrored);

                    // get the difference
                    GetPoseDifference(isMirrored);

                    if (infoText != null)
                    {
                        //string sPoseMessage = string.Format("Pose match: {0:F0}% {1:F1}s ago {2}", fMatchPercent * 100f, Time.realtimeSinceStartup - fMatchPoseTime,
                        //                                    (bPoseMatched ? "- Matched" : ""));
                        string sPoseMessage = string.Format("Pose match: {0:F0}% {1}", fMatchPercent * 100f, (bPoseMatched ? "- Matched" : ""));
                        if (sbDebug != null)
                            sPoseMessage += sbDebug.ToString();
                        infoText.text = sPoseMessage;
                    }
                }
                else
                {
                    // no user found
                    fMatchPercent = 0f;
                    fMatchPoseTime = 0f;
                    bPoseMatched = false;

                    if (infoText != null)
                    {
                        infoText.text = "Try to follow the model pose.";
                    }
                }
            }
        }


        // removes saved poses older than delayAllowed from the list
        private void RemoveOldSavedPoses(float fCurrentTime)
        {
            for (int i = alSavedPoses.Count - 1; i >= 0; i--)
            {
                if ((fCurrentTime - alSavedPoses[i].fTime) >= delayAllowed)
                {
                    alSavedPoses.RemoveAt(i);
                }
            }
        }


        // adds current pose of poseModel to the saved poses list
        private void AddCurrentPoseToSaved(float fCurrentTime, bool isMirrored)
        {
            KinectManager kinectManager = KinectManager.Instance;
            if (kinectManager == null || poseModel == null || poseJoints == null)
                return;

            PoseModelData pose = new PoseModelData();
            pose.fTime = fCurrentTime;
            pose.avBoneDirs = new Vector3[poseJoints.Count];

            // save model rotation
            Quaternion poseModelRotation = poseModel.transform.rotation;

            if(avatarController)
            {
                ulong avatarUserId = avatarController.playerId;
                bool isAvatarMirrored = avatarController.mirroredMovement;

                Quaternion userRotation = kinectManager.GetUserOrientation(avatarUserId, !isAvatarMirrored);
                poseModel.transform.rotation = initialPoseRotation * userRotation;

            }

            int jointCount = kinectManager.GetJointCount();
            for (int i = 0; i < poseJoints.Count; i++)
            {
                KinectInterop.JointType joint = poseJoints[i];
                KinectInterop.JointType nextJoint = kinectManager.GetNextJoint(joint);

                if (nextJoint != joint && (int)nextJoint >= 0 && (int)nextJoint < jointCount)
                {
                    Transform poseTransform1 = poseModel.GetBoneTransform(poseModel.GetBoneIndexByJoint(joint, isMirrored));
                    Transform poseTransform2 = poseModel.GetBoneTransform(poseModel.GetBoneIndexByJoint(nextJoint, isMirrored));

                    if (poseTransform1 != null && poseTransform2 != null)
                    {
                        pose.avBoneDirs[i] = (poseTransform2.position - poseTransform1.position).normalized;
                    }
                }
            }

            // add pose to the list
            alSavedPoses.Add(pose);

            // restore model rotation
            poseModel.transform.rotation = poseModelRotation;
        }


        // gets the current avatar pose
        private void GetAvatarPose(float fCurrentTime, bool isMirrored)
        {
            KinectManager kinectManager = KinectManager.Instance;
            if (kinectManager == null || avatarModel == null || poseJoints == null)
                return;

            poseAvatar.fTime = fCurrentTime;
            if (poseAvatar.avBoneDirs == null)
            {
                poseAvatar.avBoneDirs = new Vector3[poseJoints.Count];
            }

            for (int i = 0; i < poseJoints.Count; i++)
            {
                KinectInterop.JointType joint = poseJoints[i];
                KinectInterop.JointType nextJoint = kinectManager.GetNextJoint(joint);

                int jointCount = kinectManager.GetJointCount();
                if (nextJoint != joint && (int)nextJoint >= 0 && (int)nextJoint < jointCount)
                {
                    Transform avatarTransform1 = avatarModel.GetBoneTransform(avatarModel.GetBoneIndexByJoint(joint, isMirrored));
                    Transform avatarTransform2 = avatarModel.GetBoneTransform(avatarModel.GetBoneIndexByJoint(nextJoint, isMirrored));

                    if (avatarTransform1 != null && avatarTransform2 != null)
                    {
                        poseAvatar.avBoneDirs[i] = (avatarTransform2.position - avatarTransform1.position).normalized;
                    }
                }
            }
        }


        // gets the difference between the avatar pose and the list of saved poses
        private void GetPoseDifference(bool isMirrored)
        {
            // by-default values
            bPoseMatched = false;
            fMatchPercent = 0f;
            fMatchPoseTime = 0f;

            KinectManager kinectManager = KinectManager.Instance;
            if (poseJoints == null || poseAvatar.avBoneDirs == null)
                return;

            if (sbDebug != null)
            {
                sbDebug.Clear();
                sbDebug.AppendLine();
            }

            // check the difference with saved poses, starting from the last one
            for (int p = alSavedPoses.Count - 1; p >= 0; p--)
            {
                float fAngleDiff = 0f;
                float fMaxDiff = 0f;

                PoseModelData poseModel = alSavedPoses[p];
                for (int i = 0; i < poseJoints.Count; i++)
                {
                    Vector3 vPoseBone = poseModel.avBoneDirs[i];
                    Vector3 vAvatarBone = poseAvatar.avBoneDirs[i];

                    if (vPoseBone == Vector3.zero || vAvatarBone == Vector3.zero)
                        continue;

                    float fDiff = Vector3.Angle(vPoseBone, vAvatarBone);
                    if (fDiff > 90f)
                        fDiff = 90f;

                    fAngleDiff += fDiff;
                    fMaxDiff += 90f;  // we assume the max diff could be 90 degrees

                    if (sbDebug != null)
                    {
                        sbDebug.AppendFormat("SP: {0}, {1} - angle: {2:F0}, match: {3:F0}%", p, poseJoints[i], fDiff, (1f - fDiff / 90f) * 100f);
                        sbDebug.AppendLine();
                    }
                }

                float fPoseMatch = fMaxDiff > 0f ? (1f - fAngleDiff / fMaxDiff) : 0f;
                if (fPoseMatch > fMatchPercent)
                {
                    fMatchPercent = fPoseMatch;
                    fMatchPoseTime = poseModel.fTime;
                    bPoseMatched = (fMatchPercent >= matchThreshold);
                }
            }
        }

    }
}
