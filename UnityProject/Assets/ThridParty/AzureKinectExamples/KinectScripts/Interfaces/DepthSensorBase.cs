using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.rfilkov.kinect
{
    public abstract class DepthSensorBase : MonoBehaviour, DepthSensorInterface
    {
        // max depth distance in mm, used for initializing data arrays and compute buffers
        public const int MAX_DEPTH_DISTANCE_MM = 10000;

        [Tooltip("Device streaming mode, in means of connected sensor, recording or disabled.")]
        public KinectInterop.DeviceStreamingMode deviceStreamingMode = KinectInterop.DeviceStreamingMode.ConnectedSensor;

        [Tooltip("Index of the depth sensor in the list of currently connected sensors.")]
        public int deviceIndex = 0;

        [Tooltip("Path to the recording file, if the streaming mode is PlayRecording.")]
        public string recordingFile = string.Empty;

        //[Tooltip("Sensor position in space.")]
        //public Vector3 devicePosition = new Vector3(0f, 1f, 0f);

        //[Tooltip("Sensor rotation in space.")]
        //public Vector3 deviceRotation = new Vector3(0f, 0f, 0f);

        //[Tooltip("Whether the body tracking for this sensor is enabled or not.")]
        //internal bool bodyTrackingEnabled = false;

        [Tooltip("Minimum distance in meters, used for creating the depth-related images.")]
        [Range(0f, 10f)]
        public float minDistance = 0.5f;

        [Tooltip("Maximum distance in meters, used for creating the depth-related images.")]
        [Range(0f, 10f)]
        public float maxDistance = 10f;

        [Tooltip("Resolution of the generated point-cloud textures.")]
        public PointCloudResolution pointCloudResolution = PointCloudResolution.DepthCameraResolution;
        public enum PointCloudResolution : int { DepthCameraResolution = 0, ColorCameraResolution = 1 }

        [Tooltip("Render texture, used for point-cloud vertex mapping. The texture resolution should match the depth or color image resolution.")]
        public RenderTexture pointCloudVertexTexture = null;

        [Tooltip("Render texture, used for point-cloud color mapping. The texture resolution should match the depth or color image resolution.")]
        public RenderTexture pointCloudColorTexture = null;


        // initial parameters
        protected KinectInterop.FrameSource frameSourceFlags;
        protected bool isSyncDepthAndColor = false;
        protected bool isSyncBodyAndDepth = false;

        // initial pose parameters
        protected Vector3 initialPosePosition = Vector3.zero;
        protected Quaternion initialPoseRotation = Quaternion.identity;
        protected Matrix4x4 matTransformPose = Matrix4x4.identity;
        protected Matrix4x4 matLocalPose = Matrix4x4.identity;

        // frame numbers
        //protected ulong colorFrameNumber = 0;
        //protected ulong depthFrameNumber = 0;
        //protected ulong infraredFrameNumber = 0;
        //protected ulong poseFrameNumber = 0;

        // raw color data
        protected byte[] rawColorImage = null;
        protected ulong rawColorTimestamp = 0;
        protected ulong currentColorTimestamp = 0;
        protected object colorFrameLock = new object();

        // raw depth data
        protected ushort[] rawDepthImage = null;
        protected ulong rawDepthTimestamp = 0;
        protected ulong currentDepthTimestamp = 0;
        protected object depthFrameLock = new object();

        // raw infrared data
        protected ushort[] rawInfraredImage = null;
        protected ulong rawInfraredTimestamp = 0;
        protected ulong currentInfraredTimestamp = 0;
        protected object infraredFrameLock = new object();

        // raw pose data
        protected Vector3 rawPosePosition = Vector3.zero;
        protected Quaternion rawPoseRotation = Quaternion.identity;
        protected ulong rawPoseTimestamp = 0;
        protected ulong currentPoseTimestamp = 0;
        protected object poseFrameLock = new object();

        // sensor pose data
        protected Vector3 sensorPosePosition;
        protected Quaternion sensorPoseRotation;

        protected Vector3 sensorRotOffset = Vector3.zero;
        protected bool sensorRotFlipZ = false;
        protected bool sensorRotIgnoreY = false;
        [HideInInspector]
        public float sensorRotValueY = 0f;

        // body tracker
        protected BodyTracking bodyTracker = null;
        protected k4abt_skeleton_t bodySkeletonData;
        protected bool bIgnoreZCoordinates = false;
        protected bool bIgnoreInferredJoints = false;
        protected int btQueueCount = 0;
        protected int btQueueWaitTime = 0;
        protected ulong btQueueTime = 0;

        protected System.Threading.Thread bodyTrackerThread = null;
        protected System.Threading.AutoResetEvent bodyTrackerStopEvent = null;
        //private Capture bodyInputCapture = null;
        private Capture bodyOutputCapture = null;
        private object bodyCaptureLock = new object();

        // raw body data
        protected byte[] rawBodyIndexImage = null;
        protected uint trackedBodiesCount = 0;
        protected List<KinectInterop.BodyData> alTrackedBodies = null;
        protected ulong rawBodyTimestamp = 0;
        protected ulong currentBodyTimestamp = 0;
        protected object bodyTrackerLock = new object();


        // depth image data
        protected int[] depthHistBufferData = null;
        protected int[] equalHistBufferData = null;
        protected int histDataTotalPoints = 0;
        protected ulong lastDepthImageTimestamp = 0;
        protected object depthImageDataLock = new object();

        // infrared image data
        protected float minInfraredValue = 0f;
        protected float maxInfraredValue = 0f;

        // body image data
        protected int[] depthBodyBufferData = null;
        protected int[] equalBodyBufferData = null;
        protected int histBodyTotalPoints = 0;
        protected ulong lastBodyImageTimestamp = 0;
        protected object bodyImageDataLock = new object();

        // last updated depth coord-frame time
        protected ulong lastDepthCoordFrameTime = 0;

        // point cloud vertex shader
        protected ComputeShader pointCloudVertexShader = null;
        protected int pointCloudVertexKernel = -1;
        protected Vector2Int pointCloudVertexRes = Vector2Int.zero;
        protected RenderTexture pointCloudVertexRT = null;
        protected ComputeBuffer pointCloudSpaceBuffer = null;
        protected ComputeBuffer pointCloudDepthBuffer = null;

        // point cloud color shader
        protected ComputeShader pointCloudColorShader = null;
        protected int pointCloudColorKernel = -1;
        protected Vector2Int pointCloudColorRes = Vector2Int.zero;
        protected RenderTexture pointCloudColorRT = null;
        protected ComputeBuffer pointCloudCoordBuffer = null;
        protected Texture2D pointCloudAlignedColorTex = null;

        //// depth2space coords frame
        //protected Vector3[] depth2SpaceCoordFrame = null;
        //protected ulong lastDepth2SpaceFrameTime = 0;
        //protected object depth2SpaceFrameLock = new object();

        // space tables
        protected Vector3[] depth2SpaceTable = null;
        protected Vector3[] color2SpaceTable = null;
        //protected ushort[] lastDepthDataBuf = null;

        // depth2color coords frame
        protected byte[] depth2ColorDataFrame = null;
        protected Vector2[] depth2ColorCoordFrame = null;
        protected ulong lastDepth2ColorFrameTime = 0;
        protected object depth2ColorFrameLock = new object();

        // color2depth coords frame
        protected ushort[] color2DepthDataFrame = null;
        protected Vector2[] color2DepthCoordFrame = null;
        protected ulong lastColor2DepthFrameTime = 0;
        protected object color2DepthFrameLock = new object();

        // color2depth shader
        protected ComputeShader colorDepthShader = null;
        protected int colorDepthKernel = -1;
        protected bool colorDepthShaderInited = false;
        

        protected virtual void Awake()
        {
            // init raw sensor pose
            rawPosePosition = Vector3.zero;
            rawPoseRotation = Quaternion.identity;
            rawPoseTimestamp = (ulong)DateTime.Now.Ticks;

            sensorPosePosition = transform.position;
            sensorPoseRotation = transform.rotation;

            // initial pose params
            initialPosePosition = transform.position;
            initialPoseRotation = transform.rotation;

            matTransformPose.SetTRS(initialPosePosition, initialPoseRotation, Vector3.one);
        }



        public abstract KinectInterop.DepthSensorPlatform GetSensorPlatform();

        //public virtual bool InitSensorInterface(bool bCopyLibs, ref bool bNeedRestart)
        //{
        //    bNeedRestart = false;
        //    return true;
        //}

        //public virtual void FreeSensorInterface(bool bDeleteLibs)
        //{
        //}

        public abstract List<KinectInterop.SensorDeviceInfo> GetAvailableSensors();

        public virtual KinectInterop.SensorData OpenSensor(KinectInterop.FrameSource dwFlags, bool bSyncDepthAndColor, bool bSyncBodyAndDepth)
        {
            // save the parameters for later
            frameSourceFlags = dwFlags;
            isSyncDepthAndColor = bSyncDepthAndColor && ((dwFlags & KinectInterop.FrameSource.TypeColor) != 0) && ((dwFlags & KinectInterop.FrameSource.TypeDepth) != 0);
            isSyncBodyAndDepth = bSyncBodyAndDepth && ((dwFlags & KinectInterop.FrameSource.TypeBody) != 0) && ((dwFlags & KinectInterop.FrameSource.TypeDepth) != 0);

            return null;
        }


        public virtual void CloseSensor(KinectInterop.SensorData sensorData)
        {
            // stop body tracking, if needed
            StopBodyTracking(sensorData);

            // dispose coord mapping shaders
            DisposePointCloudVertexShader(sensorData);
            DisposePointCloudColorShader(sensorData);
            DisposeColorDepthShader(sensorData);
            DisposeDepthTexShader(sensorData);
            DisposeInfraredTexShader(sensorData);
        }


        public virtual void InitSensorData(KinectInterop.SensorData sensorData, KinectManager kinectManager)
        {
            //if (sensorData.depthImage != null)
            //{
            //    depthImageBufferData = new int[sensorData.depthImage.Length];
            //}

            // depth image data
            if (kinectManager.getDepthFrames == KinectManager.DepthTextureType.DepthTexture)
            {
                depthHistBufferData = new int[MAX_DEPTH_DISTANCE_MM + 1];
                equalHistBufferData = new int[MAX_DEPTH_DISTANCE_MM + 1];
                sensorData.depthHistBufferData = new int[equalHistBufferData.Length];
            }
            else
            {
                depthHistBufferData = null;
                equalHistBufferData = null;
                sensorData.depthHistBufferData = null;
            }

            // body image data
            if (kinectManager.getBodyFrames == KinectManager.BodyTextureType.UserTexture)
            {
                depthBodyBufferData = new int[MAX_DEPTH_DISTANCE_MM + 1];
                equalBodyBufferData = new int[MAX_DEPTH_DISTANCE_MM + 1];
                sensorData.bodyHistBufferData = new int[equalBodyBufferData.Length];
            }
            else
            {
                depthBodyBufferData = null;
                equalBodyBufferData = null;
                sensorData.bodyHistBufferData = null;
            }

            lock (bodyTrackerLock)
            {
                // save the needed KM settings
                bIgnoreZCoordinates = kinectManager.ignoreZCoordinates;
                bIgnoreInferredJoints = kinectManager.ignoreInferredJoints;
            }
        }

        public virtual void PollSensorFrames(KinectInterop.SensorData sensorData)
        {
        }


        public virtual void PollCoordTransformFrames(KinectInterop.SensorData sensorData)
        {
        }


        public virtual void ProcessSensorDataInThread(KinectInterop.SensorData sensorData)
        {
            // depth-image data
            if (lastDepthImageTimestamp != rawDepthTimestamp && rawDepthImage != null && depthHistBufferData != null)
            {
                lock (depthImageDataLock)
                {
                    Array.Clear(depthHistBufferData, 0, depthHistBufferData.Length);
                    Array.Clear(equalHistBufferData, 0, equalHistBufferData.Length);
                    histDataTotalPoints = 0;

                    int depthMinDistance = (int)(minDistance * 1000f);
                    int depthMaxDistance = (int)(maxDistance * 1000f);

                    for (int i = 0; i < rawDepthImage.Length; i++)
                    {
                        int depth = rawDepthImage[i];
                        int limDepth = (depth <= MAX_DEPTH_DISTANCE_MM) ? depth : 0;

                        if (limDepth > 0)
                        {
                            depthHistBufferData[limDepth]++;
                            histDataTotalPoints++;
                        }
                    }

                    equalHistBufferData[0] = depthHistBufferData[0];
                    for (int i = 1; i < depthHistBufferData.Length; i++)
                    {
                        equalHistBufferData[i] = equalHistBufferData[i - 1] + depthHistBufferData[i];
                    }

                    // make depth 0 equal to the max-depth
                    equalHistBufferData[0] = equalHistBufferData[equalHistBufferData.Length - 1];

                    lastDepthImageTimestamp = rawDepthTimestamp;
                    //Debug.Log("lastDepthImageTimestamp: " + lastDepthImageTimestamp);
                }
            }

            // body-image data
            if (lastBodyImageTimestamp != rawBodyTimestamp && rawDepthImage != null && rawBodyIndexImage != null && depthBodyBufferData != null)
            {
                lock (bodyImageDataLock)
                {
                    Array.Clear(depthBodyBufferData, 0, depthBodyBufferData.Length);
                    Array.Clear(equalBodyBufferData, 0, equalBodyBufferData.Length);
                    histBodyTotalPoints = 0;

                    int depthMinDistance = (int)(minDistance * 1000f);
                    int depthMaxDistance = (int)(maxDistance * 1000f);

                    for (int i = 0; i < rawDepthImage.Length; i++)
                    {
                        int depth = rawDepthImage[i];
                        int limDepth = (depth <= MAX_DEPTH_DISTANCE_MM) ? depth : 0;

                        if (rawBodyIndexImage[i] != 255 && limDepth > 0)
                        {
                            depthBodyBufferData[limDepth]++;
                            histBodyTotalPoints++;
                        }
                    }

                    if(histBodyTotalPoints > 0)
                    {
                        for (int i = 1; i < depthBodyBufferData.Length; i++)
                        {
                            equalBodyBufferData[i] = equalBodyBufferData[i - 1] + depthBodyBufferData[i];
                        }
                    }

                    lastBodyImageTimestamp = rawBodyTimestamp;
                    //Debug.Log("lastBodyImageTimestamp: " + lastBodyImageTimestamp);
                }
            }

            // ...


            // set the frame timestamps
            if (currentColorTimestamp != rawColorTimestamp)
            {
                // new color frame
                currentColorTimestamp = rawColorTimestamp;
            }

            if (currentDepthTimestamp != rawDepthTimestamp)
            {
                // new depth frame
                currentDepthTimestamp = rawDepthTimestamp;
            }

            if (currentInfraredTimestamp != rawInfraredTimestamp)
            {
                // new depth frame
                currentInfraredTimestamp = rawInfraredTimestamp;
            }

            if (currentPoseTimestamp != rawPoseTimestamp)
            {
                // new pose frame
                currentPoseTimestamp = rawPoseTimestamp;
            }

            if (currentBodyTimestamp != rawBodyTimestamp)
            {
                // new body frame
                currentBodyTimestamp = rawBodyTimestamp;
            }
        }


        public virtual bool UpdateSensorData(KinectInterop.SensorData sensorData, KinectManager kinectManager, bool isPlayMode)
        {
            // color frame
            lock (colorFrameLock)
            {
                if (rawColorImage != null && sensorData.lastColorFrameTime != currentColorTimestamp && !isPlayMode)
                {
                    Texture2D colorImageTex2D = sensorData.colorImageTexture as Texture2D;
                    if (colorImageTex2D != null)
                    {
                        colorImageTex2D.LoadRawTextureData(rawColorImage);
                        colorImageTex2D.Apply();
                    }

                    sensorData.lastColorFrameTime = currentColorTimestamp;
                    //Debug.Log("UpdateColorTimestamp: " + currentColorTimestamp);
                }
            }

            // depth frame
            lock (depthFrameLock)
            {
                if (rawDepthImage != null && sensorData.lastDepthFrameTime != currentDepthTimestamp && !isPlayMode)
                {
                    // depth image
                    if (sensorData.depthImage != null)
                    {
                        //Buffer.BlockCopy(rawDepthImage, 0, sensorData.depthImage, 0, rawDepthImage.Length * sizeof(ushort));
                        KinectInterop.CopyBytes(rawDepthImage, sizeof(ushort), sensorData.depthImage, sizeof(ushort));
                    }

                    sensorData.lastDepthFrameTime = currentDepthTimestamp;
                    //Debug.Log("UpdateDepthTimestamp: " + currentDepthTimestamp);
                }
            }

            // depth hist frame
            lock(depthImageDataLock)
            {
                if (equalHistBufferData != null && sensorData.lastDepthHistTime != lastDepthImageTimestamp && !isPlayMode)
                {
                    if (sensorData.depthHistBufferData != null)
                    {
                        KinectInterop.CopyBytes(equalHistBufferData, sizeof(int), sensorData.depthHistBufferData, sizeof(int));
                    }

                    sensorData.depthHistTotalPoints = histDataTotalPoints;
                    sensorData.lastDepthHistTime = lastDepthImageTimestamp;
                    //Debug.Log("UpdateDepthHistTimestamp: " + lastDepthImageTimestamp);
                }
            }

            // infrared frame
            lock (infraredFrameLock)
            {
                if (rawInfraredImage != null && sensorData.lastInfraredFrameTime != currentInfraredTimestamp && !isPlayMode)
                {
                    if (sensorData.infraredImage != null)
                    {
                        //Buffer.BlockCopy(rawInfraredImage, 0, sensorData.infraredImage, 0, rawInfraredImage.Length * sizeof(ushort));
                        KinectInterop.CopyBytes(rawInfraredImage, sizeof(ushort), sensorData.infraredImage, sizeof(ushort));
                    }

                    sensorData.lastInfraredFrameTime = currentInfraredTimestamp;
                    //Debug.Log("UpdateInfraredTimestamp: " + currentDepthTimestamp);
                }
            }

            // save the current pose frame time
            ulong lastSensorPoseFrameTime = sensorData.lastSensorPoseFrameTime;

            // pose frame
            lock (poseFrameLock)
            {
                if (sensorData.lastSensorPoseFrameTime != currentPoseTimestamp && !isPlayMode)
                {
                    Quaternion localPoseRot = rawPoseRotation;
                    if (sensorRotIgnoreY)
                    {
                        Vector3 localPoseRotEuler = localPoseRot.eulerAngles;
                        localPoseRotEuler.y = sensorRotValueY;
                        localPoseRot = Quaternion.Euler(localPoseRotEuler);
                    }

                    Quaternion corrPoseRotation = Quaternion.Euler(sensorRotOffset) * localPoseRot;
                    if(sensorRotFlipZ)
                    {
                        Vector3 corrPoseRotEuler = corrPoseRotation.eulerAngles;
                        corrPoseRotEuler.z = -corrPoseRotEuler.z;
                        corrPoseRotation = Quaternion.Euler(corrPoseRotEuler);
                    }

                    matLocalPose.SetTRS(rawPosePosition, corrPoseRotation, Vector3.one);
                    Matrix4x4 matTransform = matTransformPose * matLocalPose;

                    sensorPosePosition = matTransform.GetColumn(3);
                    sensorPoseRotation = matTransform.rotation;

                    sensorData.sensorPosePosition = sensorPosePosition;
                    sensorData.sensorPoseRotation = sensorPoseRotation;

                    sensorData.lastSensorPoseFrameTime = currentPoseTimestamp;
                    //Debug.Log("UpdatePoseTimestamp: " + currentPoseTimestamp);
                }
            }

            // check if the pose data has changed
            if (lastSensorPoseFrameTime != sensorData.lastSensorPoseFrameTime)
            {
                if (kinectManager.getPoseFrames != KinectManager.PoseUsageType.RawPoseData)
                {
                    switch (kinectManager.getPoseFrames)
                    {
                        case KinectManager.PoseUsageType.DisplayInfo:
                            if(kinectManager.statusInfoText != null)
                            {
                                kinectManager.statusInfoText.text = string.Format("Sensor position: ({0:F2}, {1:F2}, {2:F2}), rotation: {3}", 
                                    sensorPosePosition.x, sensorPosePosition.y, sensorPosePosition.z, sensorPoseRotation.eulerAngles);
                            }
                            break;

                        case KinectManager.PoseUsageType.UpdateTransform:
                            transform.position = sensorPosePosition;  // sensorData.sensorPosePosition;
                            transform.rotation = sensorPoseRotation;  // sensorData.sensorPoseRotation;
                            //sensorData.sensorTransformUpdated = true;
                            break;
                    }
                }
            }

            // body frame
            lock (bodyTrackerLock)
            {
                if (sensorData.lastBodyFrameTime != currentBodyTimestamp)
                {
                    // body index image
                    if (rawBodyIndexImage != null && sensorData.bodyIndexImage != null)
                    {
                        sensorData.lastBodyIndexFrameTime = currentBodyTimestamp;
                        KinectInterop.CopyBytes(rawBodyIndexImage, sizeof(byte), sensorData.bodyIndexImage, sizeof(byte));
                    }

                    // number of bodies
                    sensorData.trackedBodiesCount = trackedBodiesCount;

                    // create the needed slots
                    if (sensorData.alTrackedBodies.Length < trackedBodiesCount)
                    {
                        //sensorData.alTrackedBodies.Add(new KinectInterop.BodyData((int)KinectInterop.JointType.Count));
                        Array.Resize<KinectInterop.BodyData>(ref sensorData.alTrackedBodies, (int)trackedBodiesCount);

                        for(int i = 0; i < trackedBodiesCount; i++)
                        {
                            sensorData.alTrackedBodies[i] = new KinectInterop.BodyData((int)KinectInterop.JointType.Count);
                        }
                    }

                    //alTrackedBodies.CopyTo(sensorData.alTrackedBodies);
                    for (int i = 0; i < trackedBodiesCount; i++)
                    {
                        //sensorData.alTrackedBodies[i] = alTrackedBodies[i];
                        //KinectInterop.CopyBytes<KinectInterop.BodyData>(alTrackedBodies[i], ref sensorData.alTrackedBodies[i]);
                        alTrackedBodies[i].CopyTo(ref sensorData.alTrackedBodies[i]);

                        //KinectInterop.BodyData bodyData = sensorData.alTrackedBodies[i];
                        //Debug.Log("  (U)User ID: " + bodyData.liTrackingID + ", body: " + i + ", pos: " + bodyData.kinectPos);
                    }

                    sensorData.lastBodyFrameTime = currentBodyTimestamp;
                    //Debug.Log("UpdateBodyTimestamp: " + currentBodyTimestamp);
                }
            }

            // body hist frame
            lock (bodyImageDataLock)
            {
                if (equalBodyBufferData != null && sensorData.lastBodyHistTime != lastBodyImageTimestamp && !isPlayMode)
                {
                    if (sensorData.bodyHistBufferData != null)
                    {
                        KinectInterop.CopyBytes(equalBodyBufferData, sizeof(int), sensorData.bodyHistBufferData, sizeof(int));
                    }

                    sensorData.bodyHistTotalPoints = histBodyTotalPoints;
                    sensorData.lastBodyHistTime = lastBodyImageTimestamp;
                    //Debug.Log("UpdateBodyHistTimestamp: " + lastBodyImageTimestamp);
                }
            }

            return true;
        }


        // returns the point cloud texture resolution
        public Vector2Int GetPointCloudTexResolution(KinectInterop.SensorData sensorData)
        {
            Vector2Int texRes = Vector2Int.zero;

            switch (pointCloudResolution)
            {
                case PointCloudResolution.DepthCameraResolution:
                    texRes = new Vector2Int(sensorData.depthImageWidth, sensorData.depthImageHeight);
                    break;

                case PointCloudResolution.ColorCameraResolution:
                    texRes = new Vector2Int(sensorData.colorImageWidth, sensorData.colorImageHeight);
                    break;
            }

            if(texRes == Vector2Int.zero)
            {
                throw new Exception("Unsupported point cloud resolution: " + pointCloudResolution + " or the respective image is not available.");
            }

            return texRes;
        }


        // creates the point-cloud vertex shader and its respective buffers, as needed
        protected virtual bool CreatePointCloudVertexShader(KinectInterop.SensorData sensorData)
        {
            if (sensorData.depthCamIntr == null || sensorData.depthCamIntr.distType == KinectInterop.DistortionType.None)
                return false;

            pointCloudVertexRes = GetPointCloudTexResolution(sensorData);

            if (pointCloudVertexRT == null)
            {
                pointCloudVertexRT = new RenderTexture(pointCloudVertexRes.x, pointCloudVertexRes.y, 0, RenderTextureFormat.ARGBHalf);
                pointCloudVertexRT.enableRandomWrite = true;
                pointCloudVertexRT.Create();
            }

            if (pointCloudVertexShader == null)
            {
                pointCloudVertexShader = Resources.Load("PointCloudVertexShaderAll") as ComputeShader;
                pointCloudVertexKernel = pointCloudVertexShader != null ? pointCloudVertexShader.FindKernel("BakeVertexTex") : -1;
            }

            if (pointCloudSpaceBuffer == null)
            {
                int spaceBufferLength = pointCloudVertexRes.x * pointCloudVertexRes.y * 3;
                pointCloudSpaceBuffer = new ComputeBuffer(spaceBufferLength, sizeof(float));

                // depth2space table
                //Debug.Log("Started creating space tables...");
                //float fTimeStart = Time.realtimeSinceStartup;

                //int depthImageLength = pointCloudVertexRes.x * pointCloudVertexRes.y;
                //Vector3[] depth2SpaceTable = new Vector3[depthImageLength];

                //for (int dy = 0, di = 0; dy < pointCloudVertexRes.y; dy++)
                //{
                //    for (int dx = 0; dx < pointCloudVertexRes.x; dx++, di++)
                //    {
                //        Vector2 depthPos = new Vector2(dx, dy);
                //        depth2SpaceTable[di] = pointCloudResolution == PointCloudResolution.ColorCameraResolution ?
                //            MapColorPointToSpaceCoords(sensorData, depthPos, 1000) : MapDepthPointToSpaceCoords(sensorData, depthPos, 1000);
                //    }
                //}

                depth2SpaceTable = pointCloudResolution == PointCloudResolution.ColorCameraResolution ?
                    GetColorCameraSpaceTable(sensorData) : GetDepthCameraSpaceTable(sensorData);

                //// parallelize for gaining time
                //System.Threading.Tasks.Parallel.For(0, pointCloudVertexRes.y, dy =>
                //{
                //    int di = dy * pointCloudVertexRes.x;

                //    for (var dx = 0; dx < pointCloudVertexRes.x; dx++, di++)
                //    {
                //        Vector2 depthPos = new Vector2(dx, dy);
                //        depth2SpaceTable[di] = pointCloudResolution == PointCloudResolution.ColorCameraResolution ?
                //            MapColorPointToSpaceCoords(sensorData, depthPos, 1000) : MapDepthPointToSpaceCoords(sensorData, depthPos, 1000);
                //    }
                //});

                //Debug.Log("depth2SpaceTable: " + depth2SpaceTable);
                pointCloudSpaceBuffer.SetData(depth2SpaceTable);
                depth2SpaceTable = null;
                //Debug.Log("Finished creating space tables in " + (Time.realtimeSinceStartup - fTimeStart) + "s");
            }

            if (pointCloudDepthBuffer == null)
            {
                int depthBufferLength = pointCloudVertexRes.x * pointCloudVertexRes.y / 2;
                pointCloudDepthBuffer = new ComputeBuffer(depthBufferLength, sizeof(uint));
            }

            if (pointCloudResolution == PointCloudResolution.ColorCameraResolution && color2DepthDataFrame == null)
            {
                color2DepthDataFrame = new ushort[sensorData.colorImageWidth * sensorData.colorImageHeight];
            }

            return true;
        }


        // disposes the point-cloud vertex shader and its respective buffers
        protected virtual void DisposePointCloudVertexShader(KinectInterop.SensorData sensorData)
        {
            if (pointCloudSpaceBuffer != null)
            {
                pointCloudSpaceBuffer.Dispose();
                pointCloudSpaceBuffer = null;
            }

            if (pointCloudDepthBuffer != null)
            {
                pointCloudDepthBuffer.Dispose();
                pointCloudDepthBuffer = null;
            }

            if (pointCloudCoordBuffer != null)
            {
                // K2 color camera resolution
                pointCloudCoordBuffer.Dispose();
                pointCloudCoordBuffer = null;
            }

            if (pointCloudVertexRT != null)
            {
                pointCloudVertexRT.Release();
                pointCloudVertexRT = null;
            }

            if (color2DepthDataFrame != null)
            {
                color2DepthDataFrame = null;
            }

            if (color2DepthCoordFrame != null)
            {
                color2DepthDataFrame = null;
            }

            if (pointCloudVertexShader != null)
            {
                pointCloudVertexShader = null;
            }
        }


        // updates the point-cloud vertex shader with the actual data
        protected virtual bool UpdatePointCloudVertexShader(KinectInterop.SensorData sensorData)
        {
            if (pointCloudVertexShader != null && sensorData.depthImage != null && pointCloudVertexRT != null &&
                sensorData.lastDepth2SpaceFrameTime != sensorData.lastDepthFrameTime)
            {
                sensorData.lastDepth2SpaceFrameTime = sensorData.lastDepthFrameTime;

                if (pointCloudResolution == PointCloudResolution.ColorCameraResolution)
                {
                    lock(color2DepthFrameLock)
                    {
                        KinectInterop.SetComputeBufferData(pointCloudDepthBuffer, color2DepthDataFrame, color2DepthDataFrame.Length >> 1, sizeof(uint));
                    }
                }
                else
                {
                    KinectInterop.SetComputeBufferData(pointCloudDepthBuffer, sensorData.depthImage, sensorData.depthImage.Length >> 1, sizeof(uint));
                }

                KinectInterop.SetComputeShaderInt2(pointCloudVertexShader, "PointCloudRes", pointCloudVertexRes.x, pointCloudVertexRes.y);
                KinectInterop.SetComputeShaderFloat2(pointCloudVertexShader, "SpaceScale", sensorData.sensorSpaceScale.x, sensorData.sensorSpaceScale.y);
                pointCloudVertexShader.SetInt("MinDepth", (int)(minDistance * 1000f));
                pointCloudVertexShader.SetInt("MaxDepth", (int)(maxDistance * 1000f));
                pointCloudVertexShader.SetBuffer(pointCloudVertexKernel, "SpaceTable", pointCloudSpaceBuffer);
                pointCloudVertexShader.SetBuffer(pointCloudVertexKernel, "DepthMap", pointCloudDepthBuffer);
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
        protected virtual bool CreatePointCloudColorShader(KinectInterop.SensorData sensorData)
        {
            //renderDepthAlignedColorTexture.enableRandomWrite = true;

            //if (pointCloudColorRT == null)
            //{
            //    pointCloudColorRT = new RenderTexture(sensorData.depthImageWidth, sensorData.depthImageHeight, 0, RenderTextureFormat.ARGB32);
            //    pointCloudColorRT.enableRandomWrite = true;
            //    pointCloudColorRT.Create();
            //}

            pointCloudColorRes = GetPointCloudTexResolution(sensorData);

            if(pointCloudResolution == PointCloudResolution.DepthCameraResolution)
            {
                if (pointCloudAlignedColorTex == null)
                {
                    pointCloudAlignedColorTex = new Texture2D(sensorData.depthImageWidth, sensorData.depthImageHeight, sensorData.colorImageFormat, false);
                }

                if (depth2ColorDataFrame == null)
                {
                    depth2ColorDataFrame = new byte[sensorData.depthImageWidth * sensorData.depthImageHeight * sensorData.colorImageStride];
                }
            }

            return true;
        }


        // disposes the point-cloud color shader and its respective buffers
        protected virtual void DisposePointCloudColorShader(KinectInterop.SensorData sensorData)
        {
            if (pointCloudCoordBuffer != null)
            {
                // K2 depth camera resolution
                pointCloudCoordBuffer.Dispose();
                pointCloudCoordBuffer = null;
            }

            if (pointCloudColorRT)
            {
                pointCloudColorRT.Release();
                pointCloudColorRT = null;
            }

            if (pointCloudAlignedColorTex != null)
            {
                Destroy(pointCloudAlignedColorTex);
                pointCloudAlignedColorTex = null;
            }

            if (depth2ColorDataFrame != null)
            {
                depth2ColorDataFrame = null;
            }

            if (depth2ColorCoordFrame != null)
            {
                depth2ColorCoordFrame = null;
            }

            if (pointCloudColorShader != null)
            {
                pointCloudColorShader = null;
            }
        }


        // updates the point-cloud color shader with the actual data
        protected virtual bool UpdatePointCloudColorShader(KinectInterop.SensorData sensorData)
        {
            Texture texColor = null;

            if (pointCloudResolution == PointCloudResolution.DepthCameraResolution)
            {
                if (pointCloudAlignedColorTex != null && depth2ColorDataFrame != null && sensorData.lastDepth2ColorFrameTime != lastDepth2ColorFrameTime)
                {
                    lock (depth2ColorFrameLock)
                    {
                        sensorData.lastDepth2ColorFrameTime = lastDepth2ColorFrameTime;

                        pointCloudAlignedColorTex.LoadRawTextureData(depth2ColorDataFrame);
                        pointCloudAlignedColorTex.Apply();
                    }

                    if (pointCloudColorRT != null)
                    {
                        Graphics.CopyTexture(pointCloudAlignedColorTex, pointCloudColorRT);
                    }

                    texColor = pointCloudAlignedColorTex;
                }
            }
            else
            {
                texColor = sensorData.colorImageTexture;
            }

            if(texColor != null)
            {
                Graphics.Blit(texColor, pointCloudColorTexture);
                return true;
            }

            return false;
        }


        // creates the color-depth shader and its respective buffers, as needed
        protected virtual bool CreateColorDepthShader(KinectInterop.SensorData sensorData)
        {
            if (color2DepthDataFrame == null)
            {
                color2DepthDataFrame = new ushort[sensorData.colorImageWidth * sensorData.colorImageHeight];
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


        // disposes the color-depth shader and its respective buffers
        protected virtual void DisposeColorDepthShader(KinectInterop.SensorData sensorData)
        {
            if (color2DepthDataFrame != null)
            {
                color2DepthDataFrame = null;
            }

            if (sensorData.colorDepthTexture != null)
            {
                sensorData.colorDepthTexture.Release();
                sensorData.colorDepthTexture = null;
            }

            if (pointCloudDepthBuffer != null)
            {
                pointCloudDepthBuffer.Dispose();
                pointCloudDepthBuffer = null;
            }

            if (pointCloudCoordBuffer != null)
            {
                pointCloudCoordBuffer.Dispose();
                pointCloudCoordBuffer = null;
            }

            if (color2DepthCoordFrame != null)
            {
                color2DepthCoordFrame = null;
            }

            if (colorDepthShader != null)
            {
                colorDepthShader = null;
            }

            colorDepthShaderInited = false;
        }


        // updates the color-depth shader with the actual data
        protected virtual bool UpdateColorDepthShader(KinectInterop.SensorData sensorData)
        {
            if (color2DepthDataFrame != null)
            {
                if(sensorData.usedColorDepthBufferTime == sensorData.lastColorDepthBufferTime && sensorData.lastColorDepthBufferTime != lastColor2DepthFrameTime)
                {
                    lock(color2DepthFrameLock)
                    {
                        if (sensorData.colorImageTexture != null)
                        {
                            Graphics.Blit(sensorData.colorImageTexture, sensorData.colorDepthTexture);
                        }

                        int bufferLength = sensorData.colorImageWidth * sensorData.colorImageHeight / 2;
                        KinectInterop.SetComputeBufferData(sensorData.colorDepthBuffer, color2DepthDataFrame, bufferLength, sizeof(uint));
                        sensorData.lastColorDepthBufferTime = lastColor2DepthFrameTime;
                    }
                }

                return true;
            }

            return false;
        }


        // creates the depth-tex shader and its respective buffers, as needed
        protected virtual bool CreateDepthTexShader(KinectInterop.SensorData sensorData)
        {
            Shader depthTexShader = Shader.Find("Kinect/DepthTexShader");
            if (depthTexShader != null)
            {
                sensorData.depthTexMaterial = new Material(depthTexShader);

                if (sensorData.depthImageBuffer == null)
                {
                    int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                    sensorData.depthImageBuffer = KinectInterop.CreateComputeBuffer(sensorData.depthImageBuffer, depthBufferLength, sizeof(uint));
                }
            }

            return true;
        }


        // disposes the depth-tex shader and its respective buffers
        protected virtual void DisposeDepthTexShader(KinectInterop.SensorData sensorData)
        {
            if (sensorData.depthTexTexture != null)
            {
                sensorData.depthTexTexture.Release();
                sensorData.depthTexTexture = null;
            }

            if (sensorData.depthImageBuffer != null)
            {
                sensorData.depthImageBuffer.Dispose();
                sensorData.depthImageBuffer = null;
            }

            sensorData.depthTexMaterial = null;
        }


        // creates the infrared-tex shader and its respective buffers, as needed
        protected virtual bool CreateInfraredTexShader(KinectInterop.SensorData sensorData)
        {
            Shader infraredTexShader = Shader.Find("Kinect/DepthTexShader");
            if (infraredTexShader != null)
            {
                sensorData.infraredTexMaterial = new Material(infraredTexShader);

                if (sensorData.infraredImageBuffer == null)
                {
                    int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                    sensorData.infraredImageBuffer = KinectInterop.CreateComputeBuffer(sensorData.infraredImageBuffer, depthBufferLength, sizeof(uint));
                }
            }

            return true;
        }


        // disposes the infrared-tex shader and its respective buffers
        protected virtual void DisposeInfraredTexShader(KinectInterop.SensorData sensorData)
        {
            if (sensorData.infraredTexTexture != null)
            {
                sensorData.infraredTexTexture.Release();
                sensorData.infraredTexTexture = null;
            }

            if (sensorData.infraredImageBuffer != null)
            {
                sensorData.infraredImageBuffer.Dispose();
                sensorData.infraredImageBuffer = null;
            }

            sensorData.infraredTexMaterial = null;
        }


        // updates transformed frame textures, if needed
        public virtual bool UpdateTransformedFrameTextures(KinectInterop.SensorData sensorData, KinectManager kinectManager)
        {
            // depth2space frame
            if (pointCloudVertexTexture != null)
            {
                if (pointCloudVertexShader != null || CreatePointCloudVertexShader(sensorData))
                {
                    UpdatePointCloudVertexShader(sensorData);
                }
            }
            else
            {
                if (pointCloudVertexShader != null)
                {
                    DisposePointCloudVertexShader(sensorData);
                }
            }

            // depth2color frame
            if (pointCloudColorTexture != null)
            {
                if (pointCloudColorShader != null || pointCloudAlignedColorTex != null || CreatePointCloudColorShader(sensorData))
                {
                    UpdatePointCloudColorShader(sensorData);
                }
            }
            else
            {
                if (pointCloudColorShader != null || pointCloudAlignedColorTex != null)
                {
                    DisposePointCloudColorShader(sensorData);
                }
            }

            // color2depth
            if (sensorData.colorDepthBuffer != null)
            {
                if(colorDepthShaderInited || CreateColorDepthShader(sensorData))
                {
                    UpdateColorDepthShader(sensorData);
                }
            }
            else
            {
                if(colorDepthShaderInited)
                {
                    DisposeColorDepthShader(sensorData);
                }
            }

            // depth-tex
            if (sensorData.depthTexTexture != null)
            {
                if (sensorData.depthTexMaterial != null || CreateDepthTexShader(sensorData))
                {
                    //UpdateDepthTexShader(sensorData);  // code moved to UpdateSensorTextures()
                }
            }
            else
            {
                if (sensorData.depthTexMaterial != null)
                {
                    DisposeDepthTexShader(sensorData);
                }
            }

            // infrared-tex
            if (sensorData.infraredTexTexture != null)
            {
                if (sensorData.infraredTexMaterial != null || CreateInfraredTexShader(sensorData))
                {
                    //UpdateInfraredTexShader(sensorData);  // code moved to UpdateSensorTextures()
                }
            }
            else
            {
                if (sensorData.infraredTexMaterial != null)
                {
                    DisposeInfraredTexShader(sensorData);
                }
            }

            return true;
        }


        public virtual bool UpdateSensorTextures(KinectInterop.SensorData sensorData, KinectManager kinectManager, ulong prevDepthFrameTime)
        {
            // check if the depth data has changed
            if (prevDepthFrameTime != sensorData.lastDepthFrameTime)
            {
                // depth texture
                if (sensorData.depthImageTexture != null && sensorData.depthImageMaterial != null &&
                    sensorData.lastDepthImageTime != sensorData.lastDepthFrameTime)
                {
                    if (sensorData.depthImageBuffer != null && sensorData.depthImage != null)
                    {
                        int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                        KinectInterop.SetComputeBufferData(sensorData.depthImageBuffer, sensorData.depthImage, depthBufferLength, sizeof(uint));
                    }

                    if (sensorData.depthHistBuffer != null && sensorData.depthHistBufferData != null)
                    {
                        //sensorData.depthHistBuffer.SetData(equalHistBufferData);
                        KinectInterop.SetComputeBufferData(sensorData.depthHistBuffer, sensorData.depthHistBufferData, sensorData.depthHistBufferData.Length, sizeof(int));
                    }

                    sensorData.depthImageMaterial.SetInt("_TexResX", sensorData.depthImageWidth);
                    sensorData.depthImageMaterial.SetInt("_TexResY", sensorData.depthImageHeight);
                    sensorData.depthImageMaterial.SetInt("_MinDepth", (int)(minDistance * 1000f));
                    sensorData.depthImageMaterial.SetInt("_MaxDepth", (int)(maxDistance * 1000f));
                    sensorData.depthImageMaterial.SetInt("_TotalPoints", sensorData.depthHistTotalPoints);
                    sensorData.depthImageMaterial.SetBuffer("_DepthMap", sensorData.depthImageBuffer);
                    sensorData.depthImageMaterial.SetBuffer("_HistMap", sensorData.depthHistBuffer);

                    Graphics.Blit(null, sensorData.depthImageTexture, sensorData.depthImageMaterial);

                    sensorData.lastDepthImageTime = sensorData.lastDepthFrameTime;
                    //Debug.Log("DepthTextureTimestamp: " + sensorData.lastDepthImageTime);
                }

                // infrared texture
                if (sensorData.infraredImageTexture != null && sensorData.infraredImageMaterial != null &&
                    sensorData.lastInfraredImageTime != sensorData.lastInfraredFrameTime)
                {
                    if (sensorData.infraredImageBuffer != null && sensorData.infraredImage != null)
                    {
                        int infraredBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                        KinectInterop.SetComputeBufferData(sensorData.infraredImageBuffer, sensorData.infraredImage, infraredBufferLength, sizeof(uint));
                    }

                    sensorData.infraredImageMaterial.SetInt("_TexResX", sensorData.depthImageWidth);
                    sensorData.infraredImageMaterial.SetInt("_TexResY", sensorData.depthImageHeight);
                    sensorData.infraredImageMaterial.SetFloat("_MinValue", minInfraredValue);
                    sensorData.infraredImageMaterial.SetFloat("_MaxValue", maxInfraredValue);
                    sensorData.infraredImageMaterial.SetBuffer("_InfraredMap", sensorData.infraredImageBuffer);

                    Graphics.Blit(null, sensorData.infraredImageTexture, sensorData.infraredImageMaterial);

                    sensorData.lastInfraredImageTime = sensorData.lastInfraredFrameTime;
                    //Debug.Log("InfraredTextureTimestamp: " + sensorData.lastInfraredImageTime);
                }

                // user texture & body texture
                if (sensorData.bodyImageTexture != null && sensorData.bodyImageMaterial != null &&
                    sensorData.lastBodyImageTime != sensorData.lastBodyIndexFrameTime)
                {
                    if (sensorData.bodyIndexBuffer != null && rawBodyIndexImage != null)
                    {
                        int bodyIndexBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 4;
                        KinectInterop.SetComputeBufferData(sensorData.bodyIndexBuffer, rawBodyIndexImage, bodyIndexBufferLength, sizeof(uint));
                    }

                    if (sensorData.bodyHistBuffer != null && sensorData.bodyHistBufferData != null)
                    {
                        //sensorData.depthHistBuffer.SetData(equalBodyBufferData);
                        KinectInterop.SetComputeBufferData(sensorData.bodyHistBuffer, sensorData.bodyHistBufferData, sensorData.bodyHistBufferData.Length, sizeof(int));
                    }

                    float minDist = kinectManager.minUserDistance != 0f ? kinectManager.minUserDistance : minDistance;
                    float maxDist = kinectManager.maxUserDistance != 0f ? kinectManager.maxUserDistance : maxDistance;

                    sensorData.bodyImageMaterial.SetInt("_TexResX", sensorData.depthImageWidth);
                    sensorData.bodyImageMaterial.SetInt("_TexResY", sensorData.depthImageHeight);
                    sensorData.bodyImageMaterial.SetInt("_MinDepth", (int)(minDist * 1000f));
                    sensorData.bodyImageMaterial.SetInt("_MaxDepth", (int)(maxDist * 1000f));

                    sensorData.bodyImageMaterial.SetBuffer("_BodyIndexMap", sensorData.bodyIndexBuffer);

                    if(kinectManager.getBodyFrames == KinectManager.BodyTextureType.UserTexture)
                    {
                        if (sensorData.depthImageBuffer != null && sensorData.depthImage != null)
                        {
                            int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                            KinectInterop.SetComputeBufferData(sensorData.depthImageBuffer, sensorData.depthImage, depthBufferLength, sizeof(uint));
                        }

                        sensorData.bodyImageMaterial.SetBuffer("_DepthMap", sensorData.depthImageBuffer);
                        sensorData.bodyImageMaterial.SetBuffer("_HistMap", sensorData.bodyHistBuffer);
                        sensorData.bodyImageMaterial.SetInt("_TotalPoints", sensorData.bodyHistTotalPoints);
                        //sensorData.bodyImageMaterial.SetInt("_FirstUserIndex", sensorData.firstUserIndex);

                        Color[] bodyIndexColors = kinectManager.GetBodyIndexColors();
                        sensorData.bodyImageMaterial.SetColorArray("_BodyIndexColors", bodyIndexColors);
                    }

                    Graphics.Blit(null, sensorData.bodyImageTexture, sensorData.bodyImageMaterial);

                    sensorData.lastBodyImageTime = sensorData.lastBodyIndexFrameTime;
                    //Debug.Log("BodyTextureTimestamp: " + sensorData.lastBodyImageTime);
                }

                // depth-tex
                if (sensorData.depthTexMaterial != null && 
                    sensorData.lastDepthTexTime != sensorData.lastDepthFrameTime)
                {
                    if (sensorData.depthImageBuffer != null && sensorData.depthImage != null)
                    {
                        int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                        KinectInterop.SetComputeBufferData(sensorData.depthImageBuffer, sensorData.depthImage, depthBufferLength, sizeof(uint));
                    }

                    sensorData.depthTexMaterial.SetBuffer("_DepthMap", sensorData.depthImageBuffer);
                    sensorData.depthTexMaterial.SetInt("_TexResX", sensorData.depthImageWidth);
                    sensorData.depthTexMaterial.SetInt("_TexResY", sensorData.depthImageHeight);
                    sensorData.depthTexMaterial.SetInt("_MinDepth", (int)(minDistance * 1000f));
                    sensorData.depthTexMaterial.SetInt("_MaxDepth", (int)(maxDistance * 1000f));

                    Graphics.Blit(null, sensorData.depthTexTexture, sensorData.depthTexMaterial);
                    sensorData.lastDepthTexTime = sensorData.lastDepthFrameTime;
                    //Debug.Log("DepthTexTimestamp: " + sensorData.lastDepthTexTime);
                }

                // infrared-tex
                if (sensorData.infraredTexMaterial != null &&
                    sensorData.lastInfraredTexTime != sensorData.lastInfraredFrameTime)
                {
                    if (sensorData.infraredImageBuffer != null && sensorData.infraredImage != null)
                    {
                        int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                        KinectInterop.SetComputeBufferData(sensorData.infraredImageBuffer, sensorData.infraredImage, depthBufferLength, sizeof(uint));
                    }

                    sensorData.infraredTexMaterial.SetBuffer("_DepthMap", sensorData.infraredImageBuffer);
                    sensorData.infraredTexMaterial.SetInt("_TexResX", sensorData.depthImageWidth);
                    sensorData.infraredTexMaterial.SetInt("_TexResY", sensorData.depthImageHeight);
                    sensorData.infraredTexMaterial.SetInt("_MinDepth", (int)(minDistance * 1000f));
                    sensorData.infraredTexMaterial.SetInt("_MaxDepth", (int)(maxDistance * 1000f));

                    Graphics.Blit(null, sensorData.infraredTexTexture, sensorData.infraredTexMaterial);
                    sensorData.lastInfraredTexTime = sensorData.lastInfraredFrameTime;
                    //Debug.Log("InfraredTexTimestamp: " + sensorData.lastInfraredTexTime);
                }

            }

            return true;
        }


        // returns sensor-to-world matrix
        public virtual Matrix4x4 GetSensorToWorldMatrix()
        {
            Matrix4x4 mSensor = Matrix4x4.identity;
            mSensor.SetTRS(sensorPosePosition, sensorPoseRotation, Vector3.one);

            return mSensor;
        }


        // returns sensor rotation, properly adjusted for body tracking
        protected Quaternion GetSensorRotationNotZFlipped(bool bInverted)
        {
            Vector3 sensorRotEuler = sensorPoseRotation.eulerAngles;

            if (sensorRotFlipZ)
            {
                sensorRotEuler.z = -sensorRotEuler.z;
            }

            Quaternion sensorRot = Quaternion.Euler(sensorRotEuler);
            return bInverted ? Quaternion.Inverse(sensorRot) : sensorRot; 
        }


        // returns sensor transform. Please note transform updates depend on the getPoseFrames-KM setting.
        public virtual Transform GetSensorTransform()
        {
            return transform;
        }


        // unprojects plane point into the space
        protected virtual Vector3 UnprojectPoint(KinectInterop.CameraIntrinsics intr, Vector2 pixel, float depth)
        {
            return Vector3.zero;
        }


        // projects space point onto a plane
        protected virtual Vector2 ProjectPoint(KinectInterop.CameraIntrinsics intr, Vector3 point)
        {
            return Vector2.zero;
        }


        // transforms a point from one space to another
        protected virtual Vector3 TransformPoint(KinectInterop.CameraExtrinsics extr, Vector3 point)
        {
            return Vector3.zero;
        }


        public virtual Vector3[] GetDepthCameraSpaceTable(KinectInterop.SensorData sensorData)
        {
            if (sensorData == null)
                return null;

            // depth2space table
            int depthImageLength = sensorData.depthImageWidth * sensorData.depthImageHeight;
            if (depth2SpaceTable == null || depth2SpaceTable.Length != depthImageLength)
            {
                depth2SpaceTable = new Vector3[depthImageLength];

                for (int dy = 0, di = 0; dy < sensorData.depthImageHeight; dy++)
                {
                    for (int dx = 0; dx < sensorData.depthImageWidth; dx++, di++)
                    {
                        Vector2 depthPos = new Vector2(dx, dy);
                        depth2SpaceTable[di] = MapDepthPointToSpaceCoords(sensorData, depthPos, 1000);
                    }
                }
            }

            return depth2SpaceTable;
        }


        public virtual Vector3[] GetColorCameraSpaceTable(KinectInterop.SensorData sensorData)
        {
            if (sensorData == null)
                return null;

            // color2space
            int colorImageLength = sensorData.colorImageWidth * sensorData.colorImageHeight;
            if (color2SpaceTable == null || color2SpaceTable.Length != colorImageLength)
            {
                color2SpaceTable = new Vector3[colorImageLength];

                for (int cy = 0, ci = 0; cy < sensorData.colorImageHeight; cy++)
                {
                    for (int cx = 0; cx < sensorData.colorImageWidth; cx++, ci++)
                    {
                        Vector2 colorPos = new Vector2(cx, cy);
                        color2SpaceTable[ci] = MapColorPointToSpaceCoords(sensorData, colorPos, 1000);
                    }
                }
            }

            return color2SpaceTable;
        }


        public virtual Vector3 MapDepthPointToSpaceCoords(KinectInterop.SensorData sensorData, Vector2 depthPos, ushort depthVal)
        {
            if (sensorData.depthCamIntr != null)
            {
                return UnprojectPoint(sensorData.depthCamIntr, depthPos, (float)depthVal / 1000f);
            }

            return Vector3.zero;
        }


        public virtual Vector2 MapSpacePointToDepthCoords(KinectInterop.SensorData sensorData, Vector3 spacePos)
        {
            if (sensorData.depthCamIntr != null)
            {
                return ProjectPoint(sensorData.depthCamIntr, spacePos);
            }

            return Vector2.zero;
        }


        public virtual Vector3 MapColorPointToSpaceCoords(KinectInterop.SensorData sensorData, Vector2 colorPos, ushort depthVal)
        {
            if (sensorData.colorCamIntr != null)
            {
                return UnprojectPoint(sensorData.colorCamIntr, colorPos, (float)depthVal / 1000f);
            }

            return Vector3.zero;
        }


        public virtual Vector2 MapSpacePointToColorCoords(KinectInterop.SensorData sensorData, Vector3 spacePos)
        {
            if (sensorData.colorCamIntr != null)
            {
                return ProjectPoint(sensorData.colorCamIntr, spacePos);
            }

            return Vector2.zero;
        }


        public virtual Vector2 MapDepthPointToColorCoords(KinectInterop.SensorData sensorData, Vector2 depthPos, ushort depthVal)
        {
            if (sensorData.depthCamIntr != null && sensorData.colorCamIntr != null && sensorData.depth2ColorExtr != null)
            {
                Vector3 depthSpacePos = UnprojectPoint(sensorData.depthCamIntr, depthPos, (float)depthVal / 1000f);
                Vector3 colorSpacePos = TransformPoint(sensorData.depth2ColorExtr, depthSpacePos);
                Vector2 colorPos = ProjectPoint(sensorData.colorCamIntr, colorSpacePos);

                return colorPos;
            }

            return Vector2.zero;
        }


        public virtual Vector2 MapColorPointToDepthCoords(KinectInterop.SensorData sensorData, Vector2 colorPos)
        {
            if (sensorData.depthCamIntr != null && sensorData.colorCamIntr != null && sensorData.color2DepthExtr != null && sensorData.depthImage != null)
            {
                Vector3 colorSpacePos1 = UnprojectPoint(sensorData.colorCamIntr, colorPos, 1f);

                int minDist = (int)(minDistance * 1000f);
                int maxDist = (int)(maxDistance * 1000f);

                int depthImageW = sensorData.depthImageWidth;
                int depthImageL = sensorData.depthImage.Length;

                Vector2 depthPos = Vector2.zero;
                bool bFound = false;

                for (int d = minDist; d < maxDist; d++)
                {
                    Vector3 colorSpacePos = colorSpacePos1 * (d * 0.001f);
                    Vector3 depthSpacePos = TransformPoint(sensorData.color2DepthExtr, colorSpacePos);
                    depthPos = ProjectPoint(sensorData.depthCamIntr, depthSpacePos);

                    int di = (int)(depthPos.y + 0.5f) * depthImageW + (int)(depthPos.x + 0.5f);

                    if (di >= 0 && di < depthImageL)
                    {
                        int z = sensorData.depthImage[di];
                        if ((z != 0) && (z <= d))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                return bFound ? depthPos : Vector2.zero;
            }

            return Vector2.zero;
        }


        //public virtual bool MapDepthFrameToSpaceCoords(KinectInterop.SensorData sensorData, ref Vector3[] vSpaceCoords)
        //{
        //    if (vSpaceCoords == null)
        //    {
        //        vSpaceCoords = new Vector3[sensorData.depthImageWidth * sensorData.depthImageHeight];
        //    }

        //    if (InitCoordMapperSpaceTables(sensorData, true, false))
        //    {
        //        for (int dy = 0, di = 0; dy < sensorData.depthImageHeight; dy++)
        //        {
        //            for (int dx = 0; dx < sensorData.depthImageWidth; dx++, di++)
        //            {
        //                if (sensorData.depthImage[di] != 0)
        //                {
        //                    float depthVal = (float)sensorData.depthImage[di] / 1000f;
        //                    vSpaceCoords[di] = depth2SpaceTable[di] * depthVal;
        //                }
        //                else
        //                {
        //                    vSpaceCoords[di] = Vector3.zero;
        //                }
        //            }
        //        }

        //        return true;
        //    }

        //    return false;
        //}


        //public virtual bool MapDepthFrameToColorData(KinectInterop.SensorData sensorData, ref byte[] vColorFrameData)
        //{
        //    return false;
        //}


        //public virtual bool MapColorFrameToDepthData(KinectInterop.SensorData sensorData, ref ushort[] vDepthFrameData)
        //{
        //    return false;
        //}


        //public virtual bool MapDepthFrameToColorCoords(KinectInterop.SensorData sensorData, ref Vector2[] vColorCoords)
        //{
        //    if (sensorData.depthCamIntr != null && sensorData.colorCamIntr != null && sensorData.depth2ColorExtr != null)
        //    {
        //        int depthImageW = sensorData.depthImageWidth;
        //        int depthImageH = sensorData.depthImageHeight;

        //        int mapImageLen = depthImageW * depthImageH;
        //        if (vColorCoords == null || vColorCoords.Length != mapImageLen)
        //        {
        //            vColorCoords = new Vector2[mapImageLen];
        //        }

        //        for (int dy = 0, dIndex = 0; dy < depthImageH; dy++)
        //        {
        //            for (int dx = 0; dx < depthImageW; dx++, dIndex++)
        //            {
        //                ushort depthVal = sensorData.depthImage[dIndex];

        //                if (depthVal != 0)
        //                {
        //                    Vector2 depthPos = new Vector2(dx, dy);

        //                    Vector3 depthSpacePos = UnprojectPoint(sensorData.depthCamIntr, depthPos, (float)depthVal / 1000f);
        //                    Vector3 colorSpacePos = TransformPoint(sensorData.depth2ColorExtr, depthSpacePos);
        //                    vColorCoords[dIndex] = ProjectPoint(sensorData.colorCamIntr, colorSpacePos);
        //                }
        //                else
        //                {
        //                    vColorCoords[dIndex] = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        //                }
        //            }
        //        }

        //        return true;
        //    }

        //    return false;
        //}

        //public virtual bool MapColorFrameToDepthCoords(KinectInterop.SensorData sensorData, ref Vector2[] vDepthCoords)
        //{
        //    if (sensorData.depthCamIntr != null && sensorData.colorCamIntr != null && sensorData.depth2ColorExtr != null)
        //    {
        //        int depthImageW = sensorData.depthImageWidth;
        //        int depthImageH = sensorData.depthImageHeight;

        //        int mapImageLen = sensorData.colorImageWidth * sensorData.colorImageHeight;
        //        if (vDepthCoords == null || vDepthCoords.Length != mapImageLen)
        //        {
        //            vDepthCoords = new Vector2[mapImageLen];
        //        }

        //        int colorWidth = sensorData.colorCamIntr.width;
        //        int colorHeight = sensorData.colorCamIntr.height;

        //        for (int dy = 0, dIndex = 0; dy < depthImageH; dy++)
        //        {
        //            for (int dx = 0; dx < depthImageW; dx++, dIndex++)
        //            {
        //                ushort depthVal = sensorData.depthImage[dIndex];

        //                if (depthVal != 0)
        //                {
        //                    float depth = (float)depthVal / 1000f;

        //                    Vector2 depthPos1 = new Vector2(dx - 0.5f, dy - 0.5f);
        //                    Vector3 depthSpacePos1 = UnprojectPoint(sensorData.depthCamIntr, depthPos1, depth);
        //                    Vector3 colorSpacePos1 = TransformPoint(sensorData.depth2ColorExtr, depthSpacePos1);
        //                    Vector2 colorPos1 = ProjectPoint(sensorData.colorCamIntr, colorSpacePos1);

        //                    int colorPos1X = Mathf.RoundToInt(colorPos1.x);
        //                    int colorPos1Y = Mathf.RoundToInt(colorPos1.y);

        //                    Vector2 depthPos2 = new Vector2(dx + 0.5f, dy + 0.5f);
        //                    Vector3 depthSpacePos2 = UnprojectPoint(sensorData.depthCamIntr, depthPos2, depth);
        //                    Vector3 colorSpacePos2 = TransformPoint(sensorData.depth2ColorExtr, depthSpacePos2);
        //                    Vector2 colorPos2 = ProjectPoint(sensorData.colorCamIntr, colorSpacePos2);

        //                    int colorPos2X = (int)(colorPos2.x + 0.5f);
        //                    int colorPos2Y = (int)(colorPos2.y + 0.5f);

        //                    if (colorPos1X < 0 || colorPos1Y < 0 || colorPos2X >= colorWidth || colorPos2Y >= colorHeight)
        //                        continue;

        //                    // Transfer between the depth pixels and the pixels inside the rectangle on the other image
        //                    for (int y = colorPos1Y; y <= colorPos2Y; y++)
        //                    {
        //                        int cIndex = y * colorWidth + colorPos1X;

        //                        for (int x = colorPos1X; x <= colorPos2X; x++, cIndex++)
        //                        {
        //                            vDepthCoords[cIndex] = new Vector2(dx, dy);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //vDepthCoords[cIndex] = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        //                }
        //            }
        //        }

        //        return true;
        //    }

        //    return false;
        //}


        // estimates horizontal and vertical FOV
        protected void EstimateFOV(KinectInterop.CameraIntrinsics intr)
        {
            //intr.hFOV = (Mathf.Atan2(intr.ppx + 0.5f, intr.fx) + Mathf.Atan2(intr.width - (intr.ppx + 0.5f), intr.fx)) * 57.2957795f;
            //intr.vFOV = (Mathf.Atan2(intr.ppy + 0.5f, intr.fy) + Mathf.Atan2(intr.height - (intr.ppy + 0.5f), intr.fy)) * 57.2957795f;
            intr.hFOV = 2f * Mathf.Atan2((float)intr.width * 0.5f, intr.fx) * Mathf.Rad2Deg;
            intr.vFOV = 2f * Mathf.Atan2((float)intr.height * 0.5f, intr.fy) * Mathf.Rad2Deg;
        }

        // initializes the body-data structures and start the body tracking
        protected virtual bool InitBodyTracking(KinectInterop.FrameSource dwFlags, KinectInterop.SensorData sensorData, Calibration calibration, bool bCreateTracker)
        {
            try
            {
                if ((dwFlags & KinectInterop.FrameSource.TypeDepth) != 0)  // check for depth stream
                {
                    string bodyTrackingPath = KinectInterop.BODY_TRACKING_TOOLS_FOLDER;
                    if (!string.IsNullOrEmpty(bodyTrackingPath) && bodyTrackingPath[bodyTrackingPath.Length - 1] != '/' && bodyTrackingPath[bodyTrackingPath.Length - 1] != '\\')
                    {
                        bodyTrackingPath += "/";
                    }

                    if (!string.IsNullOrEmpty(bodyTrackingPath) && !bodyTrackingPath.EndsWith("/tools/") && !bodyTrackingPath.EndsWith("\\tools\\") && !bodyTrackingPath.EndsWith("\\tools/"))
                    {
                        bodyTrackingPath += "tools/";
                    }

                    if(!KinectInterop.IsFolderExist(bodyTrackingPath))
                    {
                        Debug.LogWarning("BT-Folder not found: " + bodyTrackingPath);
                    }

                    // copy the needed libraries
                    KinectInterop.CopyFolderFile(bodyTrackingPath, "cublas64_100.dll", ".");
                    KinectInterop.CopyFolderFile(bodyTrackingPath, "cudart64_100.dll", ".");
                    KinectInterop.CopyFolderFile(bodyTrackingPath, "cudnn64_7.dll", ".");

                    //KinectInterop.CopyFolderFile(bodyTrackingPath, "k4abt.dll", ".");
                    KinectInterop.CopyFolderFile(bodyTrackingPath, "onnxruntime.dll", ".");
                    KinectInterop.CopyFolderFile(bodyTrackingPath, "vcomp140.dll", ".");

                    if(KinectInterop.IsFileExist(bodyTrackingPath + "dnn_model_2_0.onnx"))
                        KinectInterop.CopyFolderFile(bodyTrackingPath, "dnn_model_2_0.onnx", ".");
                    else
                        KinectInterop.CopyFolderFile(bodyTrackingPath, "dnn_model.onnx", ".");

                    if ((dwFlags & KinectInterop.FrameSource.TypeBodyIndex) != 0)
                    {
                        rawBodyIndexImage = new byte[sensorData.depthImageWidth * sensorData.depthImageHeight];
                        sensorData.bodyIndexImage = new byte[sensorData.depthImageWidth * sensorData.depthImageHeight];
                    }

                    alTrackedBodies = new List<KinectInterop.BodyData>();
                    sensorData.alTrackedBodies = new KinectInterop.BodyData[0]; // new List<KinectInterop.BodyData>();

                    trackedBodiesCount = 0;
                    sensorData.trackedBodiesCount = 0;

                    btQueueCount = 0;
                    btQueueWaitTime = MAX_BODY_QUEUE_LENGTH <= 1 ? 0 : -1;
                    //Debug.Log("MaxQueueLen: " + MAX_BODY_QUEUE_LENGTH  + ", WaitTime: " + btQueueWaitTime);

                    if (bCreateTracker)
                    {
                        bodyTracker = new BodyTracking(calibration, k4abt_sensor_orientation_t.K4ABT_SENSOR_ORIENTATION_DEFAULT, false);
                        bodyTracker.SetTemporalSmoothing(0f);
                        bodySkeletonData = bodyTracker.CreateBodySkeleton();
                        //Debug.Log("Body tracker successfully created.");

                        // start body-tracker thread
                        bodyTrackerStopEvent = new System.Threading.AutoResetEvent(false);
                        bodyTrackerThread = new System.Threading.Thread(() => BodyTrackerThread(sensorData));
                        bodyTrackerThread.Name = "BT-" + GetType().Name + deviceIndex;
                        bodyTrackerThread.IsBackground = true;
                        bodyTrackerThread.Start();
                    }
                }
                else
                {
                    Debug.LogWarning("Body tracked not started! Please enable the depth stream, to allow tracking the users.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Can't create body tracker for " + GetType().Name + deviceIndex + "!");
                Debug.LogException(ex);
            }

            return (bodyTracker != null);
        }


        // stops the body tracker and releases its data
        protected virtual void StopBodyTracking(KinectInterop.SensorData sensorData)
        {
            if (bodyTrackerThread != null)
            {
                //Debug.Log("Stopping BT thread: " + bodyTrackerThread.Name);

                // stop the frame-polling thread
                bodyTrackerStopEvent.Set();
                bodyTrackerThread.Join();

                bodyTrackerThread = null;
                bodyTrackerStopEvent.Dispose();
                bodyTrackerStopEvent = null;

                //Debug.Log("BT thread stopped.");
            }

            if (bodyTracker != null)
            {
                // wait for all enqueued frames to pop
                int maxWaitTime = 5000;

                while (btQueueCount > 0 && maxWaitTime > 0)
                {
                    IntPtr bodyFrameHandle = bodyTracker.PollBodyFrame(1000);

                    if(bodyFrameHandle != IntPtr.Zero)
                    {
                        Image bodyIndexImage = bodyTracker.GetBodyIndexMap(bodyFrameHandle);
                        bodyIndexImage.Dispose();

                        Capture btCapture = bodyTracker.GetCapture(bodyFrameHandle);
                        btCapture.Dispose();

                        bodyTracker.ReleaseBodyFrame(bodyFrameHandle);
                        bodyFrameHandle = IntPtr.Zero;

                        lock (bodyCaptureLock)
                        {
                            btQueueCount--;
                        }
                    }

                    maxWaitTime -= 1000;
                }

                if(btQueueCount > 0 && maxWaitTime <= 0)
                {
                    Debug.LogWarning("Timed out waiting to pop all BT-frames. QueueCount: " + btQueueCount);
                }

                bodyTracker.ShutdownBodyTracker();
                bodyTracker.Dispose();
                bodyTracker = null;

                lock (bodyCaptureLock)
                {
                    btQueueCount = 0;
                    btQueueWaitTime = 0;
                }

                //Debug.Log("Body tracker disposed.");
            }
        }


        // polls for new body frame
        protected virtual Capture PollBodyFrame(KinectInterop.SensorData sensorData, Capture capture)
        {
            Capture bodyCapture = null;

            if(bodyOutputCapture != null)
            {
                lock (bodyCaptureLock)
                {
                    bodyCapture = bodyOutputCapture;
                    bodyOutputCapture = null;
                }
            }

            // push the new capture
            PushBodyFrameInternal(capture);

            return bodyCapture;
        }


        // polls the body tracker for frames and updates the body-related data in a thread
        private void BodyTrackerThread(KinectInterop.SensorData sensorData)
        {
            if (sensorData == null)
                return;

            while (!bodyTrackerStopEvent.WaitOne(0))
            {
                try
                {
                    //Capture sensorCapture = null;

                    bool bGetBodyData = bodyOutputCapture == null;
                    Capture bodyCapture = PollBodyFrameInternal(sensorData, bGetBodyData/**, sensorCapture*/);

                    if (bodyOutputCapture == null)
                    {
                        lock (bodyCaptureLock)
                        {
                            bodyOutputCapture = bodyCapture;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }


        // maximum queue length
        private const int MAX_BODY_QUEUE_LENGTH = 1;


        // pushes new body frame into bt-processing queue, if possible
        private bool PushBodyFrameInternal(Capture capture)
        {
            bool bEnqueued = false;

            if (capture != null && capture.Depth != null && (btQueueCount < MAX_BODY_QUEUE_LENGTH /**|| rawBodyTimestamp == sensorData.lastBodyFrameTime*/))
            {
                bEnqueued = bodyTracker.EnqueueCapture(capture, btQueueWaitTime);

                if (bEnqueued)
                {
                    //Debug.Log("QueuedBodyTimestamp: " + capture.Depth.DeviceTimestamp.Ticks + ", QueueLen: " + (btQueueCount + 1));

                    lock (bodyCaptureLock)
                    {
                        // queued
                        btQueueCount++;
                        btQueueTime = (ulong)DateTime.Now.Ticks;
                        //Debug.Log("Push btQueueCount: " + btQueueCount);

                        //if (btQueueWaitTime > 0)
                        //    btQueueWaitTime--;
                    }
                }
                else
                {
                    //Debug.LogWarning("Adding capture to BT queue failed! QueueCount: " + btQueueCount);
                    //btQueueWaitTime++;
                }
            }

            return bEnqueued;
        }

        // internal body-tracking polling method (used by the BT-thread)
        private Capture PollBodyFrameInternal(KinectInterop.SensorData sensorData, bool bGetBodyData/**, Capture capture*/)
        {
            Capture btCapture = null;

            if (bodyTracker != null && btQueueCount > 0)
            {
                // check for body frame
                //bool bCheckBodyFrame = (rawColorImage != null && rawColorTimestamp == sensorData.lastColorFrameTime) ||
                //    (rawDepthImage != null && rawDepthTimestamp == sensorData.lastDepthFrameTime) ||
                //    (rawInfraredImage != null && rawInfraredTimestamp == sensorData.lastInfraredFrameTime);
                //IntPtr bodyFrameHandle = bCheckBodyFrame ? bodyTracker.PollBodyFrame(0) : IntPtr.Zero;

                IntPtr bodyFrameHandle = bodyTracker.PollBodyFrame(0);

                if (bodyFrameHandle != IntPtr.Zero)
                {
                    lock (bodyCaptureLock)
                    {
                        if (btQueueCount > 0)
                            btQueueCount--;
                        //Debug.Log("Poll btQueueCount: " + btQueueCount);
                    }

                    lock (bodyTrackerLock)
                    {
                        if(rawBodyIndexImage != null)
                        {
                            Image bodyIndexImage = bodyTracker.GetBodyIndexMap(bodyFrameHandle);
                            if(bGetBodyData)
                                bodyIndexImage.CopyBytesTo(rawBodyIndexImage, 0, 0, rawBodyIndexImage.Length);
                            bodyIndexImage.Dispose();
                        }

                        if(bGetBodyData)
                        {
                            rawBodyTimestamp = bodyTracker.GetBodyFrameTimestampUsec(bodyFrameHandle);
                            trackedBodiesCount = bodyTracker.GetNumberOfBodies(bodyFrameHandle);
                            //Debug.Log("RawBodyTimestamp: " + rawBodyTimestamp + ", QueueLen: " + btQueueCount);

                            // get body tracking capture
                            btCapture = bodyTracker.GetCapture(bodyFrameHandle);

                            // create the needed slots
                            while (alTrackedBodies.Count < trackedBodiesCount)
                            {
                                alTrackedBodies.Add(new KinectInterop.BodyData((int)KinectInterop.JointType.Count));
                            }

                            // get sensor-to-world matrix
                            Matrix4x4 sensorToWorld = GetSensorToWorldMatrix();
                            Quaternion sensorRotInv = GetSensorRotationNotZFlipped(true);
                            float scaleX = sensorData.sensorSpaceScale.x * 0.001f;
                            float scaleY = sensorData.sensorSpaceScale.y * 0.001f;

                            for (int i = 0; i < trackedBodiesCount; i++)
                            {
                                KinectInterop.BodyData bodyData = alTrackedBodies[i];

                                bodyData.liTrackingID = bodyTracker.GetBodyId(bodyFrameHandle, (uint)i);
                                bodyData.iBodyIndex = i;
                                bodyData.bIsTracked = true;

                                bodyTracker.GetBodySkeleton(bodyFrameHandle, (uint)i, ref bodySkeletonData);
                                for (int jBT = 0; jBT < (int)k4abt_joint_type.K4ABT_JOINT_COUNT; jBT++)
                                {
                                    k4abt_joint_t jointBT = bodySkeletonData.joints[jBT];
                                    int j = BtJoint2JointType[jBT];

                                    if (j >= 0)
                                    {
                                        KinectInterop.JointData jointData = bodyData.joint[j];

                                        jointData.trackingState = (KinectInterop.TrackingState)jointBT.confidence_level;  // KinectInterop.TrackingState.Tracked;  // always tracked?

                                        float jPosZ = (bIgnoreZCoordinates && j > 0) ? bodyData.joint[0].kinectPos.z : jointBT.position.Z * 0.001f;
                                        jointData.kinectPos = new Vector3(jointBT.position.X * 0.001f, jointBT.position.Y * 0.001f, jointBT.position.Z * 0.001f);
                                        jointData.position = sensorToWorld.MultiplyPoint3x4(new Vector3(jointBT.position.X * scaleX, jointBT.position.Y * scaleY, jPosZ));

                                        jointData.orientation = new Quaternion(jointBT.orientation.X, jointBT.orientation.Y, jointBT.orientation.Z, jointBT.orientation.W);
                                        jointData.orientation = sensorRotInv * jointData.orientation;

                                        if (j == 0)
                                        {
                                            bodyData.kinectPos = jointData.kinectPos;
                                            bodyData.position = jointData.position;
                                            bodyData.orientation = jointData.orientation;
                                        }

                                        bodyData.joint[j] = jointData;
                                    }
                                }

                                // estimate additional joints
                                CalcBodySpecialJoints(ref bodyData);

                                // calculate bone dirs
                                KinectInterop.CalcBodyJointDirs(ref bodyData);

                                // calculate joint orientations
                                CalcBodyJointOrients(ref bodyData);

                                // body orientation
                                bodyData.normalRotation = bodyData.joint[0].normalRotation;
                                bodyData.mirroredRotation = bodyData.joint[0].mirroredRotation;

                                alTrackedBodies[i] = bodyData;

                                //Debug.Log("  (T)User ID: " + bodyData.liTrackingID + ", body: " + i + ", pos: " + bodyData.kinectPos);
                            }
                        }

                        bodyTracker.ReleaseBodyFrame(bodyFrameHandle);
                        bodyFrameHandle = IntPtr.Zero;
                    }
                }

                //// check for timeout
                //ulong currentTime = (ulong)DateTime.Now.Ticks;
                //if (btQueueCount > 0 && (currentTime - btQueueTime) >= 10000000)  // 1 sec.
                //{
                //    // timeout
                //    //Debug.LogWarning("Timed out waiting for bt-frame to pop! QueueCount: " + btQueueCount);
                //    btQueueTime = currentTime;
                //    //btQueueCount--;
                //}

                //// enqueue capture 
                //if (capture != null && capture.Depth != null && (btQueueCount < MAX_BODY_QUEUE_LENGTH /**|| rawBodyTimestamp == sensorData.lastBodyFrameTime*/))
                //{
                //    if (bodyTracker.EnqueueCapture(capture, btQueueWaitTime))
                //    {
                //        //Debug.Log("QueuedBodyTimestamp: " + capture.Depth.DeviceTimestamp.Ticks + ", QueueLen: " + (btQueueCount + 1));

                //        // queued
                //        btQueueCount++;
                //        btQueueTime = currentTime;

                //        //if (btQueueWaitTime > 0)
                //        //    btQueueWaitTime--;
                //    }
                //    else
                //    {
                //        Debug.LogWarning("Adding capture to BT queue failed! QueueCount: " + btQueueCount);
                //        //btQueueWaitTime++;
                //    }
                //}
            }

            return btCapture;
        }


        private static readonly int[] BtJoint2JointType =
        {
            (int)KinectInterop.JointType.Pelvis,
            (int)KinectInterop.JointType.SpineNaval,
            (int)KinectInterop.JointType.SpineChest,
            (int)KinectInterop.JointType.Neck,

            (int)KinectInterop.JointType.ClavicleLeft,
            (int)KinectInterop.JointType.ShoulderLeft,
            (int)KinectInterop.JointType.ElbowLeft,
            (int)KinectInterop.JointType.WristLeft,

            (int)KinectInterop.JointType.HandLeft,
            (int)KinectInterop.JointType.HandtipLeft,
            (int)KinectInterop.JointType.ThumbLeft,

            (int)KinectInterop.JointType.ClavicleRight,
            (int)KinectInterop.JointType.ShoulderRight,
            (int)KinectInterop.JointType.ElbowRight,
            (int)KinectInterop.JointType.WristRight,

            (int)KinectInterop.JointType.HandRight,
            (int)KinectInterop.JointType.HandtipRight,
            (int)KinectInterop.JointType.ThumbRight,

            (int)KinectInterop.JointType.HipLeft,
            (int)KinectInterop.JointType.KneeLeft,
            (int)KinectInterop.JointType.AnkleLeft,
            (int)KinectInterop.JointType.FootLeft,

            (int)KinectInterop.JointType.HipRight,
            (int)KinectInterop.JointType.KneeRight,
            (int)KinectInterop.JointType.AnkleRight,
            (int)KinectInterop.JointType.FootRight,

            (int)KinectInterop.JointType.Head,

            (int)KinectInterop.JointType.Nose,
            (int)KinectInterop.JointType.EyeLeft,
            (int)KinectInterop.JointType.EarLeft,
            (int)KinectInterop.JointType.EyeRight,
            (int)KinectInterop.JointType.EarRight
        };


        // estimates additional joints for the given body
        protected virtual void CalcBodySpecialJoints(ref KinectInterop.BodyData bodyData)
        {
            //// hand left
            //{
            //    int e = (int)KinectInterop.JointType.ElbowLeft;
            //    int w = (int)KinectInterop.JointType.WristLeft;
            //    int h = (int)KinectInterop.JointType.HandLeft;

            //    KinectInterop.JointData jointData = bodyData.joint[h];
            //    jointData.trackingState = bodyData.joint[w].trackingState;
            //    jointData.orientation = bodyData.joint[w].orientation;

            //    Vector3 posWrist = bodyData.joint[w].kinectPos;
            //    Vector3 posElbow = bodyData.joint[e].kinectPos;
            //    jointData.kinectPos = posWrist + (posWrist - posElbow) * 0.25f;

            //    posWrist = bodyData.joint[w].position;
            //    posElbow = bodyData.joint[e].position;
            //    jointData.position = posWrist + (posWrist - posElbow) * 0.25f;

            //    bodyData.joint[h] = jointData;
            //}

            //// hand right
            //{
            //    int e = (int)KinectInterop.JointType.ElbowRight;
            //    int w = (int)KinectInterop.JointType.WristRight;
            //    int h = (int)KinectInterop.JointType.HandRight;

            //    KinectInterop.JointData jointData = bodyData.joint[h];
            //    jointData.trackingState = bodyData.joint[w].trackingState;
            //    jointData.orientation = bodyData.joint[w].orientation;

            //    Vector3 posWrist = bodyData.joint[w].kinectPos;
            //    Vector3 posElbow = bodyData.joint[e].kinectPos;
            //    jointData.kinectPos = posWrist + (posWrist - posElbow) * 0.25f;

            //    posWrist = bodyData.joint[w].position;
            //    posElbow = bodyData.joint[e].position;
            //    jointData.position = posWrist + (posWrist - posElbow) * 0.25f;

            //    bodyData.joint[h] = jointData;
            //}

        }


        //// calculates all bone directions for the given body
        //protected virtual void CalcBodyJointDirs(ref KinectInterop.BodyData bodyData)
        //{
        //    if (bodyData.bIsTracked)
        //    {
        //        for (int j = 0; j < (int)KinectInterop.JointType.Count; j++)
        //        {
        //            if (j == 0)
        //            {
        //                bodyData.joint[j].direction = Vector3.zero;
        //            }
        //            else
        //            {
        //                int jParent = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)j);

        //                if (bodyData.joint[j].trackingState != KinectInterop.TrackingState.NotTracked &&
        //                    bodyData.joint[jParent].trackingState != KinectInterop.TrackingState.NotTracked)
        //                {
        //                    bodyData.joint[j].direction = (bodyData.joint[j].position - bodyData.joint[jParent].position); //.normalized;
        //                }
        //            }
        //        }
        //    }
        //}

        // calculates all joint orientations for the given body
        protected virtual void CalcBodyJointOrients(ref KinectInterop.BodyData bodyData)
        {
            if (bodyData.bIsTracked)
            {
                for (int j = 0; j < (int)KinectInterop.JointType.Count; j++)
                {
                    if(bodyData.joint[j].trackingState != KinectInterop.TrackingState.NotTracked)
                    {
                        Quaternion jointOrient = bodyData.joint[j].orientation;
                        Quaternion jointOrientNormal = jointOrient * _JointTurnCS[j] * _JointBaseOrient[j];
                        bodyData.joint[j].normalRotation = jointOrientNormal;

                        Vector3 mirroredAngles = jointOrientNormal.eulerAngles;
                        mirroredAngles.y = -mirroredAngles.y;
                        mirroredAngles.z = -mirroredAngles.z;
                        bodyData.joint[j].mirroredRotation = Quaternion.Euler(mirroredAngles);
                    }
                }
            }
        }

        // base orientations
        private static readonly Quaternion[] _JointBaseOrient =
        {
            Quaternion.LookRotation(Vector3.left, Vector3.back),  // Pelvis
            Quaternion.LookRotation(Vector3.left, Vector3.back),
            Quaternion.LookRotation(Vector3.left, Vector3.back),
            Quaternion.LookRotation(Vector3.left, Vector3.back),
            Quaternion.LookRotation(Vector3.left, Vector3.back),

            Quaternion.LookRotation(Vector3.down, Vector3.back),  // ClavicleL
            Quaternion.LookRotation(Vector3.down, Vector3.back),
            Quaternion.LookRotation(Vector3.down, Vector3.back),
            Quaternion.LookRotation(Vector3.down, Vector3.back),
            Quaternion.LookRotation(Vector3.down, Vector3.back),

            Quaternion.LookRotation(Vector3.up, Vector3.forward),  // ClavicleR
            Quaternion.LookRotation(Vector3.up, Vector3.forward),
            Quaternion.LookRotation(Vector3.up, Vector3.forward),
            Quaternion.LookRotation(Vector3.up, Vector3.forward),
            Quaternion.LookRotation(Vector3.up, Vector3.forward),

            Quaternion.LookRotation(Vector3.left, Vector3.back),  // HipL
            Quaternion.LookRotation(Vector3.left, Vector3.back),
            Quaternion.LookRotation(Vector3.left, Vector3.back),
            Quaternion.LookRotation(Vector3.left, Vector3.back),

            Quaternion.LookRotation(Vector3.left, Vector3.forward),  // HipR
            Quaternion.LookRotation(Vector3.left, Vector3.forward),
            Quaternion.LookRotation(Vector3.left, Vector3.forward),
            Quaternion.LookRotation(Vector3.left, Vector3.forward),

            Quaternion.LookRotation(Vector3.left, Vector3.back),  // Nose
            Quaternion.LookRotation(Vector3.left, Vector3.back),
            Quaternion.LookRotation(Vector3.left, Vector3.back),
            Quaternion.LookRotation(Vector3.left, Vector3.back),
            Quaternion.LookRotation(Vector3.left, Vector3.back),

            Quaternion.LookRotation(Vector3.down, Vector3.back),  // FingersL
            Quaternion.LookRotation(Vector3.down, Vector3.back),
            Quaternion.LookRotation(Vector3.up, Vector3.forward),  // FingersR
            Quaternion.LookRotation(Vector3.up, Vector3.forward)
        };

        // turn cs rotations
        private static readonly Quaternion[] _JointTurnCS =
        {
            Quaternion.Euler(0f, 90f, 90f),  // Pelvis
            Quaternion.Euler(0f, 90f, 90f),
            Quaternion.Euler(0f, 90f, 90f),
            Quaternion.Euler(0f, 90f, 90f),
            Quaternion.Euler(0f, 90f, 90f),

            Quaternion.Euler(180f, 0f, 180f),  // ClavicleL
            Quaternion.Euler(180f, 0f, 180f),
            Quaternion.Euler(180f, 0f, 180f),
            Quaternion.Euler(-90f, 0f, 180f),
            Quaternion.Euler(-90f, 0f, 180f),

            Quaternion.Euler(0f, 180f, 0f),  // ClavicleR
            Quaternion.Euler(0f, 180f, 0f),
            Quaternion.Euler(0f, 180f, 0f),
            Quaternion.Euler(-90f, 0f, 180f),
            Quaternion.Euler(-90f, 0f, 180f),

            Quaternion.Euler(0f, 90f, 90f),  // HipL
            Quaternion.Euler(0f, 90f, 90f),
            Quaternion.Euler(0f, 90f, 90f),
            Quaternion.Euler(90f, 0f, 0f),

            Quaternion.Euler(0f, 90f, -90f),  // HipR
            Quaternion.Euler(0f, 90f, -90f),
            Quaternion.Euler(0f, 90f, -90f),
            Quaternion.Euler(90f, 0f, 180f),

            Quaternion.Euler(0f, 90f, 90f),  // Nose
            Quaternion.Euler(90f, 0f, 0f),
            Quaternion.Euler(0f, -90f, -90f),
            Quaternion.Euler(90f, 0f, 0f),
            Quaternion.Euler(0f, 90f, 90f),

            Quaternion.Euler(-90f, 0f, 180f),  // FingersL
            Quaternion.Euler(-90f, 0f, 180f),
            Quaternion.Euler(-90f, 0f, 180f),  // FingersR
            Quaternion.Euler(-90f, 0f, 180f)
        };


        // converts camera intrinsics to calibration-structure
        protected Calibration GetBodyTrackerCalibration(KinectInterop.CameraIntrinsics intr)
        {
            Calibration cal = new Calibration();

            cal.ColorResolution = ColorResolution.Off;
            cal.DepthMode = DepthMode.NFOV_Unbinned;

            CameraCalibration camParams = new CameraCalibration();
            Intrinsics camIntr = new Intrinsics();

            camIntr.ParameterCount = 15;
            camIntr.Parameters = new float[camIntr.ParameterCount];

            camParams.ResolutionWidth = intr.width;  // 640; // 
            camParams.ResolutionHeight = intr.height;  // 576; // 

            // 0        float cx;
            // 1        float cy;
            camIntr.Parameters[0] = intr.ppx;
            camIntr.Parameters[1] = intr.ppy;

            // 2        float fx;            /**< Focal length x */
            // 3        float fy;            /**< Focal length y */
            camIntr.Parameters[2] = intr.fx;
            camIntr.Parameters[3] = intr.fy;

            // 4        float k1;
            // 5        float k2;
            // 6        float k3;
            // 7        float k4;
            // 8        float k5;
            // 9        float k6;
            camIntr.Parameters[4] = intr.distCoeffs.Length >= 1 ? intr.distCoeffs[0] : 0f;
            camIntr.Parameters[5] = intr.distCoeffs.Length >= 2 ? intr.distCoeffs[1] : 0f;
            camIntr.Parameters[6] = intr.distCoeffs.Length >= 3 ? intr.distCoeffs[2] : 0f;
            camIntr.Parameters[7] = intr.distCoeffs.Length >= 4 ? intr.distCoeffs[3] : 0f;
            camIntr.Parameters[8] = intr.distCoeffs.Length >= 5 ? intr.distCoeffs[4] : 0f;
            camIntr.Parameters[9] = intr.distCoeffs.Length >= 6 ? intr.distCoeffs[5] : 0f;

            if (intr.distType == KinectInterop.DistortionType.Theta)
                camIntr.Type = CalibrationModelType.Theta;
            else if (intr.distType == KinectInterop.DistortionType.Polynomial3K)
                camIntr.Type = CalibrationModelType.Polynomial3K;
            else if (intr.distType == KinectInterop.DistortionType.Rational6KT)
                camIntr.Type = CalibrationModelType.Rational6KT;
            else
                camIntr.Type = (CalibrationModelType)intr.distType;

            // 10            float codx;
            // 11            float cody;
            camIntr.Parameters[10] = intr.codx;
            camIntr.Parameters[11] = intr.cody;

            // 12            float p2;
            // 13            float p1;
            camIntr.Parameters[12] = intr.p2;
            camIntr.Parameters[13] = intr.p1;

            // 14           float metric_radius;
            camIntr.Parameters[14] = intr.maxRadius;

            camParams.Intrinsics = camIntr;
            cal.DepthCameraCalibration = camParams;

            return cal;
        }

    }
}
