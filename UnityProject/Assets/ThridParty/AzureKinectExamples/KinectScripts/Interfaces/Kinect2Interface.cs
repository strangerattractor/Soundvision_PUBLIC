#if (UNITY_STANDALONE_WIN)
using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Runtime.InteropServices;
//using Microsoft.Kinect.Face;
using System.Collections.Generic;
using System;

namespace com.rfilkov.kinect
{
    public class Kinect2Interface : DepthSensorBase
    {
        // change this to false, if you aren't using Kinect-v2 only and want KM to check for available sensors
        public static bool sensorAlwaysAvailable = true;

        public KinectSensor kinectSensor;
        public CoordinateMapper coordMapper;

        private BodyFrameReader bodyFrameReader;
        private BodyIndexFrameReader bodyIndexFrameReader;
        private ColorFrameReader colorFrameReader;
        private DepthFrameReader depthFrameReader;
        private InfraredFrameReader infraredFrameReader;

        private MultiSourceFrameReader multiSourceFrameReader;
        private MultiSourceFrame multiSourceFrame;

        private ColorFrame msColorFrame = null;
        private DepthFrame msDepthFrame = null;
        private InfraredFrame msInfraredFrame = null;
        private BodyFrame msBodyFrame = null;
        private BodyIndexFrame msBodyIndexFrame = null;


        private int kinectBodyCount = 0;
        private int kinectJointCount = 0;
        private Body[] kinectBodyData;

        private bool floorPlaneDetected = false;
        private Windows.Kinect.Vector4 vFloorPlane;


        public override KinectInterop.DepthSensorPlatform GetSensorPlatform()
        {
            return KinectInterop.DepthSensorPlatform.KinectV2;
        }

        public override List<KinectInterop.SensorDeviceInfo> GetAvailableSensors()
        {
            List<KinectInterop.SensorDeviceInfo> alSensorInfo = new List<KinectInterop.SensorDeviceInfo>();
            KinectInterop.SensorDeviceInfo sensorInfo = new KinectInterop.SensorDeviceInfo();

            KinectSensor sensor = KinectSensor.GetDefault();
            if (sensor != null)
            {
                if (sensorAlwaysAvailable)
                {
                    sensorInfo.sensorId = "KinectV2";
                    sensorInfo.sensorName = "Kinect-v2 Sensor";
                    sensorInfo.sensorCaps = KinectInterop.FrameSource.TypeAll & ~KinectInterop.FrameSource.TypePose;
                    Debug.Log(string.Format("  D{0}: {1}, id: {2}", 0, sensorInfo.sensorName, sensorInfo.sensorId));

                    alSensorInfo.Add(sensorInfo);

                    return alSensorInfo;
                }
                else
                {
                    // check for available sensor
                    if (!sensor.IsOpen)
                    {
                        sensor.Open();
                    }

                    float fWaitTime = Time.realtimeSinceStartup + 3f;
                    while (!sensor.IsAvailable && Time.realtimeSinceStartup < fWaitTime)
                    {
                        // wait for availability
                    }

                    if (sensor.IsAvailable)
                    {
                        sensorInfo.sensorId = "KinectV2";
                        sensorInfo.sensorName = "Kinect-v2 Sensor";
                        sensorInfo.sensorCaps = KinectInterop.FrameSource.TypeAll & ~KinectInterop.FrameSource.TypePose;
                        Debug.Log(string.Format("  D{0}: {1}, id: {2}", 0, sensorInfo.sensorName, sensorInfo.sensorId));

                        alSensorInfo.Add(sensorInfo);
                    }

                    if (sensor.IsOpen)
                    {
                        sensor.Close();
                    }

                    fWaitTime = Time.realtimeSinceStartup + 3f;
                    while (sensor.IsOpen && Time.realtimeSinceStartup < fWaitTime)
                    {
                        // wait for sensor to close
                    }
                }

                sensor = null;
            }

            //if(alSensorInfo.Count == 0)
            //{
            //    Debug.Log("  No sensor devices found.");
            //}

            return alSensorInfo;
        }

        public override KinectInterop.SensorData OpenSensor(KinectInterop.FrameSource dwFlags, bool bSyncDepthAndColor, bool bSyncBodyAndDepth)
        {
            // save initial parameters
            base.OpenSensor(dwFlags, bSyncDepthAndColor, bSyncBodyAndDepth);

            if (deviceStreamingMode == KinectInterop.DeviceStreamingMode.PlayRecording)
            {
                Debug.LogError("Please use Kinect Studio v2.0 to play the sensor data recording!");
                return null;
            }

            List<KinectInterop.SensorDeviceInfo> alSensors = GetAvailableSensors();
            if (deviceIndex >= alSensors.Count)
            {
                Debug.Log("  D" + deviceIndex + " is not available. You can set the device index to -1, to disable it.");
                return null;
            }

            // try to get reference to the default sensor
            kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor == null)
            {
                Debug.Log("Kinect-v2 sensor not found!");
                return null;
            }

            KinectInterop.SensorData sensorData = new KinectInterop.SensorData();

            // get reference to the coordinate mapper
            coordMapper = kinectSensor.CoordinateMapper;

            // flip color & depth image vertically
            sensorData.colorImageScale = new Vector3(1f, -1f, 1f);
            sensorData.depthImageScale = new Vector3(1f, -1f, 1f);
            sensorData.infraredImageScale = new Vector3(1f, -1f, 1f);
            sensorData.sensorSpaceScale = new Vector3(1f, 1f, 1f);

            // depth camera offset & matrix z-flip
            sensorRotOffset = Vector3.zero;   // if for instance the depth camera is tilted downwards
            sensorRotFlipZ = false;
            sensorRotIgnoreY = false;

            // color
            var frameDesc = kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            sensorData.colorImageWidth = frameDesc.Width;
            sensorData.colorImageHeight = frameDesc.Height;

            sensorData.colorImageFormat = TextureFormat.RGBA32;
            sensorData.colorImageStride = 4;  // 4 bytes per pixel

            if ((dwFlags & KinectInterop.FrameSource.TypeColor) != 0)
            {
                if (!isSyncDepthAndColor)
                    colorFrameReader = kinectSensor.ColorFrameSource.OpenReader();

                rawColorImage = new byte[frameDesc.LengthInPixels * frameDesc.BytesPerPixel];

                sensorData.colorImageTexture = new Texture2D(sensorData.colorImageWidth, sensorData.colorImageHeight, TextureFormat.RGBA32, false);
                sensorData.colorImageTexture.wrapMode = TextureWrapMode.Clamp;
                sensorData.colorImageTexture.filterMode = FilterMode.Point;
            }

            // depth
            sensorData.depthImageWidth = kinectSensor.DepthFrameSource.FrameDescription.Width;
            sensorData.depthImageHeight = kinectSensor.DepthFrameSource.FrameDescription.Height;

            if ((dwFlags & KinectInterop.FrameSource.TypeDepth) != 0)
            {
                if (!isSyncDepthAndColor)
                    depthFrameReader = kinectSensor.DepthFrameSource.OpenReader();

                rawDepthImage = new ushort[kinectSensor.DepthFrameSource.FrameDescription.LengthInPixels];
                sensorData.depthImage = new ushort[kinectSensor.DepthFrameSource.FrameDescription.LengthInPixels];
            }

            // infrared
            if ((dwFlags & KinectInterop.FrameSource.TypeInfrared) != 0)
            {
                if (!isSyncDepthAndColor)
                    infraredFrameReader = kinectSensor.InfraredFrameSource.OpenReader();

                rawInfraredImage = new ushort[kinectSensor.InfraredFrameSource.FrameDescription.LengthInPixels];
                sensorData.infraredImage = new ushort[kinectSensor.InfraredFrameSource.FrameDescription.LengthInPixels];

                minInfraredValue = 0f;
                maxInfraredValue = 10000f;
            }

            if ((dwFlags & KinectInterop.FrameSource.TypeBodyIndex) != 0)
            {
                if (!(isSyncDepthAndColor && isSyncBodyAndDepth))
                    bodyIndexFrameReader = kinectSensor.BodyIndexFrameSource.OpenReader();

                //rawBodyIndexImage = new byte[kinectSensor.BodyIndexFrameSource.FrameDescription.LengthInPixels];  // created by InitBodyTracking()
            }

            if ((dwFlags & KinectInterop.FrameSource.TypeBody) != 0)
            {
                if (!(isSyncDepthAndColor && isSyncBodyAndDepth))
                    bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();

                kinectBodyCount = 6;
                kinectJointCount = 25;
                kinectBodyData = new Body[kinectBodyCount];

                // init body tracking data
                InitBodyTracking(dwFlags, sensorData, new Microsoft.Azure.Kinect.Sensor.Calibration(), false);
            }

            //if(!kinectSensor.IsOpen)
            {
                //Debug.Log("Opening sensor, available: " + kinectSensor.IsAvailable);
                kinectSensor.Open();
            }

            float fWaitTime = Time.realtimeSinceStartup + 3f;
            while (!kinectSensor.IsAvailable && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for sensor to be available
            }

            //fWaitTime = Time.realtimeSinceStartup + 3f;
            while (!kinectSensor.IsOpen && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for sensor to open
            }

            Debug.Log("K2-sensor " + (kinectSensor.IsOpen ? "opened" : "closed") +
                      ", available: " + kinectSensor.IsAvailable);

            if (isSyncDepthAndColor && dwFlags != KinectInterop.FrameSource.TypeNone && kinectSensor.IsOpen)
            {
                multiSourceFrameReader = kinectSensor.OpenMultiSourceFrameReader((FrameSourceTypes)((int)dwFlags & 0x3F));
            }

            //if (deviceMode == KinectInterop.DepthSensorMode.CreateRecording)
            //{
            //    Debug.LogWarning("Please use Kinect Studio v2.0 to save sensor data recordings.");
            //}

            return sensorData;
        }

        public override void CloseSensor(KinectInterop.SensorData sensorData)
        {
            base.CloseSensor(sensorData);

            if (coordMapper != null)
            {
                coordMapper = null;
            }

            if (colorFrameReader != null)
            {
                colorFrameReader.Dispose();
                colorFrameReader = null;
            }

            if (depthFrameReader != null)
            {
                depthFrameReader.Dispose();
                depthFrameReader = null;
            }

            if (infraredFrameReader != null)
            {
                infraredFrameReader.Dispose();
                infraredFrameReader = null;
            }

            if (bodyFrameReader != null)
            {
                bodyFrameReader.Dispose();
                bodyFrameReader = null;
            }

            if (bodyIndexFrameReader != null)
            {
                bodyIndexFrameReader.Dispose();
                bodyIndexFrameReader = null;
            }

            if (multiSourceFrameReader != null)
            {
                multiSourceFrameReader.Dispose();
                multiSourceFrameReader = null;
            }

            if (kinectSensor != null)
            {
                //if (kinectSensor.IsOpen)
                {
                    //Debug.Log("Closing sensor, available: " + kinectSensor.IsAvailable);
                    kinectSensor.Close();
                }

                float fWaitTime = Time.realtimeSinceStartup + 3f;
                while (kinectSensor.IsOpen && Time.realtimeSinceStartup < fWaitTime)
                {
                    // wait for sensor to close
                }

                Debug.Log("K2-sensor " + (kinectSensor.IsOpen ? "opened" : "closed") +
                          ", available: " + kinectSensor.IsAvailable);

                kinectSensor = null;
            }
        }

        public override void PollSensorFrames(KinectInterop.SensorData sensorData)
        {
            // check for single-source or multi-source frames
            if (multiSourceFrameReader == null)
            {
                // single source - check for color frame
                if (colorFrameReader != null)
                {
                    msColorFrame = colorFrameReader.AcquireLatestFrame();
                }

                // check for depth frame
                if (depthFrameReader != null)
                {
                    msDepthFrame = depthFrameReader.AcquireLatestFrame();
                }

                // check for IR frame
                if (infraredFrameReader != null)
                {
                    msInfraredFrame = infraredFrameReader.AcquireLatestFrame();
                }

                // check for body index frame
                if (bodyIndexFrameReader != null)
                {
                    msBodyIndexFrame = bodyIndexFrameReader.AcquireLatestFrame();
                }

                // check for body frame
                if (bodyFrameReader != null)
                {
                    msBodyFrame = bodyFrameReader.AcquireLatestFrame();
                }

                // process currently read sensor frames
                ProcessSensorFrames(sensorData);
            }
            else
            {
                // multi-source frames
                multiSourceFrame = multiSourceFrameReader.AcquireLatestFrame();

                if (multiSourceFrame != null)
                {
                    // try to get all frames at once
                    msColorFrame = (frameSourceFlags & KinectInterop.FrameSource.TypeColor) != 0 ? multiSourceFrame.ColorFrameReference.AcquireFrame() : null;
                    msDepthFrame = (frameSourceFlags & KinectInterop.FrameSource.TypeDepth) != 0 ? multiSourceFrame.DepthFrameReference.AcquireFrame() : null;
                    msInfraredFrame = (frameSourceFlags & KinectInterop.FrameSource.TypeInfrared) != 0 ? multiSourceFrame.InfraredFrameReference.AcquireFrame() : null;
                    msBodyFrame = (frameSourceFlags & KinectInterop.FrameSource.TypeBody) != 0 ? multiSourceFrame.BodyFrameReference.AcquireFrame() : null;
                    msBodyIndexFrame = (frameSourceFlags & KinectInterop.FrameSource.TypeBodyIndex) != 0 ? multiSourceFrame.BodyIndexFrameReference.AcquireFrame() : null;

                    bool bAllSet =
                        ((frameSourceFlags & KinectInterop.FrameSource.TypeColor) == 0 || msColorFrame != null) &&
                        ((frameSourceFlags & KinectInterop.FrameSource.TypeDepth) == 0 || msDepthFrame != null) &&
                        ((frameSourceFlags & KinectInterop.FrameSource.TypeInfrared) == 0 || msInfraredFrame != null);

                    if(isSyncBodyAndDepth)
                    {
                        bAllSet &= ((frameSourceFlags & KinectInterop.FrameSource.TypeBody) == 0 || msBodyFrame != null) &&
                        ((frameSourceFlags & KinectInterop.FrameSource.TypeBodyIndex) == 0 || msBodyIndexFrame != null);
                    }

                    if (bAllSet)
                    {
                        // process currently read sensor frames
                        ProcessSensorFrames(sensorData);
                    }

                    //release all frames
                    if (msColorFrame != null)
                    {
                        msColorFrame.Dispose();
                        msColorFrame = null;
                    }

                    if (msDepthFrame != null)
                    {
                        msDepthFrame.Dispose();
                        msDepthFrame = null;
                    }

                    if (msInfraredFrame != null)
                    {
                        msInfraredFrame.Dispose();
                        msInfraredFrame = null;
                    }

                    if (msBodyFrame != null)
                    {
                        msBodyFrame.Dispose();
                        msBodyFrame = null;
                    }

                    if (msBodyIndexFrame != null)
                    {
                        msBodyIndexFrame.Dispose();
                        msBodyIndexFrame = null;
                    }

                    if (multiSourceFrame != null)
                    {
                        multiSourceFrame = null;
                    }
                }
            }
        }

        // processes the currently read sensor frames
        // todo: provide thread sync
        private void ProcessSensorFrames(KinectInterop.SensorData sensorData)
        {
            // color frame
            if (msColorFrame != null)
            {
                if(rawColorTimestamp == sensorData.lastColorFrameTime)
                {
                    lock (colorFrameLock)
                    {
                        var pColorData = GCHandle.Alloc(rawColorImage, GCHandleType.Pinned);
                        msColorFrame.CopyConvertedFrameDataToIntPtr(pColorData.AddrOfPinnedObject(), (uint)rawColorImage.Length, ColorImageFormat.Rgba);
                        pColorData.Free();

                        rawColorTimestamp = (ulong)msColorFrame.RelativeTime.Ticks;
                        //Debug.Log("RawColorTimestamp: " + rawColorTimestamp);
                    }
                }

                msColorFrame.Dispose();
                msColorFrame = null;
            }

            // depth frame
            if (msDepthFrame != null)
            {
                if(rawDepthTimestamp == sensorData.lastDepthFrameTime)
                {
                    lock (depthFrameLock)
                    {
                        var pDepthData = GCHandle.Alloc(rawDepthImage, GCHandleType.Pinned);
                        msDepthFrame.CopyFrameDataToIntPtr(pDepthData.AddrOfPinnedObject(), (uint)rawDepthImage.Length * sizeof(ushort));
                        pDepthData.Free();

                        rawDepthTimestamp = (ulong)msDepthFrame.RelativeTime.Ticks;
                        //Debug.Log("RawDepthTimestamp: " + rawDepthTimestamp);
                    }
                }

                msDepthFrame.Dispose();
                msDepthFrame = null;
            }

            // infrared frame
            if (msInfraredFrame != null)
            {
                if(rawInfraredTimestamp == sensorData.lastInfraredFrameTime)
                {
                    lock (infraredFrameLock)
                    {
                        var pInfraredData = GCHandle.Alloc(rawInfraredImage, GCHandleType.Pinned);
                        msInfraredFrame.CopyFrameDataToIntPtr(pInfraredData.AddrOfPinnedObject(), (uint)rawInfraredImage.Length * sizeof(ushort));
                        pInfraredData.Free();

                        rawInfraredTimestamp = (ulong)msInfraredFrame.RelativeTime.Ticks;
                        //Debug.Log("RawInfraredTimestamp: " + rawInfraredTimestamp);
                    }
                }

                msInfraredFrame.Dispose();
                msInfraredFrame = null;
            }

            // body index frame
            bool bProcessBodyFrame = rawBodyTimestamp == sensorData.lastBodyFrameTime;

            if (msBodyIndexFrame != null)
            {
                if(bProcessBodyFrame)
                {
                    lock (bodyTrackerLock)
                    {
                        var pBodyIndexData = GCHandle.Alloc(rawBodyIndexImage, GCHandleType.Pinned);
                        msBodyIndexFrame.CopyFrameDataToIntPtr(pBodyIndexData.AddrOfPinnedObject(), (uint)rawBodyIndexImage.Length);
                        pBodyIndexData.Free();

                        //rawBodyTimestamp = (ulong)msBodyIndexFrame.RelativeTime.Ticks;
                        //Debug.Log("RawBodyIndexTimestamp: " + rawBodyTimestamp);
                    }
                }

                msBodyIndexFrame.Dispose();
                msBodyIndexFrame = null;
            }

            // body frame
            if (msBodyFrame != null)
            {
                if (bProcessBodyFrame)
                {
                    lock (bodyTrackerLock)
                    {
                        ProcessBodyFrame(msBodyFrame, sensorData);
                    }

                    if (floorPlaneDetected)
                    {
                        lock (poseFrameLock)
                        {
                            // update the sensor pose
                            if(vFloorPlane.X != 0f || vFloorPlane.Y != 0f || vFloorPlane.Z != 0f)
                            {
                                Vector3 vFloorNormal = new Vector3(vFloorPlane.X, vFloorPlane.Y, vFloorPlane.Z);
                                rawPoseRotation = Quaternion.FromToRotation(vFloorNormal, Vector3.up);

                                if (vFloorPlane.W != 0f)
                                {
                                    rawPosePosition = new Vector3(0f, vFloorPlane.W, 0f) - initialPosePosition;
                                }

                                rawPoseTimestamp = rawBodyTimestamp;
                            }
                        }
                    }
                }

                msBodyFrame.Dispose();
                msBodyFrame = null;
            }
        }


        // processes the acquired body frame
        private void ProcessBodyFrame(BodyFrame frame, KinectInterop.SensorData sensorData)
        {
            frame.GetAndRefreshBodyData(kinectBodyData);
            rawBodyTimestamp = (ulong)frame.RelativeTime.Ticks;
            //Debug.Log("RawBodyTimestamp: " + rawBodyTimestamp);

            // get the floor plane
            vFloorPlane = frame.FloorClipPlane;
            floorPlaneDetected = true;

            frame.Dispose();
            frame = null;

            //Debug.Log("rawBodyTimestamp: " + rawBodyTimestamp);

            // get sensor-to-world matrix
            Matrix4x4 sensorToWorld = GetSensorToWorldMatrix();
            float scaleX = sensorData.depthImageScale.x;
            //float scaleY = sensorData.depthImageScale.y;

            // create the needed slots
            while (alTrackedBodies.Count < kinectBodyCount)
            {
                alTrackedBodies.Add(new KinectInterop.BodyData((int)KinectInterop.JointType.Count));
            }

            trackedBodiesCount = 0;

            for (int i = 0; i < kinectBodyCount; i++)
            {
                Body body = kinectBodyData[i];

                if (body == null)
                    continue;

                KinectInterop.BodyData bodyData = alTrackedBodies[i];

                bodyData.liTrackingID = body.TrackingId;
                bodyData.iBodyIndex = i;
                bodyData.bIsTracked = body.IsTracked;

                if (!bodyData.bIsTracked)
                    continue;

                // cache the body joints (following the advice of Brian Chasalow)
                Dictionary<Windows.Kinect.JointType, Windows.Kinect.Joint> bodyJoints = body.Joints;

                for (int jKJ = 0; jKJ < kinectJointCount; jKJ++)
                {
                    Windows.Kinect.Joint joint = bodyJoints[(Windows.Kinect.JointType)jKJ];

                    int j = KinectJoint2JointType[jKJ];

                    if (j >= 0)
                    {
                        KinectInterop.JointData jointData = bodyData.joint[j];

                        jointData.trackingState = (KinectInterop.TrackingState)joint.TrackingState;

                        float jPosZ = (bIgnoreZCoordinates && j > 0) ? bodyData.joint[0].kinectPos.z : joint.Position.Z;
                        jointData.kinectPos = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        jointData.position = sensorToWorld.MultiplyPoint3x4(new Vector3(joint.Position.X * scaleX, joint.Position.Y, jPosZ));

                        jointData.orientation = Quaternion.identity;

                        if (j == 0)
                        {
                            bodyData.kinectPos = jointData.kinectPos;
                            bodyData.position = jointData.position;
                            bodyData.orientation = jointData.orientation;
                            //floorPlaneDetected = true;
                        }

                        bodyData.joint[j] = jointData;
                    }
                }

                bodyJoints.Clear();

                // estimate additional joints
                CalcBodySpecialJoints(ref bodyData);

                // calculate bone dirs
                KinectInterop.CalcBodyJointDirs(ref bodyData);

                // calculate joint orientations
                CalcBodyJointOrients(ref bodyData);

                // body orientation
                bodyData.normalRotation = bodyData.joint[0].normalRotation;
                bodyData.mirroredRotation = bodyData.joint[0].mirroredRotation;

                alTrackedBodies[(int)trackedBodiesCount] = bodyData;
                trackedBodiesCount++;

                //Debug.Log("  (T)User ID: " + bodyData.liTrackingID + ", body: " + (trackedBodiesCount - 1) + ", pos: " + bodyData.kinectPos);
            }
        }

        // estimates additional joints for the given body
        protected override void CalcBodySpecialJoints(ref KinectInterop.BodyData bodyData)
        {
            // clavicle right
            {
                int l = (int)KinectInterop.JointType.ClavicleLeft;
                int r = (int)KinectInterop.JointType.ClavicleRight;

                KinectInterop.JointData jointData = bodyData.joint[r];
                jointData.trackingState = bodyData.joint[l].trackingState;
                jointData.orientation = bodyData.joint[l].orientation;

                jointData.kinectPos = bodyData.joint[l].kinectPos;
                jointData.position = bodyData.joint[l].position;

                bodyData.joint[r] = jointData;
            }

            // spine naval
            {
                int p = (int)KinectInterop.JointType.Pelvis;
                int sc = (int)KinectInterop.JointType.SpineChest;
                int sn = (int)KinectInterop.JointType.SpineNaval;

                KinectInterop.JointData jointData = bodyData.joint[sn];
                jointData.trackingState = bodyData.joint[sc].trackingState;
                jointData.orientation = bodyData.joint[sc].orientation;

                Vector3 posChest = bodyData.joint[sc].kinectPos;
                Vector3 posPelvis = bodyData.joint[p].kinectPos;
                jointData.kinectPos = (posPelvis + posChest) * 0.5f;

                posChest = bodyData.joint[sc].position;
                posPelvis = bodyData.joint[p].position;
                jointData.position = (posPelvis + posChest) * 0.5f;

                bodyData.joint[sn] = jointData;
            }
        }

        // calculates all joint orientations for the given body
        protected override void CalcBodyJointOrients(ref KinectInterop.BodyData bodyData)
        {
            int jointCount = bodyData.joint.Length;

            Vector3 posRShoulder = bodyData.joint[(int)KinectInterop.JointType.ShoulderRight].position;
            Vector3 posLShoulder = bodyData.joint[(int)KinectInterop.JointType.ShoulderLeft].position;
            Vector3 shouldersDirection = posRShoulder - posLShoulder;
            shouldersDirection -= Vector3.Project(shouldersDirection, Vector3.up);

            for (int j = 0; j < jointCount; j++)
            {
                int joint = j;

                KinectInterop.JointData jointData = bodyData.joint[joint];
                bool bJointValid = bIgnoreInferredJoints ? jointData.trackingState == KinectInterop.TrackingState.Tracked : jointData.trackingState != KinectInterop.TrackingState.NotTracked;

                if (bJointValid)
                {
                    int nextJoint = (int)KinectInterop.GetNextJoint((KinectInterop.JointType)joint);
                    if (nextJoint != joint && nextJoint >= 0 && nextJoint < jointCount)
                    {
                        KinectInterop.JointData nextJointData = bodyData.joint[nextJoint];
                        bool bNextJointValid = bIgnoreInferredJoints ? nextJointData.trackingState == KinectInterop.TrackingState.Tracked : nextJointData.trackingState != KinectInterop.TrackingState.NotTracked;

                        Vector3 baseDir = KinectJointBaseDir[nextJoint];
                        Vector3 jointDir = nextJointData.direction.normalized;
                        jointDir = new Vector3(jointDir.x, jointDir.y, -jointDir.z).normalized;

                        Quaternion jointOrientNormal = jointData.normalRotation;
                        if (bNextJointValid)
                        {
                            jointOrientNormal = Quaternion.FromToRotation(baseDir, jointDir);
                        }

                        if ((joint == (int)KinectInterop.JointType.ShoulderLeft) ||
                            (joint == (int)KinectInterop.JointType.ElbowLeft) ||
                            (joint == (int)KinectInterop.JointType.WristLeft) ||
                            (joint == (int)KinectInterop.JointType.HandLeft))
                        {
                            if (bNextJointValid && jointData.direction != Vector3.zero && jointDir != Vector3.zero)
                            {
                                Vector3 parJointDir = jointData.direction.normalized;
                                parJointDir = new Vector3(parJointDir.x, parJointDir.y, -parJointDir.z).normalized;

                                if (joint == (int)KinectInterop.JointType.WristLeft)
                                {
                                    // for wrist, take the finger direction into account, too
                                    int fingerJoint = (int)KinectInterop.GetNextJoint((KinectInterop.JointType)nextJoint);

                                    if (fingerJoint != joint && fingerJoint >= 0 && fingerJoint < jointCount)
                                    {
                                        KinectInterop.JointData fingerData = bodyData.joint[fingerJoint];
                                        if (fingerData.trackingState != KinectInterop.TrackingState.NotTracked)
                                        {
                                            jointDir = (nextJointData.direction + fingerData.direction).normalized;
                                            jointDir = new Vector3(jointDir.x, jointDir.y, -jointDir.z).normalized;
                                        }
                                    }
                                }

                                float parDotJoint = Vector3.Dot(parJointDir, jointDir);
                                //Debug.Log (joint + ": " + parDotJoint);

                                if ((parDotJoint >= 0.01f && parDotJoint <= 0.99f) || (parDotJoint >= -0.99f && parDotJoint <= -0.01f))
                                {
                                    if (joint != (int)KinectInterop.JointType.ShoulderLeft && parJointDir != Vector3.zero)
                                    {
                                        Vector3 upDir = -Vector3.Cross(-parJointDir, jointDir).normalized;
                                        Vector3 fwdDir = Vector3.Cross(-jointDir, upDir).normalized;
                                        jointOrientNormal = Quaternion.LookRotation(fwdDir, upDir);
                                    }
                                    else
                                    {
                                        KinectInterop.JointData shCenterData = bodyData.joint[(int)KinectInterop.JointType.ClavicleLeft];

                                        Vector3 spineDir = shCenterData.direction.normalized;
                                        spineDir = new Vector3(spineDir.x, spineDir.y, -spineDir.z).normalized;

                                        Vector3 fwdDir = Vector3.Cross(-jointDir, spineDir).normalized;
                                        Vector3 upDir = Vector3.Cross(fwdDir, -jointDir).normalized;
                                        jointOrientNormal = Quaternion.LookRotation(fwdDir, upDir);
                                    }

                                    jointData.normalRotation = jointOrientNormal;
                                }
                            }

                            // allowedHandRotations = All (left wrist/hand)
                            if (joint == (int)KinectInterop.JointType.WristLeft || joint == (int)KinectInterop.JointType.HandLeft)
                            {
                                KinectInterop.JointData thumbData = bodyData.joint[(int)KinectInterop.JointType.ThumbLeft];

                                int prevJoint = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)joint);
                                KinectInterop.JointData prevJointData = bodyData.joint[prevJoint];

                                if (thumbData.trackingState != KinectInterop.TrackingState.NotTracked &&
                                    prevJointData.trackingState != KinectInterop.TrackingState.NotTracked)
                                {
                                    Vector3 rightDir = -jointDir;
                                    Vector3 fwdDir = thumbData.direction.normalized;
                                    fwdDir = new Vector3(fwdDir.x, fwdDir.y, -fwdDir.z).normalized;

                                    if (joint == (int)KinectInterop.JointType.HandLeft)
                                    {
                                        Vector3 prevBaseDir = -Vector3.left;  // - KinectInterop.JointBaseDir[prevJoint];
                                        Vector3 prevOrthoDir = new Vector3(prevBaseDir.y, prevBaseDir.z, prevBaseDir.x);
                                        fwdDir = prevJointData.normalRotation * prevOrthoDir;
                                        //rightDir -= Vector3.Project(rightDir, fwdDir);
                                    }

                                    if (rightDir != Vector3.zero && fwdDir != Vector3.zero)
                                    {
                                        Vector3 upDir = Vector3.Cross(fwdDir, rightDir).normalized;
                                        fwdDir = Vector3.Cross(rightDir, upDir).normalized;

                                        //jointData.normalRotation = Quaternion.LookRotation(fwdDir, upDir);
                                        Quaternion jointOrientThumb = Quaternion.LookRotation(fwdDir, upDir);
                                        jointOrientNormal = (joint == (int)KinectInterop.JointType.WristLeft) ?
                                            Quaternion.RotateTowards(prevJointData.normalRotation, jointOrientThumb, 80f) : jointOrientThumb;

                                        jointData.normalRotation = jointOrientNormal;
                                    }
                                }

                                //bRotated = true;
                            }

                            if (joint == (int)KinectInterop.JointType.WristLeft || joint == (int)KinectInterop.JointType.HandLeft)
                            {
                                // limit wrist and hand twist
                                int prevJoint = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)joint);
                                KinectInterop.JointData prevJointData = bodyData.joint[prevJoint];

                                if (prevJointData.trackingState != KinectInterop.TrackingState.NotTracked)
                                {
                                    jointData.normalRotation = Quaternion.RotateTowards(prevJointData.normalRotation, jointData.normalRotation, 70f);
                                }
                            }

                        }
                        else if ((joint == (int)KinectInterop.JointType.ShoulderRight) ||
                            (joint == (int)KinectInterop.JointType.ElbowRight) ||
                            (joint == (int)KinectInterop.JointType.WristRight) ||
                            (joint == (int)KinectInterop.JointType.HandRight))
                        {
                            if (bNextJointValid && jointData.direction != Vector3.zero && jointDir != Vector3.zero)
                            {
                                Vector3 parJointDir = jointData.direction.normalized;
                                parJointDir = new Vector3(parJointDir.x, parJointDir.y, -parJointDir.z).normalized;

                                if (joint == (int)KinectInterop.JointType.WristRight)
                                {
                                    // for wrist, take the finger direction into account, too
                                    int fingerJoint = (int)KinectInterop.GetNextJoint((KinectInterop.JointType)nextJoint);

                                    if (fingerJoint != joint && fingerJoint >= 0 && fingerJoint < jointCount)
                                    {
                                        KinectInterop.JointData fingerData = bodyData.joint[fingerJoint];
                                        if (fingerData.trackingState != KinectInterop.TrackingState.NotTracked)
                                        {
                                            jointDir = (nextJointData.direction + fingerData.direction).normalized;
                                            jointDir = new Vector3(jointDir.x, jointDir.y, -jointDir.z).normalized;
                                        }
                                    }
                                }

                                float parDotJoint = Vector3.Dot(parJointDir, jointDir);
                                //Debug.Log (joint + ": " + parDotJoint);

                                if ((parDotJoint >= 0.01f && parDotJoint <= 0.99f) || (parDotJoint >= -0.99f && parDotJoint <= -0.01f))
                                {
                                    if (joint != (int)KinectInterop.JointType.ShoulderRight && parJointDir != Vector3.zero)
                                    {
                                        Vector3 upDir = -Vector3.Cross(parJointDir, jointDir).normalized;
                                        Vector3 fwdDir = Vector3.Cross(jointDir, upDir).normalized;
                                        jointOrientNormal = Quaternion.LookRotation(fwdDir, upDir);
                                    }
                                    else
                                    {
                                        KinectInterop.JointData shCenterData = bodyData.joint[(int)KinectInterop.JointType.ClavicleLeft];

                                        Vector3 spineDir = shCenterData.direction.normalized;
                                        spineDir = new Vector3(spineDir.x, spineDir.y, -spineDir.z).normalized;

                                        Vector3 fwdDir = Vector3.Cross(jointDir, spineDir).normalized;
                                        Vector3 upDir = Vector3.Cross(fwdDir, jointDir).normalized;
                                        jointOrientNormal = Quaternion.LookRotation(fwdDir, upDir);
                                    }

                                    jointData.normalRotation = jointOrientNormal;
                                }
                            }

                            // allowedHandRotations = All (right wrist/hand)
                            if (joint == (int)KinectInterop.JointType.WristRight || joint == (int)KinectInterop.JointType.HandRight)
                            {
                                KinectInterop.JointData thumbData = bodyData.joint[(int)KinectInterop.JointType.ThumbRight];

                                int prevJoint = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)joint);
                                KinectInterop.JointData prevJointData = bodyData.joint[prevJoint];

                                if (thumbData.trackingState != KinectInterop.TrackingState.NotTracked &&
                                    prevJointData.trackingState != KinectInterop.TrackingState.NotTracked)
                                {
                                    Vector3 rightDir = jointDir;
                                    Vector3 fwdDir = thumbData.direction.normalized;
                                    fwdDir = new Vector3(fwdDir.x, fwdDir.y, -fwdDir.z).normalized;

                                    if (joint == (int)KinectInterop.JointType.HandRight)
                                    {
                                        Vector3 prevBaseDir = Vector3.right;  // KinectInterop.JointBaseDir[prevJoint];
                                        Vector3 prevOrthoDir = new Vector3(prevBaseDir.y, prevBaseDir.z, prevBaseDir.x);
                                        fwdDir = prevJointData.normalRotation * prevOrthoDir;
                                        //rightDir -= Vector3.Project(rightDir, fwdDir);
                                    }

                                    if (rightDir != Vector3.zero && fwdDir != Vector3.zero)
                                    {
                                        Vector3 upDir = Vector3.Cross(fwdDir, rightDir).normalized;
                                        fwdDir = Vector3.Cross(rightDir, upDir).normalized;

                                        Quaternion jointOrientThumb = Quaternion.LookRotation(fwdDir, upDir);
                                        jointOrientNormal = (joint == (int)KinectInterop.JointType.WristRight) ?
                                            Quaternion.RotateTowards(prevJointData.normalRotation, jointOrientThumb, 80f) : jointOrientThumb;

                                        jointData.normalRotation = jointOrientNormal;
                                    }
                                }

                                //bRotated = true;
                            }

                            if (joint == (int)KinectInterop.JointType.WristRight || joint == (int)KinectInterop.JointType.HandRight)
                            {
                                // limit wrist and hand twist
                                int prevJoint = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)joint);
                                KinectInterop.JointData prevJointData = bodyData.joint[prevJoint];

                                if (prevJointData.trackingState != KinectInterop.TrackingState.NotTracked)
                                {
                                    jointData.normalRotation = Quaternion.RotateTowards(prevJointData.normalRotation, jointData.normalRotation, 70f);
                                }
                            }

                        }
                        else
                        {
                            jointData.normalRotation = jointOrientNormal;
                        }

                        if ((joint == (int)KinectInterop.JointType.Pelvis) ||
                            (joint == (int)KinectInterop.JointType.SpineNaval) ||
                            (joint == (int)KinectInterop.JointType.SpineChest) ||
                            (joint == (int)KinectInterop.JointType.ClavicleLeft) ||
                            (joint == (int)KinectInterop.JointType.ClavicleRight) ||
                            (joint == (int)KinectInterop.JointType.Neck))
                        {
                            Vector3 baseDir2 = Vector3.right;
                            Vector3 jointDir2 = shouldersDirection;
                            jointDir2.z = -jointDir2.z;

                            jointData.normalRotation *= Quaternion.FromToRotation(baseDir2, jointDir2);
                        }
                        else if ((joint == (int)KinectInterop.JointType.HipLeft) || (joint == (int)KinectInterop.JointType.HipRight) ||
                            (joint == (int)KinectInterop.JointType.KneeLeft) || (joint == (int)KinectInterop.JointType.KneeRight) ||
                            (joint == (int)KinectInterop.JointType.AnkleLeft) || (joint == (int)KinectInterop.JointType.AnkleRight))
                        {
                            Vector3 baseDir2 = Vector3.right;
                            Vector3 jointDir2 = shouldersDirection;
                            jointDir2.z = -jointDir2.z;

                            jointData.normalRotation *= Quaternion.FromToRotation(baseDir2, jointDir2);
                        }

                        Vector3 mirroredAngles = jointData.normalRotation.eulerAngles;
                        mirroredAngles.y = -mirroredAngles.y;
                        mirroredAngles.z = -mirroredAngles.z;

                        jointData.mirroredRotation = Quaternion.Euler(mirroredAngles);
                    }
                    else
                    {
                        // get the orientation of the parent joint
                        int prevJoint = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)joint);
                        if (prevJoint != joint && prevJoint >= 0 && prevJoint < jointCount)
                        {
                            jointData.normalRotation = bodyData.joint[prevJoint].normalRotation;
                            jointData.mirroredRotation = bodyData.joint[prevJoint].mirroredRotation;
                        }
                        else
                        {
                            jointData.normalRotation = Quaternion.identity;
                            jointData.mirroredRotation = Quaternion.identity;
                        }
                    }

                    bodyData.joint[joint] = jointData;
                }
                else
                {
                    // joint is not tracked
                }

                if (joint == (int)KinectInterop.JointType.Pelvis)
                {
                    bodyData.normalRotation = jointData.normalRotation;
                    bodyData.mirroredRotation = jointData.mirroredRotation;
                }
            }
        }


        protected static readonly int[] KinectJoint2JointType =
        {
            (int)KinectInterop.JointType.Pelvis,
            (int)KinectInterop.JointType.SpineChest,
            (int)KinectInterop.JointType.Neck,
            (int)KinectInterop.JointType.Head,

            (int)KinectInterop.JointType.ShoulderLeft,
            (int)KinectInterop.JointType.ElbowLeft,
            (int)KinectInterop.JointType.WristLeft,
            (int)KinectInterop.JointType.HandLeft,

            (int)KinectInterop.JointType.ShoulderRight,
            (int)KinectInterop.JointType.ElbowRight,
            (int)KinectInterop.JointType.WristRight,
            (int)KinectInterop.JointType.HandRight,

            (int)KinectInterop.JointType.HipLeft,
            (int)KinectInterop.JointType.KneeLeft,
            (int)KinectInterop.JointType.AnkleLeft,
            (int)KinectInterop.JointType.FootLeft,

            (int)KinectInterop.JointType.HipRight,
            (int)KinectInterop.JointType.KneeRight,
            (int)KinectInterop.JointType.AnkleRight,
            (int)KinectInterop.JointType.FootRight,

            (int)KinectInterop.JointType.ClavicleLeft,

            (int)KinectInterop.JointType.HandtipLeft,
            (int)KinectInterop.JointType.ThumbLeft,
            (int)KinectInterop.JointType.HandtipRight,
            (int)KinectInterop.JointType.ThumbRight
        };

        public static readonly Vector3[] KinectJointBaseDir =
        {
            Vector3.zero,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,

            Vector3.left,
            Vector3.left,
            Vector3.left,
            Vector3.left,
            Vector3.left,

            Vector3.right,
            Vector3.right,
            Vector3.right,
            Vector3.right,
            Vector3.right,

            Vector3.down,
            Vector3.down,
            Vector3.down,
            Vector3.forward,

            Vector3.down,
            Vector3.down,
            Vector3.down,
            Vector3.forward,

            Vector3.forward,
            Vector3.left,
            Vector3.left,
            Vector3.right,
            Vector3.right,

            Vector3.left,
            Vector3.forward,
            Vector3.right,
            Vector3.forward,
        };


        public override bool UpdateSensorData(KinectInterop.SensorData sensorData, KinectManager kinectManager, bool isPlayMode)
        {
            base.UpdateSensorData(sensorData, kinectManager, isPlayMode);

            if (sensorData.depthCamIntr == null && coordMapper != null)
            {
                lock (depthFrameLock)
                {
                    // get depth camera intrinsics
                    CameraIntrinsics depthCamIntr = coordMapper.GetDepthCameraIntrinsics();

                    if (depthCamIntr.PrincipalPointX != 0f && depthCamIntr.PrincipalPointY != 0f)
                    {
                        GetDepthCameraIntrinsics(depthCamIntr, ref sensorData.depthCamIntr, sensorData.depthImageWidth, sensorData.depthImageHeight);
                    }
                }
            }

            if (sensorData.colorCamIntr == null && coordMapper != null)
            {
                lock (colorFrameLock)
                {
                    GetColorCameraIntrinsics(ref sensorData.colorCamIntr, sensorData.colorImageWidth, sensorData.colorImageHeight);
                }
            }

            return true;
        }


        // gets the depth camera intrinsics
        private void GetDepthCameraIntrinsics(CameraIntrinsics camIntr, ref KinectInterop.CameraIntrinsics intr, int camWidth, int camHeight)
        {
            intr = new KinectInterop.CameraIntrinsics();

            intr.width = camWidth;
            intr.height = camHeight;

            intr.ppx = camIntr.PrincipalPointX;
            intr.ppy = camIntr.PrincipalPointY;

            intr.fx = camIntr.FocalLengthX;
            intr.fy = camIntr.FocalLengthY;

            intr.distCoeffs = new float[3];
            intr.distCoeffs[0] = camIntr.RadialDistortionSecondOrder;
            intr.distCoeffs[1] = camIntr.RadialDistortionFourthOrder;
            intr.distCoeffs[2] = camIntr.RadialDistortionSixthOrder;

            intr.distType = KinectInterop.DistortionType.BrownConrady;

            EstimateFOV(intr);
        }


        // gets the color camera intrinsics
        private void GetColorCameraIntrinsics(ref KinectInterop.CameraIntrinsics intr, int camWidth, int camHeight)
        {
            intr = new KinectInterop.CameraIntrinsics();

            intr.width = camWidth;
            intr.height = camHeight;

            intr.ppx = 946.0374f;
            intr.ppy = 537.392f;

            intr.fx = 1065.267f;
            intr.fy = 1065.409f;

            intr.distCoeffs = new float[3];
            intr.distCoeffs[0] = 0.014655f;
            intr.distCoeffs[1] = -0.000476f;
            intr.distCoeffs[2] = 0f;

            intr.distType = KinectInterop.DistortionType.BrownConrady;

            EstimateFOV(intr);
        }


        public override void PollCoordTransformFrames(KinectInterop.SensorData sensorData)
        {
            if (lastDepthCoordFrameTime != rawDepthTimestamp)
            {
                lastDepthCoordFrameTime = rawDepthTimestamp;

                //// depth2space frame
                //if (depth2SpaceCoordFrame != null)
                //{
                //    lock (depth2SpaceFrameLock)
                //    {
                //        MapDepthFrameToSpaceCoords(sensorData, ref depth2SpaceCoordFrame);
                //        lastDepth2SpaceFrameTime = lastDepthCoordFrameTime;
                //    }
                //}

                // depth2color frame
                if (depth2ColorCoordFrame != null && rawDepthImage != null)
                {
                    lock (depth2ColorFrameLock)
                    {
                        var pDepthData = GCHandle.Alloc(rawDepthImage, GCHandleType.Pinned);
                        var pColorCoordsData = GCHandle.Alloc(depth2ColorCoordFrame, GCHandleType.Pinned);

                        coordMapper.MapDepthFrameToColorSpaceUsingIntPtr(
                            pDepthData.AddrOfPinnedObject(),
                            rawDepthImage.Length * sizeof(ushort),
                            pColorCoordsData.AddrOfPinnedObject(),
                            (uint)depth2ColorCoordFrame.Length);

                        pColorCoordsData.Free();
                        pDepthData.Free();

                        //int di = (sensorData.depthImageHeight / 2) * sensorData.depthImageWidth + (sensorData.depthImageWidth / 2);
                        //Debug.Log("d2cCoordData: " + depth2ColorCoordFrame[di]);

                        lastDepth2ColorFrameTime = lastDepthCoordFrameTime;
                        //Debug.Log("Depth2ColorFrameTime: " + lastDepth2ColorFrameTime);
                    }
                }

                // color2depth frame
                if (color2DepthCoordFrame != null)
                {
                    lock (color2DepthFrameLock)
                    {
                        var pDepthData = GCHandle.Alloc(rawDepthImage, GCHandleType.Pinned);
                        var pDepthCoordsData = GCHandle.Alloc(color2DepthCoordFrame, GCHandleType.Pinned);

                        coordMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                            pDepthData.AddrOfPinnedObject(),
                            (uint)rawDepthImage.Length * sizeof(ushort),
                            pDepthCoordsData.AddrOfPinnedObject(),
                            (uint)color2DepthCoordFrame.Length);

                        //int ci = (sensorData.colorImageHeight / 2) * sensorData.colorImageWidth + (sensorData.colorImageWidth / 2);
                        //Debug.Log("c2dCoordData: " + color2DepthCoordFrame[ci]);

                        pDepthCoordsData.Free();
                        pDepthData.Free();

                        lastColor2DepthFrameTime = lastDepthCoordFrameTime;
                        //Debug.Log("Color2DepthFrameTime: " + lastColor2DepthFrameTime);
                    }
                }
            }
        }


        //public override bool UpdateSensorData(KinectInterop.SensorData sensorData)
        //{
        //    base.UpdateSensorData(sensorData);
        //    return true;
        //}


        // creates the point-cloud vertex shader and its respective buffers, as needed
        protected override bool CreatePointCloudVertexShader(KinectInterop.SensorData sensorData)
        {
            if (pointCloudResolution != PointCloudResolution.ColorCameraResolution)
            {
                return base.CreatePointCloudVertexShader(sensorData);
            }

            // for K2 color camera resolution only
            pointCloudVertexRes = GetPointCloudTexResolution(sensorData);

            if (pointCloudVertexRT == null)
            {
                pointCloudVertexRT = new RenderTexture(pointCloudVertexRes.x, pointCloudVertexRes.y, 0, RenderTextureFormat.ARGBHalf);
                pointCloudVertexRT.enableRandomWrite = true;
                pointCloudVertexRT.Create();
            }

            if (pointCloudVertexShader == null)
            {
                pointCloudVertexShader = Resources.Load("PointCloudVertexShaderCRK2") as ComputeShader;
                pointCloudVertexKernel = pointCloudVertexShader != null ? pointCloudVertexShader.FindKernel("BakeVertexTexColorResK2") : -1;
            }

            if (pointCloudSpaceBuffer == null)
            {
                int spaceBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight * 3;
                pointCloudSpaceBuffer = new ComputeBuffer(spaceBufferLength, sizeof(float));

                // depth2space table
                //int depthImageLength = sensorData.depthImageWidth * sensorData.depthImageHeight;
                //Vector3[] depth2SpaceTable = new Vector3[depthImageLength];

                //for (int dy = 0, di = 0; dy < sensorData.depthImageHeight; dy++)
                //{
                //    for (int dx = 0; dx < sensorData.depthImageWidth; dx++, di++)
                //    {
                //        Vector2 depthPos = new Vector2(dx, dy);
                //        depth2SpaceTable[di] = MapDepthPointToSpaceCoords(sensorData, depthPos, 1000);
                //    }
                //}

                Vector3[] depth2SpaceTable = GetDepthCameraSpaceTable(sensorData);
                pointCloudSpaceBuffer.SetData(depth2SpaceTable);
                depth2SpaceTable = null;
            }

            if (pointCloudDepthBuffer == null)
            {
                int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                pointCloudDepthBuffer = new ComputeBuffer(depthBufferLength, sizeof(uint));
            }

            if (pointCloudCoordBuffer == null)
            {
                int coordBufferLength = sensorData.colorImageWidth * sensorData.colorImageHeight * 2;  // Vector2 = 2 x float
                pointCloudCoordBuffer = new ComputeBuffer(coordBufferLength, sizeof(float));
            }

            if (color2DepthCoordFrame == null)
            {
                color2DepthCoordFrame = new Vector2[sensorData.colorImageWidth * sensorData.colorImageHeight];
            }

            return true;
        }


        // updates the point-cloud vertex shader with the actual data
        protected override bool UpdatePointCloudVertexShader(KinectInterop.SensorData sensorData)
        {
            if (pointCloudResolution != PointCloudResolution.ColorCameraResolution)
            {
                return base.UpdatePointCloudVertexShader(sensorData);
            }

            // for K2 color camera resolution only
            if (pointCloudVertexShader != null && sensorData.depthImage != null && pointCloudVertexRT != null &&
                sensorData.lastDepth2SpaceFrameTime != sensorData.lastDepthFrameTime)
            {
                sensorData.lastDepth2SpaceFrameTime = sensorData.lastDepthFrameTime;

                KinectInterop.SetComputeBufferData(pointCloudDepthBuffer, sensorData.depthImage, sensorData.depthImage.Length >> 1, sizeof(uint));

                lock (color2DepthFrameLock)
                {
                    int coordBufferLength = sensorData.colorImageWidth * sensorData.colorImageHeight * 2;
                    KinectInterop.SetComputeBufferData(pointCloudCoordBuffer, color2DepthCoordFrame, coordBufferLength, sizeof(float));
                }

                KinectInterop.SetComputeShaderInt2(pointCloudVertexShader, "PointCloudRes", pointCloudVertexRes.x, pointCloudVertexRes.y);
                KinectInterop.SetComputeShaderInt2(pointCloudVertexShader, "DepthRes", sensorData.depthImageWidth, sensorData.depthImageHeight);
                KinectInterop.SetComputeShaderFloat2(pointCloudVertexShader, "SpaceScale", sensorData.sensorSpaceScale.x, sensorData.sensorSpaceScale.y);
                pointCloudVertexShader.SetInt("MinDepth", (int)(minDistance * 1000f));
                pointCloudVertexShader.SetInt("MaxDepth", (int)(maxDistance * 1000f));
                pointCloudVertexShader.SetBuffer(pointCloudVertexKernel, "SpaceTable", pointCloudSpaceBuffer);
                pointCloudVertexShader.SetBuffer(pointCloudVertexKernel, "DepthMap", pointCloudDepthBuffer);
                pointCloudVertexShader.SetBuffer(pointCloudVertexKernel, "ColorToDepthMap", pointCloudCoordBuffer);
                pointCloudVertexShader.SetTexture(pointCloudVertexKernel, "PointCloudVertexTex", pointCloudVertexRT);
                pointCloudVertexShader.Dispatch(pointCloudVertexKernel, pointCloudVertexRes.x / 8, pointCloudVertexRes.y / 8, 1);

                if (pointCloudVertexTexture != null)
                {
                    Graphics.Blit(pointCloudVertexRT, pointCloudVertexTexture);
                }

                return true;
            }

            return false;
        }


        // creates the point-cloud color shader and its respective buffers, as needed
        protected override bool CreatePointCloudColorShader(KinectInterop.SensorData sensorData)
        {
            if (pointCloudResolution != PointCloudResolution.DepthCameraResolution)
            {
                return base.CreatePointCloudColorShader(sensorData);
            }

            // for K2 depth camera resolution only
            if (pointCloudColorRT == null)
            {
                pointCloudColorRT = new RenderTexture(sensorData.depthImageWidth, sensorData.depthImageHeight, 0, RenderTextureFormat.ARGB32);
                pointCloudColorRT.enableRandomWrite = true;
                pointCloudColorRT.Create();
            }

            if (pointCloudColorShader == null)
            {
                pointCloudColorShader = Resources.Load("PointCloudColorShaderK2") as ComputeShader;
                pointCloudColorKernel = pointCloudColorShader != null ? pointCloudColorShader.FindKernel("BakeColorTex") : -1;
            }

            if (pointCloudCoordBuffer == null)
            {
                int coordBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight * 2;  // Vector2 = 2 x float
                pointCloudCoordBuffer = new ComputeBuffer(coordBufferLength, sizeof(float));
            }

            if (depth2ColorCoordFrame == null)
            {
                depth2ColorCoordFrame = new Vector2[sensorData.depthImageWidth * sensorData.depthImageHeight];
            }

            return true;
        }


        // updates the point-cloud color shader with the actual data
        protected override bool UpdatePointCloudColorShader(KinectInterop.SensorData sensorData)
        {
            if (pointCloudResolution != PointCloudResolution.DepthCameraResolution)
            {
                return base.UpdatePointCloudColorShader(sensorData);
            }

            // for K2 depth camera resolution only
            if (pointCloudColorShader != null && pointCloudCoordBuffer != null && sensorData.colorImageTexture != null && pointCloudColorRT != null &&
                sensorData.lastDepth2ColorFrameTime != lastDepth2ColorFrameTime)
            {
                sensorData.lastDepth2ColorFrameTime = lastDepth2ColorFrameTime;

                lock (depth2ColorFrameLock)
                {
                    int coordBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight * 2;
                    KinectInterop.SetComputeBufferData(pointCloudCoordBuffer, depth2ColorCoordFrame, coordBufferLength, sizeof(float));
                }

                KinectInterop.SetComputeShaderInt2(pointCloudColorShader, "DepthRes", sensorData.depthImageWidth, sensorData.depthImageHeight);
                pointCloudColorShader.SetBuffer(pointCloudColorKernel, "DepthToColorMap", pointCloudCoordBuffer);
                pointCloudColorShader.SetTexture(pointCloudColorKernel, "ColorTex", sensorData.colorImageTexture);
                pointCloudColorShader.SetTexture(pointCloudColorKernel, "PointCloudColorTex", pointCloudColorRT);
                pointCloudColorShader.Dispatch(pointCloudColorKernel, sensorData.depthImageWidth / 8, sensorData.depthImageHeight / 8, 1);

                if(pointCloudColorTexture != null)
                {
                    Graphics.Blit(pointCloudColorRT, pointCloudColorTexture);
                }

                return true;
            }

            return false;
        }


        // creates the color-depth shader and its respective buffers, as needed
        protected override bool CreateColorDepthShader(KinectInterop.SensorData sensorData)
        {
            if (colorDepthShader == null)
            {
                colorDepthShader = Resources.Load("ColorDepthShaderK2") as ComputeShader;
                colorDepthKernel = colorDepthShader != null ? colorDepthShader.FindKernel("BakeColorDepth") : -1;
            }

            if (pointCloudDepthBuffer == null)
            {
                int bufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                pointCloudDepthBuffer = new ComputeBuffer(bufferLength, sizeof(uint));
            }

            if (pointCloudCoordBuffer == null)
            {
                int bufferLength = sensorData.colorImageWidth * sensorData.colorImageHeight * 2;  // Vector2 = 2 x float
                pointCloudCoordBuffer = new ComputeBuffer(bufferLength, sizeof(float));
            }

            if (color2DepthCoordFrame == null)
            {
                color2DepthCoordFrame = new Vector2[sensorData.colorImageWidth * sensorData.colorImageHeight];
            }

            if (sensorData.colorDepthTexture == null)
            {
                sensorData.colorDepthTexture = new RenderTexture(sensorData.colorImageWidth, sensorData.colorImageHeight, 0, RenderTextureFormat.ARGB32);
                //sensorData.colorDepthTexture.enableRandomWrite = true;
                sensorData.colorDepthTexture.Create();
            }

            colorDepthShaderInited = true;

            return true;
        }


        // updates the color shader with the actual data
        protected override bool UpdateColorDepthShader(KinectInterop.SensorData sensorData)
        {
            // for K2 depth camera resolution only
            if (colorDepthShader != null && pointCloudDepthBuffer != null && pointCloudCoordBuffer != null && color2DepthCoordFrame != null)
            {
                if (sensorData.usedColorDepthBufferTime == sensorData.lastColorDepthBufferTime && sensorData.lastColorDepthBufferTime != lastColor2DepthFrameTime)
                {
                    if (sensorData.colorImageTexture != null)
                    {
                        Graphics.Blit(sensorData.colorImageTexture, sensorData.colorDepthTexture);
                    }

                    KinectInterop.SetComputeBufferData(pointCloudDepthBuffer, sensorData.depthImage, sensorData.depthImage.Length >> 1, sizeof(uint));

                    lock (color2DepthFrameLock)
                    {
                        int bufferLength = sensorData.colorImageWidth * sensorData.colorImageHeight * 2;
                        KinectInterop.SetComputeBufferData(pointCloudCoordBuffer, color2DepthCoordFrame, bufferLength, sizeof(float));
                    }

                    KinectInterop.SetComputeShaderInt2(colorDepthShader, "_ColorRes", sensorData.colorImageWidth, sensorData.colorImageHeight);
                    KinectInterop.SetComputeShaderInt2(colorDepthShader, "_DepthRes", sensorData.depthImageWidth, sensorData.depthImageHeight);

                    colorDepthShader.SetBuffer(colorDepthKernel, "_DepthMap", pointCloudDepthBuffer);
                    colorDepthShader.SetBuffer(colorDepthKernel, "_Color2DepthMap", pointCloudCoordBuffer);
                    colorDepthShader.SetTexture(colorDepthKernel, "_ColorTex", sensorData.colorImageTexture);
                    colorDepthShader.SetBuffer(colorDepthKernel, "_ColorDepthMap", sensorData.colorDepthBuffer);
                    colorDepthShader.Dispatch(colorDepthKernel, sensorData.colorImageWidth / 8, sensorData.colorImageHeight / 8, 1);
                    sensorData.lastColorDepthBufferTime = lastColor2DepthFrameTime;
                }

                return true;
            }

            return false;
        }


        public override Vector3 MapDepthPointToSpaceCoords(KinectInterop.SensorData sensorData, Vector2 depthPos, ushort depthVal)
        {
            Vector3 vPoint = Vector3.zero;

            if (coordMapper != null && depthPos != Vector2.zero)
            {
                DepthSpacePoint depthPoint = new DepthSpacePoint();
                depthPoint.X = depthPos.x;
                depthPoint.Y = depthPos.y;

                DepthSpacePoint[] depthPoints = new DepthSpacePoint[1];
                depthPoints[0] = depthPoint;

                ushort[] depthVals = new ushort[1];
                depthVals[0] = depthVal;

                CameraSpacePoint[] camPoints = new CameraSpacePoint[1];
                coordMapper.MapDepthPointsToCameraSpace(depthPoints, depthVals, camPoints);

                CameraSpacePoint camPoint = camPoints[0];
                vPoint.x = camPoint.X;
                vPoint.y = camPoint.Y;
                vPoint.z = camPoint.Z;
            }

            return vPoint;
        }

        public override Vector2 MapSpacePointToDepthCoords(KinectInterop.SensorData sensorData, Vector3 spacePos)
        {
            Vector2 vPoint = Vector2.zero;

            if (coordMapper != null)
            {
                CameraSpacePoint camPoint = new CameraSpacePoint();
                camPoint.X = spacePos.x;
                camPoint.Y = spacePos.y;
                camPoint.Z = spacePos.z;

                CameraSpacePoint[] camPoints = new CameraSpacePoint[1];
                camPoints[0] = camPoint;

                DepthSpacePoint[] depthPoints = new DepthSpacePoint[1];
                coordMapper.MapCameraPointsToDepthSpace(camPoints, depthPoints);

                DepthSpacePoint depthPoint = depthPoints[0];

                if (depthPoint.X >= 0 && depthPoint.X < sensorData.depthImageWidth &&
                   depthPoint.Y >= 0 && depthPoint.Y < sensorData.depthImageHeight)
                {
                    vPoint.x = depthPoint.X;
                    vPoint.y = depthPoint.Y;
                }
            }

            return vPoint;
        }

        public override Vector2 MapDepthPointToColorCoords(KinectInterop.SensorData sensorData, Vector2 depthPos, ushort depthVal)
        {
            Vector2 vPoint = Vector2.zero;

            if (coordMapper != null && depthPos != Vector2.zero)
            {
                DepthSpacePoint depthPoint = new DepthSpacePoint();
                depthPoint.X = depthPos.x;
                depthPoint.Y = depthPos.y;

                DepthSpacePoint[] depthPoints = new DepthSpacePoint[1];
                depthPoints[0] = depthPoint;

                ushort[] depthVals = new ushort[1];
                depthVals[0] = depthVal;

                ColorSpacePoint[] colPoints = new ColorSpacePoint[1];
                coordMapper.MapDepthPointsToColorSpace(depthPoints, depthVals, colPoints);

                ColorSpacePoint colPoint = colPoints[0];
                vPoint.x = colPoint.X;
                vPoint.y = colPoint.Y;
            }

            return vPoint;
        }

        //public override bool MapDepthFrameToSpaceCoords(KinectInterop.SensorData sensorData, ref Vector3[] vSpaceCoords)
        //{
        //    if (coordMapper != null && sensorData.depthImage != null)
        //    {
        //        var pDepthData = GCHandle.Alloc(sensorData.depthImage, GCHandleType.Pinned);
        //        var pSpaceCoordsData = GCHandle.Alloc(vSpaceCoords, GCHandleType.Pinned);

        //        coordMapper.MapDepthFrameToCameraSpaceUsingIntPtr(
        //            pDepthData.AddrOfPinnedObject(),
        //            sensorData.depthImage.Length * sizeof(ushort),
        //            pSpaceCoordsData.AddrOfPinnedObject(),
        //            (uint)vSpaceCoords.Length);

        //        pSpaceCoordsData.Free();
        //        pDepthData.Free();

        //        return true;
        //    }

        //    return false;
        //}

        //public override bool MapDepthFrameToColorCoords(KinectInterop.SensorData sensorData, ref Vector2[] vColorCoords)
        //{
        //    if (coordMapper != null && sensorData.colorImageTexture != null && sensorData.depthImage != null)
        //    {
        //        var pDepthData = GCHandle.Alloc(sensorData.depthImage, GCHandleType.Pinned);
        //        var pColorCoordsData = GCHandle.Alloc(vColorCoords, GCHandleType.Pinned);

        //        coordMapper.MapDepthFrameToColorSpaceUsingIntPtr(
        //            pDepthData.AddrOfPinnedObject(),
        //            sensorData.depthImage.Length * sizeof(ushort),
        //            pColorCoordsData.AddrOfPinnedObject(),
        //            (uint)vColorCoords.Length);

        //        pColorCoordsData.Free();
        //        pDepthData.Free();

        //        return true;
        //    }

        //    return false;
        //}

        //public override bool MapColorFrameToDepthCoords(KinectInterop.SensorData sensorData, ref Vector2[] vDepthCoords)
        //{
        //    if (coordMapper != null && sensorData.colorImageTexture != null && sensorData.depthImage != null)
        //    {
        //        var pDepthData = GCHandle.Alloc(sensorData.depthImage, GCHandleType.Pinned);
        //        var pDepthCoordsData = GCHandle.Alloc(vDepthCoords, GCHandleType.Pinned);

        //        coordMapper.MapColorFrameToDepthSpaceUsingIntPtr(
        //            pDepthData.AddrOfPinnedObject(),
        //            (uint)sensorData.depthImage.Length * sizeof(ushort),
        //            pDepthCoordsData.AddrOfPinnedObject(),
        //            (uint)vDepthCoords.Length);

        //        pDepthCoordsData.Free();
        //        pDepthData.Free();

        //        return true;
        //    }

        //    return false;
        //}

    }
}

#endif