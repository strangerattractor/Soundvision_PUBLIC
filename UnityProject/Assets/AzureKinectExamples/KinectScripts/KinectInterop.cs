using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace com.rfilkov.kinect
{
    /// <summary>
    /// KinectInterop is a class containing utility and interop functions, to call the proper sensor interface.
    /// </summary>
    public class KinectInterop
    {
        // body tracking sdk's tools folder - used to copy the needed libraries and files.
        // TODO - change it for other OS's or install folders. 
        internal const string BODY_TRACKING_TOOLS_FOLDER = "C:/Program Files/Azure Kinect Body Tracking SDK/tools";


        /// <summary>
        /// Constants used by this class and other K2-components
        /// </summary>
        public static class Constants
        {
            public const int MaxBodyCount = 100;

            public const float MinTimeBetweenSameGestures = 0.0f;
            public const float PoseCompleteDuration = 1.0f;
            public const float ClickMaxDistance = 0.05f;
            public const float ClickStayDuration = 2.0f;
        }


        /// <summary>
        /// Depth sensor platforms.
        /// </summary>
        public enum DepthSensorPlatform : int
        {
            None = 0,
            KinectV1 = 1,
            KinectV2 = 2,
            RealSense = 3,
            Kinect4Azure = 4,

            DummyK2 = 102
        }

        /// <summary>
        /// Device streaming modes.
        /// </summary>
        public enum DeviceStreamingMode
        {
            Disabled = 0,
            ConnectedSensor = 1,
            PlayRecording = 2,
            // CreateRecording = 3
        }

        // Data structures for interfacing C# with the native wrappers

        /// <summary>
        /// Frame-source flags.
        /// </summary>
        [Flags]
        public enum FrameSource : uint
        {
            TypeNone = 0x0,
            TypeColor = 0x1,
            TypeInfrared = 0x2,
            TypeDepth = 0x8,
            TypeBodyIndex = 0x10,
            TypeBody = 0x20,
            TypeAudio = 0x40,
            TypePose = 0x80,

            TypeAll = 0xFF
        }

        /// <summary>
        /// Body joint types (with new joints added)
        /// </summary>
        public enum JointType : int
        {
            Pelvis = 0,
            SpineNaval = 1,
            SpineChest = 2,
            Neck = 3,
            Head = 4,

            ClavicleLeft = 5,
            ShoulderLeft = 6,
            ElbowLeft = 7,
            WristLeft = 8,
            HandLeft = 9,

            ClavicleRight = 10,
            ShoulderRight = 11,
            ElbowRight = 12,
            WristRight = 13,
            HandRight = 14,

            HipLeft = 15,
            KneeLeft = 16,
            AnkleLeft = 17,
            FootLeft = 18,

            HipRight = 19,
            KneeRight = 20,
            AnkleRight = 21,
            FootRight = 22,

            Nose = 23,
            EyeLeft = 24,
            EarLeft = 25,
            EyeRight = 26,
            EarRight = 27,

            HandtipLeft = 28,
            ThumbLeft = 29,
            HandtipRight = 30,
            ThumbRight = 31,

            Count = 32
        }

        ///// <summary>
        ///// Body joint types (with new joints mixed)
        ///// </summary>
        //public enum JointType : int
        //{
        //    Pelvis = 0,
        //    SpineNaval = 1,
        //    SpineChest = 2,
        //    Neck = 3,
        //    Head = 4,

        //    ClavicleLeft = 5,
        //    ShoulderLeft = 6,
        //    ElbowLeft = 7,
        //    WristLeft = 8,

        //    HandLeft = 9,
        //    HandtipLeft = 10,
        //    ThumbLeft = 11,

        //    ClavicleRight = 12,
        //    ShoulderRight = 13,
        //    ElbowRight = 14,
        //    WristRight = 15,

        //    HandRight = 16,
        //    HandtipRight = 17,
        //    ThumbRight = 18,

        //    HipLeft = 19,
        //    KneeLeft = 20,
        //    AnkleLeft = 21,
        //    FootLeft = 22,

        //    HipRight = 23,
        //    KneeRight = 24,
        //    AnkleRight = 25,
        //    FootRight = 26,

        //    Nose = 27,
        //    EyeLeft = 28,
        //    EarLeft = 29,
        //    EyeRight = 30,
        //    EarRight = 31,

        //    Count = 32
        //}

        /// <summary>
        /// Joint tracking state.
        /// </summary>
        public enum TrackingState
        {
            NotTracked = 0,
            Inferred = 1,
            Tracked = 2,
            HighConf = 3  // reserved for future use
        }

        ///// <summary>
        ///// Background-removal blur type
        ///// </summary>
        //public enum BrBlurType : int
        //{
        //    None = 0,
        //    Blur = 1,
        //    Median = 2,
        //}

        /// <summary>
        /// Container for the body-joint data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct JointData
        {
            // parameters filled in by the sensor interface
            public JointType jointType;
            public TrackingState trackingState;

            public Vector3 kinectPos;
            public Vector3 position;
            public Quaternion orientation;

            //public Vector3 posPrev;
            //public Vector3 posRel;
            //public Vector3 posVel;

            // KM calculated parameters
            public Vector3 direction;
            public Quaternion normalRotation;
            public Quaternion mirroredRotation;

            public float lastAngle;  // used by the constraints filter


            public override string ToString()
            {
                return jointType.ToString();
            }

            public void CopyTo(ref JointData toJoint)
            {
                toJoint.jointType = jointType;
                toJoint.trackingState = trackingState;

                toJoint.kinectPos = kinectPos;
                toJoint.position = position;
                toJoint.orientation = orientation;

                //toJoint.posPrev = posPrev;
                //toJoint.posRel = posRel;
                //toJoint.posVel = posVel;

                toJoint.direction = direction;
                toJoint.normalRotation = normalRotation;
                toJoint.mirroredRotation = mirroredRotation;
            }
        }

        /// <summary>
        /// Container for the body data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct BodyData
        {
            // parameters filled in by the sensor interface
            public ulong liTrackingID;
            public int iBodyIndex;
            public bool bIsTracked;

            public Vector3 kinectPos;
            public Vector3 position;
            public Quaternion orientation;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = (int)JointType.Count, ArraySubType = UnmanagedType.Struct)]
            public JointData[] joint;

            // KM calculated parameters
            public Quaternion normalRotation;
            public Quaternion mirroredRotation;

            //public HandState leftHandState;
            //public TrackingConfidence leftHandConfidence;
            //public HandState rightHandState;
            //public TrackingConfidence rightHandConfidence;


            public BodyData(int jointCount)
            {
                liTrackingID = 0;
                iBodyIndex = 0;
                bIsTracked = false;

                kinectPos = Vector3.zero;
                position = Vector3.zero;
                orientation = Quaternion.identity;

                normalRotation = Quaternion.identity;
                mirroredRotation = Quaternion.identity;

                joint = new JointData[jointCount];
                for(int j = 0; j < jointCount; j++)
                {
                    joint[j].jointType = (JointType)j;
                    joint[j].trackingState = TrackingState.NotTracked;
                    joint[j].kinectPos = Vector3.zero;
                    joint[j].position = Vector3.zero;

                    joint[j].orientation = Quaternion.identity;
                    joint[j].normalRotation = Quaternion.identity;
                    joint[j].mirroredRotation = Quaternion.identity;
                }
            }

            public override string ToString()
            {
                return "Body" + iBodyIndex + ": " + liTrackingID;
            }

            public void CopyTo(ref BodyData toBody)
            {
                toBody.liTrackingID = liTrackingID;
                toBody.iBodyIndex = iBodyIndex;
                toBody.bIsTracked = bIsTracked;

                toBody.kinectPos = kinectPos;
                toBody.position = position;
                toBody.orientation = orientation;

                if (toBody.joint == null || toBody.joint.Length != joint.Length)
                {
                    toBody.joint = new JointData[joint.Length];
                }

                for (int j = 0; j < joint.Length; j++)
                {
                    joint[j].CopyTo(ref toBody.joint[j]);
                }

                toBody.normalRotation = normalRotation;
                toBody.mirroredRotation = mirroredRotation;
            }
        }

        /// <summary>
        /// Sensor device info.
        /// </summary>
        public class SensorDeviceInfo
        {
            public string sensorId;
            public string sensorName;

            public FrameSource sensorCaps;
        }

        /// <summary>
        /// Camera distortion type
        /// </summary>
        public enum DistortionType
        {
            None = 0,
            ModifiedBrownConrady = 1,
            InverseBrownConrady = 2,
            Theta = 3,
            BrownConrady = 4,
            Polynomial3K = 5,
            Rational6KT = 6
        }

        /// <summary>
        /// Camera intrinsics
        /// </summary>
        public class CameraIntrinsics
        {
            public int cameraType;  // camera type (specific for the sensor interface)
            public int width;  // Camera image width
            public int height; // Camera image height
            public float ppx;  // Principal point in image, x
            public float ppy;  // Principal point in image, y
            public float fx;   // Focal length x
            public float fy;   // Focal length y
            public DistortionType distType;  // distortion type
            public float[] distCoeffs;  // radial distortion coefficient
            public float codx;  // Center of distortion in Z=1 plane, x (only used for Rational6KT)
            public float cody;  // Center of distortion in Z=1 plane, y (only used for Rational6KT)
            public float p2;    // Tangential distortion coefficient 2
            public float p1;    // Tangential distortion coefficient 1
            public float maxRadius;  // Metric radius

            public float hFOV, vFOV;
        }

        /// <summary>
        /// Camera extrinsics.
        /// </summary>
        public class CameraExtrinsics
        {
            public float[] rotation;
            public float[] translation;
        }

        /// <summary>
        /// Container for the sensor data, including color, depth, ir and body frames.
        /// </summary>
        public class SensorData
        {
            public DepthSensorInterface sensorInterface;
            public DepthSensorPlatform sensorIntPlatform;

            public Thread pollFramesThread = null;
            public AutoResetEvent threadStopEvent = null;

            public CameraIntrinsics depthCamIntr = null;
            public CameraIntrinsics colorCamIntr = null;
            public CameraExtrinsics depth2ColorExtr = null;
            public CameraExtrinsics color2DepthExtr = null;

            public Vector3 colorImageScale = Vector3.one;
            public Vector3 depthImageScale = Vector3.one;
            public Vector3 infraredImageScale = Vector3.one;
            public Vector3 sensorSpaceScale = Vector3.one;

            public int colorImageWidth;
            public int colorImageHeight;

            public TextureFormat colorImageFormat = TextureFormat.RGBA32;
            public int colorImageStride = 4;

            //public byte[] colorImage;
            public Texture colorImageTexture = null;
            public ulong lastColorFrameTime = 0;
            public ulong prevColorFrameTime = 0;

            public int depthImageWidth;
            public int depthImageHeight;

            public ushort[] depthImage;
            public ulong lastDepthFrameTime = 0;
            public ulong prevDepthFrameTime = 0;

            public ushort[] infraredImage;
            public ulong lastInfraredFrameTime = 0;

            public Vector3 sensorPosePosition;
            public Quaternion sensorPoseRotation;
            public ulong lastSensorPoseFrameTime = 0;
            //public bool sensorTransformUpdated = false;

            public byte[] bodyIndexImage;
            public ulong lastBodyIndexFrameTime = 0;

            public uint trackedBodiesCount = 0;
            public BodyData[] alTrackedBodies;
            public ulong lastBodyFrameTime = 0;
            //public int firstUserIndex = 255;

            public int[] depthHistBufferData;
            public int depthHistTotalPoints;
            public ulong lastDepthHistTime = 0;

            public RenderTexture depthImageTexture;
            public Material depthImageMaterial;
            public ComputeBuffer depthImageBuffer;
            public ComputeBuffer depthHistBuffer;
            public ulong lastDepthImageTime = 0;

            public RenderTexture infraredImageTexture;
            public Material infraredImageMaterial;
            public ComputeBuffer infraredImageBuffer;
            public ulong lastInfraredImageTime = 0;

            public int[] bodyHistBufferData;
            public int bodyHistTotalPoints;
            public ulong lastBodyHistTime = 0;

            public RenderTexture bodyImageTexture;
            public Material bodyImageMaterial;
            public ComputeBuffer bodyIndexBuffer;
            public ComputeBuffer bodyHistBuffer;
            public ulong lastBodyImageTime = 0;

//            public Vector3[] depth2SpaceFrame = null;
            public ulong lastDepth2SpaceFrameTime = 0;

//            public Vector2[] depth2ColorFrame = null;
            public ulong lastDepth2ColorFrameTime = 0;

//            public Vector2[] color2DepthFrame = null;
            public ulong lastColor2DepthFrameTime = 0;

            public ComputeBuffer colorDepthBuffer;
            public RenderTexture colorDepthTexture;
            public ulong lastColorDepthBufferTime = 0;
            public ulong usedColorDepthBufferTime = 0;

            public RenderTexture depthTexTexture;
            public Material depthTexMaterial;
            public ulong lastDepthTexTime = 0;

            public RenderTexture infraredTexTexture;
            public Material infraredTexMaterial;
            public ulong lastInfraredTexTime = 0;
        }



        // graphics shader level
        private static int graphicsShaderLevel = 0;


        // sets the graphics shader level
        public static void SetGraphicsShaderLevel(int shaderLevel)
        {
            graphicsShaderLevel = shaderLevel;
        }


        // checks if DirectX11/Direct3D-11 is turned on or not
        public static bool IsDirectX11Available()
        {
            return (graphicsShaderLevel >= 50);
        }


        // returns true if the project is running on 64-bit architecture, false if 32-bit
        public static bool Is64bitArchitecture()
        {
            int sizeOfPtr = Marshal.SizeOf(typeof(IntPtr));
            return (sizeOfPtr > 4);
        }


        // copy resource asset to the target file
        public static bool CopyResourceFile(string resFileName, string targetFilePath)
        {
            TextAsset textRes = Resources.Load(resFileName, typeof(TextAsset)) as TextAsset;
            if (textRes == null)
            {
                Debug.LogWarning("Resource not found: " + resFileName);
                return false;
            }

            FileInfo targetFile = new FileInfo(targetFilePath);
            if (!targetFile.Directory.Exists)
            {
                targetFile.Directory.Create();
            }

            if (!targetFile.Exists || targetFile.Length != textRes.bytes.Length)
            {
                Debug.Log("Copying '" + resFileName + "' to: " + targetFilePath);

                using (Stream resStream = new MemoryStream(textRes.bytes))
                {
                    BinaryReader resReader = new BinaryReader(resStream);
                    byte[] buffer = new byte[32768]; //set the size of your buffer (chunk)

                    using (FileStream fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        while (true) //loop to the end of the file
                        {
                            int read = resReader.Read(buffer, 0, buffer.Length);
                            if (read <= 0) //check for end of file
                                break;

                            fileStream.Write(buffer, 0, read);
                        }
                    }
                }

                bool bFileCopied = File.Exists(targetFilePath);

                return bFileCopied;
            }

            return true;
        }


        // checks if the file exists
        public static bool IsFolderExist(string folderPath)
        {
            return Directory.Exists(folderPath);
        }


        // checks if the file exists
        public static bool IsFileExist(string filePath)
        {
            return File.Exists(filePath);
        }


        // copy file from the source folder to the target folder
        public static bool CopyFolderFile(string sourceFolderPath, string fileName, string targetFolderPath)
        {
            //if(!Directory.Exists(sourceFolderPath))
            //{
            //    //Debug.Log("Folder not found: " + folderPath);
            //    return false;
            //}

            if(!string.IsNullOrEmpty(sourceFolderPath) && sourceFolderPath[sourceFolderPath.Length - 1] != '/' && sourceFolderPath[sourceFolderPath.Length - 1] != '\\')
            {
                sourceFolderPath += "/";
            }

            string sourceFilePath = sourceFolderPath + fileName;
            FileInfo sourceFile = new FileInfo(sourceFilePath);

            if (!sourceFile.Exists)
            {
                Debug.LogWarning("File not found: " + sourceFilePath + ". Trying to find it in resources...");
                return false;
            }

            if (!string.IsNullOrEmpty(targetFolderPath) && sourceFolderPath[targetFolderPath.Length - 1] != '/' && sourceFolderPath[targetFolderPath.Length - 1] != '\\')
            {
                targetFolderPath += "/";
            }

            string targetFilePath = targetFolderPath + fileName;
            FileInfo targetFile = new FileInfo(targetFilePath);

            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }

            if (!targetFile.Exists || targetFile.Length != sourceFile.Length || targetFile.LastWriteTime != sourceFile.LastWriteTime)
            {
                Debug.Log("Copying '" + fileName + "' from " + sourceFolderPath + " to '" + targetFolderPath + "'");
                File.Copy(sourceFilePath, targetFilePath, true);

                bool bFileCopied = File.Exists(targetFilePath);

                return bFileCopied;
            }

            return true;
        }


        // creates new render texture with the given dimensions and format
        public static RenderTexture CreateRenderTexture(RenderTexture currentTex, int width, int height, RenderTextureFormat texFormat = RenderTextureFormat.Default)
        {
            if(currentTex != null)
            {
                currentTex.Release();
                //UnityEngine.Object.Destroy(currentTex);
            }

            RenderTexture renderTex = new RenderTexture(width, height, 0, texFormat);
            renderTex.wrapMode = TextureWrapMode.Clamp;
            renderTex.filterMode = FilterMode.Point;
            renderTex.enableRandomWrite = true;

            return renderTex;
        }


        // creates new compute buffer with the given length and stride
        public static ComputeBuffer CreateComputeBuffer(ComputeBuffer currentBuf, int bufLen, int bufStride)
        {
            if(currentBuf != null)
            {
                currentBuf.Release();
                currentBuf.Dispose();
            }

            ComputeBuffer computeBuf = new ComputeBuffer(bufLen, bufStride);
            return computeBuf;
        }


        // initializes the secondary sensor data, after sensor initialization
        public static void InitSensorData(SensorData sensorData, KinectManager kinectManager)
        {
            // init depth texture
            if (IsDirectX11Available() && sensorData.depthImage != null && 
                kinectManager.getDepthFrames == KinectManager.DepthTextureType.DepthTexture)
            {
                Shader depthImageShader = Shader.Find("Kinect/DepthHistImageShader");
                if (depthImageShader != null)
                {
                    if (sensorData.depthImageTexture == null || sensorData.depthImageTexture.width != sensorData.depthImageWidth || sensorData.depthImageTexture.height != sensorData.depthImageHeight)
                    {
                        sensorData.depthImageTexture = CreateRenderTexture(sensorData.depthImageTexture, sensorData.depthImageWidth, sensorData.depthImageHeight);
                    }

                    sensorData.depthImageMaterial = new Material(depthImageShader);

                    if(sensorData.depthImageBuffer == null)
                    {
                        int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                        sensorData.depthImageBuffer = CreateComputeBuffer(sensorData.depthImageBuffer, depthBufferLength, sizeof(uint));
                    }

                    if(sensorData.depthHistBuffer == null)
                    {
                        sensorData.depthHistBuffer = CreateComputeBuffer(sensorData.depthHistBuffer, DepthSensorBase.MAX_DEPTH_DISTANCE_MM + 1, sizeof(int));
                    }
                }
            }

            // init infrared texture
            if (IsDirectX11Available() && sensorData.infraredImage != null &&
                kinectManager.getInfraredFrames == KinectManager.InfraredTextureType.InfraredTexture)
            {
                Shader infraredImageShader = Shader.Find("Kinect/InfraredImageShader");
                if (infraredImageShader != null)
                {
                    if (sensorData.infraredImageTexture == null || sensorData.infraredImageTexture.width != sensorData.depthImageWidth || sensorData.infraredImageTexture.height != sensorData.depthImageHeight)
                    {
                        sensorData.infraredImageTexture = CreateRenderTexture(sensorData.infraredImageTexture, sensorData.depthImageWidth, sensorData.depthImageHeight);
                    }

                    sensorData.infraredImageMaterial = new Material(infraredImageShader);

                    if (sensorData.infraredImageBuffer == null)
                    {
                        int infraredBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                        sensorData.infraredImageBuffer = CreateComputeBuffer(sensorData.infraredImageBuffer, infraredBufferLength, sizeof(uint));
                    }
                }
            }

            // init body texture
            if (IsDirectX11Available() && sensorData.bodyIndexImage != null && 
                (kinectManager.getBodyFrames == KinectManager.BodyTextureType.BodyTexture || 
                kinectManager.getBodyFrames == KinectManager.BodyTextureType.UserTexture))
            {
                Shader bodyImageShader = null;

                switch(kinectManager.getBodyFrames)
                {
                    case KinectManager.BodyTextureType.BodyTexture:
                        bodyImageShader = Shader.Find("Kinect/UserBodyImageShader");
                        break;

                    case KinectManager.BodyTextureType.UserTexture:
                        bodyImageShader = Shader.Find("Kinect/UserHistImageShader");
                        break;
                }

                if (bodyImageShader != null)
                {
                    if (sensorData.bodyImageTexture == null || sensorData.bodyImageTexture.width != sensorData.depthImageWidth || sensorData.bodyImageTexture.height != sensorData.depthImageHeight)
                    {
                        sensorData.bodyImageTexture = CreateRenderTexture(sensorData.bodyImageTexture, sensorData.depthImageWidth, sensorData.depthImageHeight);
                    }

                    sensorData.bodyImageMaterial = new Material(bodyImageShader);

                    if (sensorData.bodyIndexBuffer == null)
                    {
                        int bodyIndexBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 4;
                        sensorData.bodyIndexBuffer = CreateComputeBuffer(sensorData.bodyIndexBuffer, bodyIndexBufferLength, sizeof(uint));
                    }

                    if (sensorData.depthImageBuffer == null)
                    {
                        int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                        sensorData.depthImageBuffer = CreateComputeBuffer(sensorData.depthImageBuffer, depthBufferLength, sizeof(uint));
                    }

                    if (sensorData.bodyHistBuffer == null)
                    {
                        sensorData.bodyHistBuffer = CreateComputeBuffer(sensorData.bodyHistBuffer, DepthSensorBase.MAX_DEPTH_DISTANCE_MM + 1, sizeof(int));
                    }
                }
            }

            // invoke the sensor interface to init its proprietary data
            if (sensorData.sensorInterface != null)
            {
                sensorData.sensorInterface.InitSensorData(sensorData, kinectManager);
            }
        }


        // closes the sensor and releases the related buffers
        public static void CloseSensor(SensorData sensorData)
        {
            if (sensorData == null)
                return;

            //FinishBackgroundRemoval(sensorData);

            if (sensorData.sensorInterface != null)
            {
                sensorData.sensorInterface.CloseSensor(sensorData);
            }

            if (sensorData.depthImageTexture)
            {
                sensorData.depthImageTexture.Release();
                sensorData.depthImageTexture = null;
            }

            if (sensorData.depthImageBuffer != null)
            {
                sensorData.depthImageBuffer.Release();
                sensorData.depthImageBuffer.Dispose();
                sensorData.depthImageBuffer = null;
            }

            if (sensorData.depthHistBuffer != null)
            {
                sensorData.depthHistBuffer.Release();
                sensorData.depthHistBuffer.Dispose();
                sensorData.depthHistBuffer = null;
            }

            if (sensorData.infraredImageTexture)
            {
                sensorData.infraredImageTexture.Release();
                sensorData.infraredImageTexture = null;
            }

            if (sensorData.infraredImageBuffer != null)
            {
                sensorData.infraredImageBuffer.Release();
                sensorData.infraredImageBuffer.Dispose();
                sensorData.infraredImageBuffer = null;
            }

            if (sensorData.bodyImageTexture)
            {
                sensorData.bodyImageTexture.Release();
                sensorData.bodyImageTexture = null;
            }

            if (sensorData.bodyIndexBuffer != null)
            {
                sensorData.bodyIndexBuffer.Release();
                sensorData.bodyIndexBuffer.Dispose();
                sensorData.bodyIndexBuffer = null;
            }

            if (sensorData.bodyHistBuffer != null)
            {
                sensorData.bodyHistBuffer.Release();
                sensorData.bodyHistBuffer.Dispose();
                sensorData.bodyHistBuffer = null;
            }
        }


        // infoked by the sensor thread to poll for frames
        public static void PollSensorFrames(SensorData sensorData)
        {
            if (sensorData != null && sensorData.sensorInterface != null)
            {
                sensorData.sensorInterface.PollSensorFrames(sensorData);
                sensorData.sensorInterface.PollCoordTransformFrames(sensorData);
                sensorData.sensorInterface.ProcessSensorDataInThread(sensorData);
            }
        }


        // invoked periodically to update sensor data, if needed
        public static bool UpdateSensorData(SensorData sensorData, KinectManager kinectManager, bool isPlayMode)
        {
            bool bResult = false;

            if (sensorData != null && sensorData.sensorInterface != null)
            {
                sensorData.prevDepthFrameTime = sensorData.lastDepthFrameTime;
                sensorData.prevColorFrameTime = sensorData.lastColorFrameTime;

                bResult = sensorData.sensorInterface.UpdateSensorData(sensorData, kinectManager, isPlayMode);

                if(sensorData.lastDepthFrameTime != sensorData.prevDepthFrameTime && 
                    sensorData.lastColorFrameTime != sensorData.prevColorFrameTime && !isPlayMode)
                {
                    sensorData.sensorInterface.UpdateTransformedFrameTextures(sensorData, kinectManager);
                }
            }

            return bResult;
        }


        // invoked periodically to update sensor textures, as needed
        public static bool UpdateSensorTextures(SensorData sensorData, KinectManager kinectManager)
        {
            bool bResult = false;

            if (sensorData != null && sensorData.sensorInterface != null)
            {
                bResult = sensorData.sensorInterface.UpdateSensorTextures(sensorData, kinectManager, sensorData.prevDepthFrameTime);
            }

            return bResult;
        }


        // displays the given texture on the screen
        public static void DisplayGuiTexture(int displayIndex, float screenWidthPercent, Vector2 imageScale, Texture imageTex)
        {
            if (imageTex == null || imageTex.width == 0 || imageTex.height == 0)
                return;

            // get the screen width & height
            float screenW = (float)Screen.width;
            float screenH = (float)Screen.height;

            float rectWidthPercent = screenWidthPercent;
            float rectHeightPercent = rectWidthPercent * (float)imageTex.height / (float)imageTex.width;

            float rectWidth = screenW * rectWidthPercent;
            float rectHeight = screenW * rectHeightPercent;

            float rectX = screenW - (displayIndex + 1) * rectWidth;
            float rectY = screenH - rectHeight;

            if (rectX < 0 || rectY < 0)
                return;

            if (imageScale.x < 0)
            {
                rectX = screenW - displayIndex * rectWidth;
                rectWidth = -rectWidth;
            }
            if (imageScale.y < 0)
            {
                rectY = screenH;
                rectHeight = -rectHeight;
            }

            Rect imageRect = new Rect(rectX, rectY, rectWidth, rectHeight);

            if (imageTex != null)
            {
                GUI.DrawTexture(imageRect, imageTex);
            }
        }


        // returns the respective sensor-to-world matrix
        public static Matrix4x4 GetSensorToWorldMatrix(SensorData sensorData)
        {
            if (sensorData.sensorInterface != null)
            {
                return sensorData.sensorInterface.GetSensorToWorldMatrix();
            }

            return Matrix4x4.identity;
        }


        // returns sensor transform. Please note transform updates depend on the getPoseFrames-KM setting.
        public static Transform GetSensorTransform(SensorData sensorData)
        {
            if (sensorData.sensorInterface != null)
            {
                return sensorData.sensorInterface.GetSensorTransform();
            }

            return null;
        }


        // returns depth camera space coordinates for the given depth-image point
        public static Vector3 MapDepthPointToSpaceCoords(SensorData sensorData, Vector2 depthPos, ushort depthVal)
        {
            Vector3 vPoint = Vector3.zero;

            if (sensorData.sensorInterface != null)
            {
                vPoint = sensorData.sensorInterface.MapDepthPointToSpaceCoords(sensorData, depthPos, depthVal);
            }

            return vPoint;
        }


        // returns depth image coordinates for the given depth camera space point
        public static Vector2 MapSpacePointToDepthCoords(SensorData sensorData, Vector3 spacePos)
        {
            Vector2 vPoint = Vector2.zero;

            if (sensorData.sensorInterface != null)
            {
                vPoint = sensorData.sensorInterface.MapSpacePointToDepthCoords(sensorData, spacePos);
            }

            return vPoint;
        }


        // returns color camera space coordinates for the given color-image point
        public static Vector3 MapColorPointToSpaceCoords(SensorData sensorData, Vector2 colorPos, ushort distance)
        {
            Vector3 vPoint = Vector3.zero;

            if (sensorData.sensorInterface != null)
            {
                vPoint = sensorData.sensorInterface.MapColorPointToSpaceCoords(sensorData, colorPos, distance);
            }

            return vPoint;
        }


        // returns color image coordinates for the given color camera space point
        public static Vector2 MapSpacePointToColorCoords(SensorData sensorData, Vector3 spacePos)
        {
            Vector2 vPoint = Vector2.zero;

            if (sensorData.sensorInterface != null)
            {
                vPoint = sensorData.sensorInterface.MapSpacePointToColorCoords(sensorData, spacePos);
            }

            return vPoint;
        }


        // returns color-image coordinates for the given depth-image point
        public static Vector2 MapDepthPointToColorCoords(SensorData sensorData, Vector2 depthPos, ushort depthVal)
        {
            Vector2 vPoint = Vector2.zero;

            if (sensorData.sensorInterface != null)
            {
                vPoint = sensorData.sensorInterface.MapDepthPointToColorCoords(sensorData, depthPos, depthVal);
            }

            return vPoint;
        }


        // returns depth-image coordinates for the given color-image point
        public static Vector2 MapColorPointToDepthCoords(SensorData sensorData, Vector2 colorPos)
        {
            Vector2 vPoint = Vector2.zero;

            if (sensorData.sensorInterface != null)
            {
                vPoint = sensorData.sensorInterface.MapColorPointToDepthCoords(sensorData, colorPos);
            }

            return vPoint;
        }


        //// estimates depth-map coordinates for the given color coords
        //public static Vector2 MapColorPointToDepthCoords(SensorData sensorData, Vector2 colorPos, bool bReadDepthCoordsIfNeeded)
        //{
        //    Vector2 vPoint = Vector2.zero;

        //    if (sensorData.sensorInterface != null && !float.IsInfinity(colorPos.x) && !float.IsInfinity(colorPos.y))
        //    {
        //        int cIndex = (int)colorPos.y * sensorData.colorImageWidth + (int)colorPos.x;

        //        //if (sensorData.color2DepthFrame != null)
        //        //{
        //        //    if (cIndex >= 0 && cIndex < sensorData.color2DepthFrame.Length)
        //        //    {
        //        //        vPoint = sensorData.color2DepthFrame[cIndex];
        //        //    }
        //        //}
        //        //else 
        //        if (bReadDepthCoordsIfNeeded)
        //        {
        //            Vector2[] vDepthCoords = new Vector2[sensorData.colorImageWidth * sensorData.colorImageHeight];

        //            if (MapColorFrameToDepthCoords(sensorData, ref vDepthCoords))
        //            {
        //                if (cIndex >= 0 && cIndex < vDepthCoords.Length)
        //                {
        //                    vPoint = vDepthCoords[cIndex];
        //                }
        //            }

        //            vDepthCoords = null;
        //        }
        //    }

        //    return vPoint;
        //}


        //// estimates space coordinates for the current depth frame
        //public static bool MapDepthFrameToSpaceCoords(SensorData sensorData, ref Vector3[] vSpaceCoords)
        //{
        //    bool bResult = false;

        //    if (sensorData.sensorInterface != null)
        //    {
        //        bResult = sensorData.sensorInterface.MapDepthFrameToSpaceCoords(sensorData, ref vSpaceCoords);
        //    }

        //    return bResult;
        //}

        //// estimates color-map coordinates for the current depth frame
        //public static bool MapDepthFrameToColorCoords(SensorData sensorData, ref Vector2[] vColorCoords)
        //{
        //    bool bResult = false;

        //    if (sensorData.sensorInterface != null)
        //    {
        //        bResult = sensorData.sensorInterface.MapDepthFrameToColorCoords(sensorData, ref vColorCoords);
        //    }

        //    return bResult;
        //}

        //// estimates depth-map coordinates for the current color frame
        //public static bool MapColorFrameToDepthCoords(SensorData sensorData, ref Vector2[] vDepthCoords)
        //{
        //    bool bResult = false;

        //    if (sensorData.sensorInterface != null)
        //    {
        //        bResult = sensorData.sensorInterface.MapColorFrameToDepthCoords(sensorData, ref vDepthCoords);
        //    }

        //    return bResult;
        //}


        private static readonly Dictionary<JointType, JointType> dictParentJoint = new Dictionary<KinectInterop.JointType, JointType>
        {
            {JointType.Pelvis, JointType.Pelvis},
            {JointType.SpineNaval, JointType.Pelvis},
            {JointType.SpineChest, JointType.SpineNaval},
            {JointType.Neck, JointType.SpineChest},
            {JointType.Head, JointType.Neck},

            {JointType.ClavicleLeft, JointType.SpineChest},
            {JointType.ShoulderLeft, JointType.ClavicleLeft},
            {JointType.ElbowLeft, JointType.ShoulderLeft},
            {JointType.WristLeft, JointType.ElbowLeft},

            {JointType.HandLeft, JointType.WristLeft},
            {JointType.HandtipLeft, JointType.HandLeft},
            {JointType.ThumbLeft, JointType.WristLeft},

            {JointType.ClavicleRight, JointType.SpineChest},
            {JointType.ShoulderRight, JointType.ClavicleRight},
            {JointType.ElbowRight, JointType.ShoulderRight},
            {JointType.WristRight, JointType.ElbowRight},

            {JointType.HandRight, JointType.WristRight},
            {JointType.HandtipRight, JointType.HandRight},
            {JointType.ThumbRight, JointType.WristRight},

            {JointType.HipLeft, JointType.Pelvis},
            {JointType.KneeLeft, JointType.HipLeft},
            {JointType.AnkleLeft, JointType.KneeLeft},
            {JointType.FootLeft, JointType.AnkleLeft},

            {JointType.HipRight, JointType.Pelvis},
            {JointType.KneeRight, JointType.HipRight},
            {JointType.AnkleRight, JointType.KneeRight},
            {JointType.FootRight, JointType.AnkleRight},

            {JointType.Nose, JointType.Head},
            {JointType.EyeLeft, JointType.Nose},
            {JointType.EarLeft, JointType.EyeLeft},
            {JointType.EyeRight, JointType.Nose},
            {JointType.EarRight, JointType.EyeRight},
        };


        // returns the parent joint of the given joint
        public static JointType GetParentJoint(JointType joint)
        {
            return dictParentJoint[joint];
        }

        private static readonly Dictionary<JointType, JointType> dictNextJoint = new Dictionary<KinectInterop.JointType, JointType>
        {
            {JointType.Pelvis, JointType.SpineNaval},
            {JointType.SpineNaval, JointType.SpineChest},
            {JointType.SpineChest, JointType.Neck},
            {JointType.Neck, JointType.Head},
            {JointType.Head, JointType.Head},

            {JointType.ClavicleLeft, JointType.ShoulderLeft},
            {JointType.ShoulderLeft, JointType.ElbowLeft},
            {JointType.ElbowLeft, JointType.WristLeft},
            {JointType.WristLeft, JointType.HandLeft},

            {JointType.HandLeft, JointType.HandtipLeft},
            {JointType.HandtipLeft, JointType.HandtipLeft},
            {JointType.ThumbLeft, JointType.ThumbLeft},

            {JointType.ClavicleRight, JointType.ShoulderRight},
            {JointType.ShoulderRight, JointType.ElbowRight},
            {JointType.ElbowRight, JointType.WristRight},
            {JointType.WristRight, JointType.HandRight},

            {JointType.HandRight, JointType.HandtipRight},
            {JointType.HandtipRight, JointType.HandtipRight},
            {JointType.ThumbRight, JointType.ThumbRight},

            {JointType.HipLeft, JointType.KneeLeft},
            {JointType.KneeLeft, JointType.AnkleLeft},
            {JointType.AnkleLeft, JointType.FootLeft},
            {JointType.FootLeft, JointType.FootLeft},

            {JointType.HipRight, JointType.KneeRight},
            {JointType.KneeRight, JointType.AnkleRight},
            {JointType.AnkleRight, JointType.FootRight},
            {JointType.FootRight, JointType.FootRight},

            {JointType.Nose, JointType.Nose},
            {JointType.EyeLeft, JointType.EarLeft},
            {JointType.EarLeft, JointType.EarLeft},
            {JointType.EyeRight, JointType.EarRight},
            {JointType.EarRight, JointType.EarRight},
        };


        // returns the next joint in the hierarchy, as to the given joint
        public static JointType GetNextJoint(JointType joint)
        {
            return dictNextJoint[joint];
        }

        private static readonly Dictionary<JointType, JointType> dictMirrorJoint = new Dictionary<KinectInterop.JointType, JointType>
        {
            {JointType.Pelvis, JointType.Pelvis},
            {JointType.SpineNaval, JointType.SpineNaval},
            {JointType.SpineChest, JointType.SpineChest},
            {JointType.Neck, JointType.Neck},
            {JointType.Head, JointType.Head},

            {JointType.ClavicleLeft, JointType.ClavicleRight},
            {JointType.ShoulderLeft, JointType.ShoulderRight},
            {JointType.ElbowLeft, JointType.ElbowRight},
            {JointType.WristLeft, JointType.WristRight},

            {JointType.HandLeft, JointType.HandRight},
            {JointType.HandtipLeft, JointType.HandtipRight},
            {JointType.ThumbLeft, JointType.ThumbRight},

            {JointType.ClavicleRight, JointType.ClavicleLeft},
            {JointType.ShoulderRight, JointType.ShoulderLeft},
            {JointType.ElbowRight, JointType.ElbowLeft},
            {JointType.WristRight, JointType.WristLeft},

            {JointType.HandRight, JointType.HandLeft},
            {JointType.HandtipRight, JointType.HandtipLeft},
            {JointType.ThumbRight, JointType.ThumbLeft},

            {JointType.HipLeft, JointType.HipRight},
            {JointType.KneeLeft, JointType.KneeRight},
            {JointType.AnkleLeft, JointType.AnkleRight},
            {JointType.FootLeft, JointType.FootRight},

            {JointType.HipRight, JointType.HipLeft},
            {JointType.KneeRight, JointType.KneeLeft},
            {JointType.AnkleRight, JointType.AnkleLeft},
            {JointType.FootRight, JointType.FootLeft},

            {JointType.Nose, JointType.Nose},
            {JointType.EyeLeft, JointType.EyeRight},
            {JointType.EarLeft, JointType.EarRight},
            {JointType.EyeRight, JointType.EyeLeft},
            {JointType.EarRight, JointType.EarLeft},
        };


        // returns the mirror joint of the given joint
        public static JointType GetMirrorJoint(JointType joint)
        {
            return dictMirrorJoint[joint];
        }


        // memcpy declaration
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr MemCpyDelegate(IntPtr dest, IntPtr src, int count);

        internal static readonly MemCpyDelegate Memcpy = MemCpy.GetMethod();

        [DllImport("libc", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        internal static extern IntPtr unix_memcpy(IntPtr dest, IntPtr src, int count);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        internal static extern IntPtr win_memcpy(IntPtr dest, IntPtr src, int count);

        [System.Security.SuppressUnmanagedCodeSecurity]
        internal static class MemCpy
        {
            internal static MemCpyDelegate GetMethod()
            {
                switch (System.Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return win_memcpy;
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        return unix_memcpy;
                    default:
                        throw new PlatformNotSupportedException(System.Environment.OSVersion.ToString());
                }
            }
        }


        // copies the given number of bytes from source to destination
        public static void CopyBytes(IntPtr srcPtr, IntPtr dstPtr, int byteCount)
        {
            if (srcPtr == IntPtr.Zero)
                throw new Exception("srcPtr should not be null.");
            if (dstPtr == IntPtr.Zero)
                throw new Exception("dstPtr should not be null.");
            if (byteCount == 0)
                throw new Exception("byteCount (" + byteCount + ") should be a positive number.");

            Memcpy(dstPtr, srcPtr, byteCount);
        }


        // copies the source array to the destination array
        public static void CopyBytes(Array srcArray, int copyElemCount, int srcElemSize, IntPtr dstPtr, int dstBlockSize)
        {
            if (srcArray == null)
                throw new Exception("srcArray should not be null.");
            if (dstPtr == IntPtr.Zero)
                throw new Exception("dstPtr should not be null.");

            int copyBytesCount = copyElemCount * srcElemSize;
            if (copyBytesCount > dstBlockSize)
                throw new Exception("Copied " + copyBytesCount + " bytes will overflow the destination memory. MemBlock size: " + dstBlockSize + " bytes");

            if (copyBytesCount > 0)
            {
                var pSrcData = GCHandle.Alloc(srcArray, GCHandleType.Pinned);
                Memcpy(dstPtr, pSrcData.AddrOfPinnedObject(), copyBytesCount);
                pSrcData.Free();
            }
        }


        // copies the source array to the destination array
        public static void CopyBytes(IntPtr srcPtr, int srcBlockSize, Array dstArray, int copyElemCount, int dstElemSize)
        {
            if (srcPtr == IntPtr.Zero)
                throw new Exception("srcPtr should not be null.");
            if (dstArray == null)
                throw new Exception("dstArray should not be null.");

            int copyBytesCount = copyElemCount * dstElemSize;
            if (srcBlockSize < copyBytesCount)
                throw new Exception("Copied " + copyBytesCount + " bytes are not available in the source memory. MemBlock size: " + srcBlockSize + " bytes");

            if (copyBytesCount > 0)
            {
                var pDstData = GCHandle.Alloc(dstArray, GCHandleType.Pinned);
                Memcpy(pDstData.AddrOfPinnedObject(), srcPtr, copyBytesCount);
                pDstData.Free();
            }
        }


        // copies the source array to the destination array
        public static void CopyBytes(Array srcArray, int srcElemSize, Array dstArray, int dstElemSize)
        {
            if (srcArray == null)
                throw new Exception("srcArray should not be null.");
            if (dstArray == null)
                throw new Exception("dstArray should not be null.");
            if ((srcArray.Length * srcElemSize) != (dstArray.Length * dstElemSize))
                throw new Exception("srcArray and dstArray should have the same byte length.");

            int copyBytesCount = dstArray.Length * dstElemSize;
            if (copyBytesCount > 0)
            {
                var pSrcData = GCHandle.Alloc(srcArray, GCHandleType.Pinned);
                var pDstData = GCHandle.Alloc(dstArray, GCHandleType.Pinned);
                Memcpy(pDstData.AddrOfPinnedObject(), pSrcData.AddrOfPinnedObject(), copyBytesCount);
                pDstData.Free();
                pSrcData.Free();
            }
        }


        //// copies the source array to the destination array
        //public static void CopyBytes<T>(T srcStruct, ref T dstStruct)
        //{
        //    if (srcStruct == null)
        //        throw new Exception("srcStruct should not be null.");
        //    if (dstStruct == null)
        //        throw new Exception("dstStruct should not be null.");

        //    int copyBytesCount = Marshal.SizeOf(typeof(T));
        //    Debug.Log("Copy " + copyBytesCount + " bytes");

        //    if (copyBytesCount > 0)
        //    {
        //        var pSrcData = GCHandle.Alloc(srcStruct, GCHandleType.Pinned);
        //        var pDstData = GCHandle.Alloc(dstStruct, GCHandleType.Pinned);
        //        CopyBytes(pSrcData.AddrOfPinnedObject(), pDstData.AddrOfPinnedObject(), copyBytesCount);
        //        pDstData.Free();
        //        pSrcData.Free();
        //    }
        //}


        // borrowed from @keijiro
        private static MethodInfo _setNativeDataMethod;
        private static object[] _setNativeDataArgs = new object[5];

        // sets compute buffer data
        public static void SetComputeBufferData(ComputeBuffer computeBuffer, IntPtr dataPointer, int elemCount, int elemSize)
        {
            if (_setNativeDataMethod == null)
            {
                _setNativeDataMethod = typeof(ComputeBuffer).GetMethod("InternalSetNativeData",
                    BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            _setNativeDataArgs[0] = dataPointer;
            _setNativeDataArgs[1] = 0;      // source offset
            _setNativeDataArgs[2] = 0;      // buffer offset
            _setNativeDataArgs[3] = elemCount;
            _setNativeDataArgs[4] = elemSize;

            _setNativeDataMethod.Invoke(computeBuffer, _setNativeDataArgs);
        }


        // sets compute buffer data
        public static void SetComputeBufferData(ComputeBuffer computeBuffer, Array data, int elemCount, int elemSize)
        {
            var pData = GCHandle.Alloc(data, GCHandleType.Pinned);
            SetComputeBufferData(computeBuffer, pData.AddrOfPinnedObject(), elemCount, elemSize);
            pData.Free();
        }


        private static int[] _csIntArgs2 = new int[2];

        // sets compute shader int2 params
        public static void SetComputeShaderInt2(ComputeShader computeShader, string name, int x, int y)
        {
            _csIntArgs2[0] = x;
            _csIntArgs2[1] = y;
            computeShader.SetInts(name, _csIntArgs2);
        }


        private static float[] _shFloatArgs2 = new float[2];

        // sets compute shader float2 params
        public static void SetComputeShaderFloat2(ComputeShader computeShader, string name, float x, float y)
        {
            _shFloatArgs2[0] = x;
            _shFloatArgs2[1] = y;
            computeShader.SetFloats(name, _shFloatArgs2);
        }


        // sets compute shader float2 params
        public static void SetMatShaderFloat2(Material mat, string name, Vector2 vFloat2)
        {
            mat.SetVector(name, vFloat2);
        }


        // sets compute shader float2 params
        public static void SetMatShaderFloat2(Material mat, string name, float x, float y)
        {
            Vector2 vFloat2 = new Vector2(x, y);
            mat.SetVector(name, vFloat2);
        }


        // reads render texture contents into tex2d (it must have the same width and height).
        public static bool RenderTex2Tex2D(RenderTexture rt, ref Texture2D tex)
        {
            if (!rt || !tex || rt.width != tex.width || rt.height != tex.height)
                return false;

            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = rt;

            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = currentActiveRT;

            return true;
        }


        // reads render texture contents into tex2d (it must have the same width and height).
        public static bool RenderTex2Tex2D(RenderTexture rt, int rtX, int rtY, int rtW, int rtH, ref Texture2D tex)
        {
            if (!rt || !tex || rtW != tex.width || rtH != tex.height)
                return false;

            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = rt;

            tex.ReadPixels(new Rect(rtX, rtY, rtW, rtH), 0, 0);
            tex.Apply();

            RenderTexture.active = currentActiveRT;

            return true;
        }


        private static Material matRender = null;

        // sets up the render material, if needed
        private static void SetRenderMat()
        {
            if (!matRender)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                matRender = new Material(shader);

                matRender.hideFlags = HideFlags.HideAndDontSave;
                matRender.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                matRender.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                matRender.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                matRender.SetInt("_ZWrite", 0);
            }
        }


        // draws point with the given size and color
        public static void DrawPoint(int x, int y, float size, Color color)
        {
            Vector3 vPoint = new Vector3(x, y, 0);
            DrawPoint(vPoint, size, color);
        }

        // draws point with the given size and color
        public static void DrawPoint(Vector3 vPoint, float quadSize, Color color)
        {
            if (!matRender)
            {
                SetRenderMat();
            }

            GL.PushMatrix();
            matRender.SetPass(0);

            GL.LoadPixelMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(color);

            _DrawPoint(vPoint, quadSize);

            GL.End();
            GL.PopMatrix();
        }

        // draws list of points with the given size and color
        public static void DrawPoints(List<Vector3> alPoints, float quadSize, Color color)
        {
            if (alPoints == null)
                return;

            if (!matRender)
            {
                SetRenderMat();
            }

            GL.PushMatrix();
            matRender.SetPass(0);

            GL.LoadPixelMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(color);

            foreach (Vector3 v in alPoints)
            {
                _DrawPoint(v, quadSize);
            }

            GL.End();
            GL.PopMatrix();
        }

        // draws point with given size
        private static void _DrawPoint(Vector3 v, float quadSize)
        {
            float q2 = quadSize / 2f;
            GL.Vertex3(v.x - q2, v.y - q2, 0f);
            GL.Vertex3(v.x - q2, v.y + q2, 0f);
            GL.Vertex3(v.x + q2, v.y + q2, 0f);
            GL.Vertex3(v.x + q2, v.y - q2, 0f);
        }

        // draws a line with the given width and color
        public static void DrawLine(int x0, int y0, int x1, int y1, float width, Color color)
        {
            Vector3 v0 = new Vector3(x0, y0, 0);
            Vector3 v1 = new Vector3(x1, y1, 0);
            DrawLine(v0, v1, width, color);
        }

        // draws a line with the given width and color
        public static void DrawLine(Vector3 v0, Vector3 v1, float lineWidth, Color color)
        {
            if (!matRender)
            {
                SetRenderMat();
            }

            GL.PushMatrix();
            matRender.SetPass(0);

            GL.LoadPixelMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(color);

            _DrawLine(v0, v1, lineWidth);

            GL.End();
            GL.PopMatrix();
        }

        // draws list of lines with the given width and color
        public static void DrawLines(List<Vector3> alLinePoints, float lineWidth, Color color)
        {
            if (alLinePoints == null)
                return;

            if (!matRender)
            {
                SetRenderMat();
            }

            GL.PushMatrix();
            matRender.SetPass(0);

            GL.LoadPixelMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(color);

            for (int i = 0; i < alLinePoints.Count; i += 2)
            {
                Vector3 v0 = alLinePoints[i];
                Vector3 v1 = alLinePoints[i + 1];

                _DrawLine(v0, v1, lineWidth);
            }

            GL.End();
            GL.PopMatrix();
        }

        // draws rectangle with the given width and color
        public static void DrawRect(Rect rect, float width, Color color)
        {
            Vector3 topLeft = new Vector3(rect.xMin, rect.yMin, 0);
            Vector3 bottomRight = new Vector3(rect.xMax, rect.yMax, 0);
            DrawRect(topLeft, bottomRight, width, color);
        }

        // draws rectangle with the given width and color
        public static void DrawRect(Vector3 topLeft, Vector3 bottomRight, float lineWidth, Color color)
        {
            if (!matRender)
            {
                SetRenderMat();
            }

            GL.PushMatrix();
            matRender.SetPass(0);

            GL.LoadPixelMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(color);

            // top
            Vector3 v0 = topLeft;
            Vector3 v1 = topLeft; v1.x = bottomRight.x;
            _DrawLine(v0, v1, lineWidth);

            // right
            v0 = v1;
            v1 = bottomRight;
            _DrawLine(v0, v1, lineWidth);

            // bottom
            v0 = v1;
            v1 = topLeft; v1.y = bottomRight.y;
            _DrawLine(v0, v1, lineWidth);

            // left
            v0 = v1;
            v1 = topLeft;
            _DrawLine(v0, v1, lineWidth);

            GL.End();
            GL.PopMatrix();
        }

        // draws line from v0 to v1 with the given width
        private static void _DrawLine(Vector3 v0, Vector3 v1, float lineWidth)
        {
            Vector3 n = ((new Vector3(v1.y, v0.x, 0f)) - (new Vector3(v0.y, v1.x, 0f))).normalized * lineWidth;
            GL.Vertex3(v0.x - n.x, v0.y - n.y, 0f);
            GL.Vertex3(v0.x + n.x, v0.y + n.y, 0f);
            GL.Vertex3(v1.x + n.x, v1.y + n.y, 0f);
            GL.Vertex3(v1.x - n.x, v1.y - n.y, 0f);
        }

        // converts current body frame to a single csv line. returns empty string if there is no body data
        public static string GetBodyFrameAsCsv(ref BodyData[] bodyData, uint bodyCount, ulong liRelTime, Vector3 spaceScale, ref float fUnityTime, char delimiter)
        {
            // check for invalid sensor data and if the frame is still the same
            if (bodyData == null)
                return string.Empty;

            // create the output string
            System.Text.StringBuilder sbBuf = new System.Text.StringBuilder();
            System.Globalization.CultureInfo invCulture = System.Globalization.CultureInfo.InvariantCulture;

            sbBuf.Append("k4b").Append(delimiter);
            sbBuf.Append(liRelTime).Append(delimiter);

            fUnityTime = Time.time;
            int jointCount = (int)JointType.Count;
            sbBuf.Append(bodyCount).Append(delimiter);
            sbBuf.Append(jointCount).Append(delimiter);

            sbBuf.Append((int)spaceScale.x).Append(delimiter);
            sbBuf.Append((int)spaceScale.y).Append(delimiter);
            sbBuf.Append((int)spaceScale.z).Append(delimiter);

            // add information for all bodies
            for (int i = 0; i < bodyCount; i++)
            {
                sbBuf.Append(bodyData[i].bIsTracked ? "1" : "0").Append(delimiter);

                if (bodyData[i].bIsTracked)
                {
                    // add information for the tracked body - body-id and joints
                    sbBuf.Append(bodyData[i].liTrackingID).Append(delimiter);

                    for (int j = 0; j < jointCount; j++)
                    {
                        JointData jointData = bodyData[i].joint[j];

                        sbBuf.Append((int)jointData.trackingState).Append(delimiter);

                        if (jointData.trackingState != TrackingState.NotTracked)
                        {
                            Vector3 jointPos = jointData.kinectPos;
                            Vector3 jointRot = jointData.normalRotation.eulerAngles;

                            sbBuf.AppendFormat(invCulture, "{0:F3}", jointPos.x).Append(delimiter);
                            sbBuf.AppendFormat(invCulture, "{0:F3}", jointPos.y).Append(delimiter);
                            sbBuf.AppendFormat(invCulture, "{0:F3}", jointPos.z).Append(delimiter);

                            sbBuf.AppendFormat(invCulture, "{0:F3}", jointRot.x).Append(delimiter);
                            sbBuf.AppendFormat(invCulture, "{0:F3}", jointRot.y).Append(delimiter);
                            sbBuf.AppendFormat(invCulture, "{0:F3}", jointRot.z).Append(delimiter);
                        }
                    }
                }
            }

            // remove the last delimiter
            if (sbBuf.Length > 0 && sbBuf[sbBuf.Length - 1] == delimiter)
            {
                sbBuf.Remove(sbBuf.Length - 1, 1);
            }

            return sbBuf.ToString();
        }

        // sets current body frame from the given csv line. returns the number of bodies
        public static uint SetBodyFrameFromCsv(string sCsvLine, string sDelim, ref BodyData[] bodyData, 
            ref Matrix4x4 sensorToWorld, bool bIgnoreZCoords, out ulong liRelTime)
        {
            liRelTime = 0;

            // check for invalid sensor data and for same frame time
            if (sCsvLine.Length == 0)
                return 0;

            // split the csv line in parts
            char[] delimiters = sDelim.ToCharArray();
            if (delimiters == null || delimiters.Length == 0)
                return 0;

            string[] alCsvParts = sCsvLine.Split(delimiters);
            if (alCsvParts.Length < 7)
                return 0;

            System.Globalization.CultureInfo invCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.NumberStyles numFloat = System.Globalization.NumberStyles.Float;

            // body count & joint count
            int.TryParse(alCsvParts[2], out int bodyCount);
            int.TryParse(alCsvParts[3], out int jointCount);

            // space scale
            int.TryParse(alCsvParts[4], out int iSX);
            int.TryParse(alCsvParts[5], out int iSY);
            int.TryParse(alCsvParts[6], out int iSZ);
            Vector3 sensorSpaceScale = new Vector3(iSX, iSY, iSZ);

            ulong.TryParse(alCsvParts[1], out liRelTime);

            if (alCsvParts[0] != "k4b" || bodyCount == 0 || jointCount == 0 || liRelTime == 0)
                return 0;
            //if (bodyCount != sensorData.bodyCount || jointCount != sensorData.jointCount)
            //    return 0;

            //// update body frame data
            //bodyFrame.liPreviousTime = bodyFrame.liRelativeTime;
            //bodyFrame.liRelativeTime = liRelTime;

            if (bodyData.Length < bodyCount)
            {
                Array.Resize<BodyData>(ref bodyData, bodyCount);
                for (int i = 0; i < bodyCount; i++)
                {
                    bodyData[i] = new BodyData((int)JointType.Count);
                }
            }

            int iIndex = 7;
            for (int i = 0; i < bodyCount; i++)
            {
                if (alCsvParts.Length < (iIndex + 1))
                    return 0;

                // update the tracked-flag and body id
                short bIsTracked = 0;
                ulong liTrackingID = 0;

                short.TryParse(alCsvParts[iIndex], out bIsTracked);
                iIndex++;

                if (bIsTracked != 0 && alCsvParts.Length >= (iIndex + 1))
                {
                    ulong.TryParse(alCsvParts[iIndex], out liTrackingID);
                    iIndex++;

                    if (liTrackingID == 0)
                    {
                        bIsTracked = 0;
                    }
                }

                bodyData[i].bIsTracked = (bIsTracked != 0);
                bodyData[i].liTrackingID = liTrackingID;

                if (bIsTracked != 0)
                {
                    if (jointCount != (int)JointType.Count)
                    {
                        // set the other joints as not tracked
                        for (int j = jointCount; j < (int)JointType.Count; j++)
                        {
                            bodyData[i].joint[j].trackingState = TrackingState.NotTracked;
                        }
                    }

                    // update joints' data
                    for (int j = 0; j < jointCount; j++)
                    {
                        JointData jointData = bodyData[i].joint[j];
                        int iTrackingState = 0;

                        if (alCsvParts.Length >= (iIndex + 1))
                        {
                            int.TryParse(alCsvParts[iIndex], out iTrackingState);
                            iIndex++;

                            jointData.trackingState = (KinectInterop.TrackingState)iTrackingState;

                            if (iTrackingState != (int)TrackingState.NotTracked && alCsvParts.Length >= (iIndex + 6))
                            {
                                float.TryParse(alCsvParts[iIndex], numFloat, invCulture, out float x);
                                float.TryParse(alCsvParts[iIndex + 1], numFloat, invCulture, out float y);
                                float.TryParse(alCsvParts[iIndex + 2], numFloat, invCulture, out float z);
                                iIndex += 3;

                                jointData.kinectPos = new Vector3(x, y, z);

                                float.TryParse(alCsvParts[iIndex], numFloat, invCulture, out x);
                                float.TryParse(alCsvParts[iIndex + 1], numFloat, invCulture, out y);
                                float.TryParse(alCsvParts[iIndex + 2], numFloat, invCulture, out z);
                                iIndex += 3;

                                jointData.normalRotation = Quaternion.Euler(x, y, z);
                            }
                            else
                            {
                                jointData.kinectPos = Vector3.zero;
                            }

                            float jPosZ = (bIgnoreZCoords && j > 0) ? bodyData[i].joint[0].kinectPos.z : jointData.kinectPos.z;
                            jointData.position = sensorToWorld.MultiplyPoint3x4(new Vector3(jointData.kinectPos.x * sensorSpaceScale.x, jointData.kinectPos.y * sensorSpaceScale.y, jPosZ));
                            jointData.orientation = Quaternion.identity;

                            Vector3 mirroredAngles = jointData.normalRotation.eulerAngles;
                            mirroredAngles.y = -mirroredAngles.y;
                            mirroredAngles.z = -mirroredAngles.z;
                            jointData.mirroredRotation = Quaternion.Euler(mirroredAngles);

                            if (j == 0)
                            {
                                // set body position
                                bodyData[i].position = jointData.position;
                                bodyData[i].orientation = jointData.orientation;

                                bodyData[i].normalRotation = jointData.normalRotation;
                                bodyData[i].mirroredRotation = jointData.mirroredRotation;
                            }
                        }

                        bodyData[i].joint[j] = jointData;
                    }

                    // calculate bone directions
                    CalcBodyJointDirs(ref bodyData[i]);

                }
            }

            return (uint)bodyCount;
        }


        // calculates all bone directions for the given body
        public static void CalcBodyJointDirs(ref BodyData bodyData)
        {
            if (bodyData.bIsTracked)
            {
                for (int j = 0; j < (int)JointType.Count; j++)
                {
                    if (j == 0)
                    {
                        bodyData.joint[j].direction = Vector3.zero;
                    }
                    else
                    {
                        int jParent = (int)GetParentJoint((JointType)j);

                        if (bodyData.joint[j].trackingState != TrackingState.NotTracked &&
                            bodyData.joint[jParent].trackingState != TrackingState.NotTracked)
                        {
                            bodyData.joint[j].direction = (bodyData.joint[j].position - bodyData.joint[jParent].position); //.normalized;
                        }
                    }
                }
            }
        }

    }
}
