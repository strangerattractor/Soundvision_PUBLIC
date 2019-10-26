using UnityEngine;
using System;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// Cubeman controller transfers the captured user motion to a cubeman model.
    /// </summary>
    public class CubemanController : MonoBehaviour
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("Whether the cubeman is allowed to move vertically or not.")]
        public bool verticalMovement = true;

        [Tooltip("Whether the cubeman is facing the player or not.")]
        public bool mirroredMovement = false;

        [Tooltip("Rate at which the cubeman will move through the scene.")]
        public float moveRate = 1f;

        public GameObject Pelvis;
        public GameObject SpineNaval;
        public GameObject SpineChest;
        public GameObject Neck;
        public GameObject Head;

        public GameObject ClavicleLeft;
        public GameObject ShoulderLeft;
        public GameObject ElbowLeft;
        public GameObject WristLeft;
        public GameObject HandLeft;

        public GameObject ClavicleRight;
        public GameObject ShoulderRight;
        public GameObject ElbowRight;
        public GameObject WristRight;
        public GameObject HandRight;

        public GameObject HipLeft;
        public GameObject KneeLeft;
        public GameObject AnkleLeft;
        public GameObject FootLeft;

        public GameObject HipRight;
        public GameObject KneeRight;
        public GameObject AnkleRight;
        public GameObject FootRight;

        public GameObject Nose;
        public GameObject EyeLeft;
        public GameObject EarLeft;
        public GameObject EyeRight;
        public GameObject EarRight;

        public GameObject HandtipLeft;
        public GameObject ThumbLeft;
        public GameObject HandtipRight;
        public GameObject ThumbRight;

        public LineRenderer skeletonLine;
        //public LineRenderer debugLine;

        private GameObject[] bones;
        private LineRenderer[] lines;

        private LineRenderer lineTLeft;
        private LineRenderer lineTRight;
        private LineRenderer lineFLeft;
        private LineRenderer lineFRight;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private Vector3 initialPosUser = Vector3.zero;
        private Vector3 initialPosOffset = Vector3.zero;
        private ulong initialPosUserID = 0;


        void Start()
        {
            //store bones in a list for easier access
            bones = new GameObject[] 
            {
                Pelvis,
                SpineNaval,
                SpineChest,
                Neck,
                Head,

                ClavicleLeft,
                ShoulderLeft,
                ElbowLeft,
                WristLeft,
                HandLeft,

                ClavicleRight,
                ShoulderRight,
                ElbowRight,
                WristRight,
                HandRight,

                HipLeft,
                KneeLeft,
                AnkleLeft,
                FootLeft,

                HipRight,
                KneeRight,
                AnkleRight,
                FootRight,

                Nose,
                EyeLeft,
                EarLeft,
                EyeRight,
                EarRight,

                HandtipLeft,
                ThumbLeft,
                HandtipRight,
                ThumbRight
            };

            // array holding the skeleton lines
            lines = new LineRenderer[bones.Length];

            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }


        void Update()
        {
            KinectManager kinectManager = KinectManager.Instance;

            // get 1st player
            ulong userID = kinectManager ? kinectManager.GetUserIdByIndex(playerIndex) : 0;

            if (userID == 0)
            {
                initialPosUserID = 0;
                initialPosOffset = Vector3.zero;
                initialPosUser = Vector3.zero;

                // reset the pointman position and rotation
                if (transform.position != initialPosition)
                {
                    transform.position = initialPosition;
                }

                if (transform.rotation != initialRotation)
                {
                    transform.rotation = initialRotation;
                }

                for (int i = 0; i < bones.Length; i++)
                {
                    bones[i].gameObject.SetActive(true);

                    bones[i].transform.localPosition = Vector3.zero;
                    bones[i].transform.localRotation = Quaternion.identity;

                    if (lines[i] != null)
                    {
                        lines[i].gameObject.SetActive(false);
                    }
                }

                return;
            }

            // set the position in space
            Vector3 posPointMan = kinectManager.GetUserPosition(userID);
            Vector3 posPointManMP = new Vector3(posPointMan.x, posPointMan.y, !mirroredMovement ? -posPointMan.z : posPointMan.z);

            // store the initial position
            if (initialPosUserID != userID)
            {
                initialPosUserID = userID;
                //initialPosOffset = transform.position - (verticalMovement ? posPointMan * moveRate : new Vector3(posPointMan.x, 0, posPointMan.z) * moveRate);
                initialPosOffset = posPointMan;

                initialPosUser = initialPosition;
                if (verticalMovement)
                    initialPosUser.y = 0f;  // posPointManMP.y provides the vertical position in this case
            }

            Vector3 relPosUser = (posPointMan - initialPosOffset);
            relPosUser.z = !mirroredMovement ? -relPosUser.z : relPosUser.z;

            transform.position = verticalMovement ? initialPosUser + posPointManMP * moveRate :
                initialPosUser + new Vector3(posPointManMP.x, 0, posPointManMP.z) * moveRate;

            //Debug.Log (userID + ", pos: " + posPointMan + ", ipos: " + initialPosUser + ", rpos: " + posPointManMP + ", tpos: " + transform.position);

            // update the local positions of the bones
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null)
                {
                    int joint = !mirroredMovement ? i : (int)KinectInterop.GetMirrorJoint((KinectInterop.JointType)i);
                    if (joint < 0)
                        continue;

                    if (kinectManager.IsJointTracked(userID, joint))
                    {
                        bones[i].gameObject.SetActive(true);

                        Vector3 posJoint = kinectManager.GetJointPosition(userID, joint);
                        posJoint.z = !mirroredMovement ? -posJoint.z : posJoint.z;

                        Quaternion rotJoint = kinectManager.GetJointOrientation(userID, joint, !mirroredMovement);
                        rotJoint = initialRotation * rotJoint;

                        posJoint -= posPointManMP;

                        if (mirroredMovement)
                        {
                            posJoint.x = -posJoint.x;
                            posJoint.z = -posJoint.z;
                        }

                        bones[i].transform.localPosition = posJoint;
                        bones[i].transform.rotation = rotJoint;

                        if (lines[i] == null && skeletonLine != null)
                        {
                            lines[i] = Instantiate(skeletonLine) as LineRenderer;
                            lines[i].transform.parent = transform;
                        }

                        if (lines[i] != null)
                        {
                            lines[i].gameObject.SetActive(true);
                            Vector3 posJoint2 = bones[i].transform.position;

                            Vector3 dirFromParent = kinectManager.GetJointDirection(userID, joint, false, false);
                            dirFromParent.z = !mirroredMovement ? -dirFromParent.z : dirFromParent.z;
                            Vector3 posParent = posJoint2 - dirFromParent;

                            //lines[i].SetVertexCount(2);
                            lines[i].SetPosition(0, posParent);
                            lines[i].SetPosition(1, posJoint2);
                        }

                    }
                    else
                    {
                        bones[i].gameObject.SetActive(false);

                        if (lines[i] != null)
                        {
                            lines[i].gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

    }
}
