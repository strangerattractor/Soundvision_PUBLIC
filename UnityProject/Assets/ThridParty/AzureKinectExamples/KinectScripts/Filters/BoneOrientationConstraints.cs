using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace com.rfilkov.kinect
{
    /// <summary>
    /// Filter to correct the joint orientations to constraint to the range of plausible human motion.
    /// </summary>
    public class BoneOrientationConstraints
    {
        // constraint types
        public enum CT { None = 0, LimA = 1, LimST = 2, LimH = 3 }

        // list of joint constraints
        private readonly List<BoneOrientationConstraint> jointConstraints = new List<BoneOrientationConstraint>();

        private UnityEngine.UI.Text debugText;

        private long frameNum = 0;
        //private float currentTime = 0f;


        // Initializes a new instance of the BoneOrientationConstraints class.
        public BoneOrientationConstraints()
        {
        }

        public void SetDebugText(UnityEngine.UI.Text debugText)
        {
            this.debugText = debugText;
        }

        // AddDefaultConstraints - Adds a set of default joint constraints for normal human poses.  
        // This is a reasonable set of constraints for plausible human bio-mechanics.
        public void AddDefaultConstraints()
        {
            // SpineNaval
            AddBoneOrientationConstraint((int)KinectInterop.JointType.SpineNaval, CT.LimST, Vector3.up, 5f, 0f);

            // SpineChest
            AddBoneOrientationConstraint((int)KinectInterop.JointType.SpineChest, CT.LimST, Vector3.up, 5f, 0f);

            // Neck
            AddBoneOrientationConstraint((int)KinectInterop.JointType.Neck, CT.LimST, Vector3.up, 50f, 80f);

            // ShoulderLeft, ShoulderRight
            AddBoneOrientationConstraint((int)KinectInterop.JointType.ShoulderLeft, CT.LimST, Vector3.left, 180f, 180f);
            AddBoneOrientationConstraint((int)KinectInterop.JointType.ShoulderRight, CT.LimST, Vector3.right, 180f, 180f);

            // ElbowLeft, ElbowRight
            AddBoneOrientationConstraint((int)KinectInterop.JointType.ElbowLeft, CT.LimST, Vector3.left, 180f, 180f);
            AddBoneOrientationConstraint((int)KinectInterop.JointType.ElbowRight, CT.LimST, Vector3.right, 180f, 180f);

            // WristLeft, WristRight
            AddBoneOrientationConstraint((int)KinectInterop.JointType.WristLeft, CT.LimST, Vector3.left, 60f, 40f);
            AddBoneOrientationConstraint((int)KinectInterop.JointType.WristRight, CT.LimST, Vector3.right, 60f, 40f);

            // HandLeft, HandRight
            AddBoneOrientationConstraint((int)KinectInterop.JointType.HandLeft, CT.LimH, Vector3.forward, -5f, -80f);
            AddBoneOrientationConstraint((int)KinectInterop.JointType.HandRight, CT.LimH, Vector3.forward, 5f, 80f);

            // HipLeft, HipRight
            AddBoneOrientationConstraint((int)KinectInterop.JointType.HipLeft, CT.LimST, Vector3.down, 120f, 0f);
            AddBoneOrientationConstraint((int)KinectInterop.JointType.HipRight, CT.LimST, Vector3.down, 120f, 0f);

            // KneeLeft, KneeRight
            AddBoneOrientationConstraint((int)KinectInterop.JointType.KneeLeft, CT.LimH, Vector3.right, 0f, 150f);
            AddBoneOrientationConstraint((int)KinectInterop.JointType.KneeRight, CT.LimH, Vector3.right, 0f, 150f);

            //// AnkleLeft, AnkleRight
            ////AddBoneOrientationConstraint((int)KinectInterop.JointType.AnkleLeft, CT.LimST, Vector3.forward, 30f, 0f);
            ////AddBoneOrientationConstraint((int)KinectInterop.JointType.AnkleRight, CT.LimST, Vector3.forward, 30f, 0f);
            //AddBoneOrientationConstraint((int)KinectInterop.JointType.AnkleLeft, CT.LimA, Vector3.forward, -5f, 5f);  // lat
            //AddBoneOrientationConstraint((int)KinectInterop.JointType.AnkleLeft, CT.LimA, Vector3.right, -10f, 10f);  // sag
            //AddBoneOrientationConstraint((int)KinectInterop.JointType.AnkleLeft, CT.LimA, Vector3.up, -30f, 30f);  // rot
            //AddBoneOrientationConstraint((int)KinectInterop.JointType.AnkleRight, CT.LimA, Vector3.forward, -5f, 5f);  // lat
            //AddBoneOrientationConstraint((int)KinectInterop.JointType.AnkleRight, CT.LimA, Vector3.right, -10f, 10f);  // sag
            //AddBoneOrientationConstraint((int)KinectInterop.JointType.AnkleRight, CT.LimA, Vector3.up, -30f, 30f);  // rot
        }

        // Apply the orientation constraints
        public void Constrain(ref KinectInterop.BodyData bodyData)
        {
            KinectManager kinectManager = KinectManager.Instance;
            frameNum++;

            for (int i = 0; i < jointConstraints.Count; i++)
            {
                BoneOrientationConstraint jc = this.jointConstraints[i];

                if (jc.thisJoint == (int)KinectInterop.JointType.Pelvis || bodyData.joint[jc.thisJoint].normalRotation == Quaternion.identity)
                    continue;
                if (bodyData.joint[jc.thisJoint].trackingState == KinectInterop.TrackingState.NotTracked)
                    continue;

                int prevJoint = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)jc.thisJoint);
                if (bodyData.joint[prevJoint].trackingState == KinectInterop.TrackingState.NotTracked)
                    continue;

                Quaternion rotParentN = bodyData.joint[prevJoint].normalRotation;
                //Quaternion rotDefaultN = Quaternion.identity;  // Quaternion.FromToRotation(KinectInterop.JointBaseDir[prevJoint], KinectInterop.JointBaseDir[jc.thisJoint]);
                //rotParentN = rotParentN * rotDefaultN;

                Quaternion rotJointN = bodyData.joint[jc.thisJoint].normalRotation;
                Quaternion rotLocalN = Quaternion.Inverse(rotParentN) * rotJointN;
                Vector3 eulerAnglesN = rotLocalN.eulerAngles;

                bool isConstrained = false;
                //string sDebug = string.Empty;

                for (int a = 0; a < jc.axisConstrainrs.Count; a++)
                {
                    AxisOrientationConstraint ac = jc.axisConstrainrs[a];
                    Quaternion rotLimited = rotLocalN;

                    switch (ac.consType)
                    {
                        case 0:
                            break;

                        case CT.LimA:
                            eulerAnglesN = LimitAngles(eulerAnglesN, ac.axis, ac.angleMin, ac.angleMax);
                            rotLimited = Quaternion.Euler(eulerAnglesN);
                            break;

                        case CT.LimST:
                            rotLimited = LimitSwing(rotLocalN, ac.axis, ac.angleMin);
                            rotLimited = LimitTwist(rotLimited, ac.axis, ac.angleMax);
                            break;

                        case CT.LimH:
                            float lastAngle = bodyData.joint[jc.thisJoint].lastAngle;
                            rotLimited = LimitHinge(rotLocalN, ac.axis, ac.angleMin, ac.angleMax, ref lastAngle);
                            bodyData.joint[jc.thisJoint].lastAngle = lastAngle;
                            break;

                        default:
                            throw new Exception("Undefined constraint type found: " + (int)ac.consType);
                    }

                    if (rotLimited != rotLocalN)
                    {
                        rotLocalN = rotLimited;
                        isConstrained = true;
                    }
                }

                //if (sDebug.Length > 0)
                //{
                //    if (debugText != null && jc.thisJoint == (int)KinectInterop.JointType.ElbowLeft)
                //    {
                //        //					debugText.text = sDebug;
                //    }

                //    Debug.Log(sDebug);
                //}

                if (isConstrained)
                {
                    rotJointN = rotParentN * rotLocalN;

                    Vector3 eulerJoint = rotJointN.eulerAngles;
                    Vector3 eulerJointM = new Vector3(eulerJoint.x, -eulerJoint.y, -eulerJoint.z);
                    Quaternion rotJointM = Quaternion.Euler(eulerJointM);

                    // put it back into the bone orientations
                    bodyData.joint[jc.thisJoint].normalRotation = rotJointN;
                    bodyData.joint[jc.thisJoint].mirroredRotation = rotJointM;
                }

            }
        }

        // find the bone constraint structure for given joint
        // returns the structure index in the list, or -1 if the bone structure is not found
        private int FindBoneOrientationConstraint(int thisJoint)
        {
            for (int i = 0; i < jointConstraints.Count; i++)
            {
                if (jointConstraints[i].thisJoint == thisJoint)
                    return i;
            }

            return -1;
        }

        // AddJointConstraint - Adds a joint constraint to the system.  
        private void AddBoneOrientationConstraint(int thisJoint, CT consType, Vector3 axis, float angleMin, float angleMax)
        {
            int index = FindBoneOrientationConstraint(thisJoint);

            BoneOrientationConstraint jc = index >= 0 ? jointConstraints[index] : new BoneOrientationConstraint(thisJoint);

            if (index < 0)
            {
                index = jointConstraints.Count;
                jointConstraints.Add(jc);
            }

            AxisOrientationConstraint constraint = new AxisOrientationConstraint(consType, axis, angleMin, angleMax);
            jc.axisConstrainrs.Add(constraint);

            jointConstraints[index] = jc;
        }

        private Vector3 LimitAngles(Vector3 eulerAngles, Vector3 axis, float limitMin, float limitMax)
        {
            int iAxis = (axis.x != 0f) ? 0 : (axis.y != 0f) ? 1 : (axis.z != 0f) ? 2 : -1;

            if (iAxis >= 0)
            {
                float angle = eulerAngles[iAxis];
                if (angle > 180f)
                {
                    angle = angle - 360f;
                }

                float newAngle = Mathf.Clamp(angle, limitMin, limitMax);
                if (newAngle < 0f)
                {
                    newAngle += 360f;
                }

                eulerAngles[iAxis] = newAngle;
            }

            return eulerAngles;
        }

        private Quaternion LimitSwing(Quaternion rotation, Vector3 axis, float limit)
        {
            if (rotation == Quaternion.identity)
                return rotation;
            if (limit >= 180f)
                return rotation;

            Vector3 swingAxis = rotation * axis;

            Quaternion swingRot = Quaternion.FromToRotation(axis, swingAxis);
            Quaternion limSwingRot = Quaternion.RotateTowards(Quaternion.identity, swingRot, limit);
            Quaternion backRot = Quaternion.FromToRotation(swingAxis, limSwingRot * axis);

            return backRot * rotation;
        }

        private Quaternion LimitTwist(Quaternion rotation, Vector3 axis, float limit)
        {
            limit = Mathf.Clamp(limit, 0f, 180f);
            if (limit >= 180f)
                return rotation;

            Vector3 orthoAxis = new Vector3(axis.y, axis.z, axis.x);
            Vector3 orthoTangent = orthoAxis;

            Vector3 normal = rotation * axis;
            Vector3.OrthoNormalize(ref normal, ref orthoTangent);

            Vector3 rotOrthoTangent = rotation * orthoAxis;
            Vector3.OrthoNormalize(ref normal, ref rotOrthoTangent);

            Quaternion fixedRot = Quaternion.FromToRotation(rotOrthoTangent, orthoTangent) * rotation;

            if (limit <= 0f)
                return fixedRot;

            return Quaternion.RotateTowards(fixedRot, rotation, limit);
        }

        private Quaternion LimitHinge(Quaternion rotation, Vector3 axis, float limitMin, float limitMax, ref float lastAngle)
        {
            if (limitMin == 0f && limitMax == 0f)
                return Quaternion.AngleAxis(0, axis);

            Quaternion rotOnAxis = Quaternion.FromToRotation(rotation * axis, axis) * rotation; // limit-1
            Quaternion lastRotation = Quaternion.AngleAxis(lastAngle, axis);

            Quaternion rotAdded = rotOnAxis * Quaternion.Inverse(lastRotation);
            float rotAngle = Quaternion.Angle(Quaternion.identity, rotAdded);

            Vector3 secAxis = new Vector3(axis.z, axis.x, axis.y);
            Vector3 cross = Vector3.Cross(secAxis, axis);

            if (Vector3.Dot(rotAdded * secAxis, cross) > 0f)
            {
                rotAngle = -rotAngle;
            }

            rotAngle = Mathf.Clamp(lastAngle + rotAngle, limitMin, limitMax);

            return Quaternion.AngleAxis(rotAngle, axis);
        }

        private struct BoneOrientationConstraint
        {
            public int thisJoint;
            public List<AxisOrientationConstraint> axisConstrainrs;


            public BoneOrientationConstraint(int thisJoint)
            {
                this.thisJoint = thisJoint;
                axisConstrainrs = new List<AxisOrientationConstraint>();
            }
        }


        private struct AxisOrientationConstraint
        {
            public CT consType;
            public Vector3 axis;

            public float angleMin;
            public float angleMax;


            public AxisOrientationConstraint(CT consType, Vector3 axis, float angleMin, float angleMax)
            {
                this.consType = consType;
                this.axis = axis;

                // Set the min and max rotations in degrees
                this.angleMin = angleMin;
                this.angleMax = angleMax;
            }
        }

    }
}
