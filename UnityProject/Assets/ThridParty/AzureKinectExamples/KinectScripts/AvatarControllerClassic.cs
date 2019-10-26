using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 


namespace com.rfilkov.components
{
    /// <summary>
    /// Avatar controller is the component that transfers the captured user motion to a humanoid model (avatar). Avatar controller classic allows manual assignment of model's rigged bones to the tracked body joints.
    /// </summary>
    public class AvatarControllerClassic : AvatarController
    {
        // Public variables that will get matched to bones. If empty, the Kinect will simply not track it.
        public Transform Pelvis;
        public Transform SpineNaval;
        public Transform SpineChest;
        public Transform Neck;
        //public Transform Head;

        public Transform ClavicleLeft;
        public Transform ShoulderLeft;
        public Transform ElbowLeft;
        public Transform WristLeft;

        public Transform ClavicleRight;
        public Transform ShoulderRight;
        public Transform ElbowRight;
        public Transform WristRight;

        public Transform HipLeft;
        public Transform KneeLeft;
        public Transform AnkleLeft;
        //private Transform FootLeft = null;

        public Transform HipRight;
        public Transform KneeRight;
        public Transform AnkleRight;
        //private Transform FootRight = null;

        [Tooltip("The body root node (optional).")]
        public Transform BodyRoot;


        // map the bones to the model.
        protected override void MapBones()
        {
            bones[0] = Pelvis;
            bones[1] = SpineNaval;
            bones[2] = SpineChest;
            bones[3] = Neck;
            //bones[4] = Head;

            bones[5] = ClavicleLeft;
            bones[6] = ShoulderLeft;
            bones[7] = ElbowLeft;
            bones[8] = WristLeft;

            bones[9] = ClavicleRight;
            bones[10] = ShoulderRight;
            bones[11] = ElbowRight;
            bones[12] = WristRight;

            bones[13] = HipLeft;
            bones[14] = KneeLeft;
            bones[15] = AnkleLeft;
            //bones[16] = FootLeft;

            bones[17] = HipRight;
            bones[18] = KneeRight;
            bones[19] = AnkleRight;
            //bones[20] = FootRight;

            // body root
            bodyRoot = BodyRoot;
        }

    }
}

