using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// Pose model helper matches the sensor-tracked joints to model transforms.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PoseModelHelper : MonoBehaviour
    {
        // Variable to hold all them bones. It will initialize the same size as initialRotations.
        protected Transform[] bones;


        /// <summary>
        /// Gets the number of bone transforms (array length).
        /// </summary>
        /// <returns>The number of bone transforms.</returns>
        public int GetBoneTransformCount()
        {
            return bones != null ? bones.Length : 0;
        }

        /// <summary>
        /// Gets the bone transform by index.
        /// </summary>
        /// <returns>The bone transform.</returns>
        /// <param name="index">Index</param>
        public Transform GetBoneTransform(int index)
        {
            if (index >= 0 && index < bones.Length)
            {
                return bones[index];
            }

            return null;
        }

        /// <summary>
        /// Gets the bone index by joint type.
        /// </summary>
        /// <returns>The bone index.</returns>
        /// <param name="joint">Joint type</param>
        /// <param name="bMirrored">If set to <c>true</c> gets the mirrored joint index.</param>
        public int GetBoneIndexByJoint(KinectInterop.JointType joint, bool bMirrored)
        {
            int boneIndex = -1;

            if (jointMap2boneIndex.ContainsKey(joint))
            {
                boneIndex = !bMirrored ? jointMap2boneIndex[joint] : mirrorJointMap2boneIndex[joint];
            }

            return boneIndex;
        }


        // transform caching gives performance boost since Unity calls GetComponent<Transform>() each time you call transform 
        protected Transform _transformCache;
        public new Transform transform
        {
            get
            {
                if (!_transformCache)
                {
                    _transformCache = base.transform;
                }

                return _transformCache;
            }
        }


        public void Awake()
        {
            // check for double start
            if (bones != null)
                return;
            if (!gameObject.activeInHierarchy)
                return;

            // inits the bones array
            bones = new Transform[21];

            // Map bones to the points the Kinect tracks
            MapBones();
        }


        // If the bones to be mapped have been declared, map that bone to the model.
        protected virtual void MapBones()
        {
            // get bone transforms from the animator component
            Animator animatorComponent = GetComponent<Animator>();

            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
                    continue;

                bones[boneIndex] = animatorComponent ? animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]) : null;
            }
        }


        // dictionaries to speed up bone processing
        protected static readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
        {
            {0, HumanBodyBones.Hips},
            {1, HumanBodyBones.Spine},
            {2, HumanBodyBones.Chest},
            {3, HumanBodyBones.Neck},
//    		{4, HumanBodyBones.Head},

            {5, HumanBodyBones.LeftShoulder},
            {6, HumanBodyBones.LeftUpperArm},
            {7, HumanBodyBones.LeftLowerArm},
            {8, HumanBodyBones.LeftHand},

            {9, HumanBodyBones.RightShoulder},
            {10, HumanBodyBones.RightUpperArm},
            {11, HumanBodyBones.RightLowerArm},
            {12, HumanBodyBones.RightHand},

            {13, HumanBodyBones.LeftUpperLeg},
            {14, HumanBodyBones.LeftLowerLeg},
            {15, HumanBodyBones.LeftFoot},
//    		{16, HumanBodyBones.LeftToes},
		
		    {17, HumanBodyBones.RightUpperLeg},
            {18, HumanBodyBones.RightLowerLeg},
            {19, HumanBodyBones.RightFoot},
//    		{20, HumanBodyBones.RightToes},
        };

        protected static readonly Dictionary<KinectInterop.JointType, int> jointMap2boneIndex = new Dictionary<KinectInterop.JointType, int>
        {
            {KinectInterop.JointType.Pelvis, 0},
            {KinectInterop.JointType.SpineNaval, 1},
            {KinectInterop.JointType.SpineChest, 2},
            {KinectInterop.JointType.Neck, 3},
            {KinectInterop.JointType.Head, 4},

            {KinectInterop.JointType.ClavicleLeft, 5},
            {KinectInterop.JointType.ShoulderLeft, 6},
            {KinectInterop.JointType.ElbowLeft, 7},
            {KinectInterop.JointType.WristLeft, 8},

            {KinectInterop.JointType.ClavicleRight, 9},
            {KinectInterop.JointType.ShoulderRight, 10},
            {KinectInterop.JointType.ElbowRight, 11},
            {KinectInterop.JointType.WristRight, 12},

            {KinectInterop.JointType.HipLeft, 13},
            {KinectInterop.JointType.KneeLeft, 14},
            {KinectInterop.JointType.AnkleLeft, 15},
            {KinectInterop.JointType.FootLeft, 16},

            {KinectInterop.JointType.HipRight, 17},
            {KinectInterop.JointType.KneeRight, 18},
            {KinectInterop.JointType.AnkleRight, 19},
            {KinectInterop.JointType.FootRight, 20},
        };

        protected static readonly Dictionary<KinectInterop.JointType, int> mirrorJointMap2boneIndex = new Dictionary<KinectInterop.JointType, int>
        {
            {KinectInterop.JointType.Pelvis, 0},
            {KinectInterop.JointType.SpineNaval, 1},
            {KinectInterop.JointType.SpineChest, 2},
            {KinectInterop.JointType.Neck, 3},
            {KinectInterop.JointType.Head, 4},

            {KinectInterop.JointType.ClavicleRight, 5},
            {KinectInterop.JointType.ShoulderRight, 6},
            {KinectInterop.JointType.ElbowRight, 7},
            {KinectInterop.JointType.WristRight, 8},

            {KinectInterop.JointType.ClavicleLeft, 9},
            {KinectInterop.JointType.ShoulderLeft, 10},
            {KinectInterop.JointType.ElbowLeft, 11},
            {KinectInterop.JointType.WristLeft, 12},

            {KinectInterop.JointType.HipRight, 13},
            {KinectInterop.JointType.KneeRight, 14},
            {KinectInterop.JointType.AnkleRight, 15},
            {KinectInterop.JointType.FootRight, 16},

            {KinectInterop.JointType.HipLeft, 17},
            {KinectInterop.JointType.KneeLeft, 18},
            {KinectInterop.JointType.AnkleLeft, 19},
            {KinectInterop.JointType.FootLeft, 20},
        };

    }
}
