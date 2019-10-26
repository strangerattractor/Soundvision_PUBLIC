using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace com.rfilkov.kinect
{
    /// <summary>
    /// KinectManager is the the main and most basic depth-sensor related component. It controls the sensors and manages the data streams.
    /// </summary>
    public class KinectManager : MonoBehaviour
    {

        [Header("Sensor Data")]

        [Tooltip("Whether to get depth frames from the sensor(s).")]
        public DepthTextureType getDepthFrames = DepthTextureType.RawDepthData;
        public enum DepthTextureType : int { None = 0, RawDepthData = 1, DepthTexture = 2 }

        [Tooltip("Whether to get color frames from the sensor(s).")]
        public ColorTextureType getColorFrames = ColorTextureType.None;
        public enum ColorTextureType : int { None = 0, ColorTexture = 2 }

        [Tooltip("Whether to get infrared frames from the sensor(s).")]
        public InfraredTextureType getInfraredFrames = InfraredTextureType.None;
        public enum InfraredTextureType : int { None = 0, RawInfraredData = 1, InfraredTexture = 2 }

        [Tooltip("Whether to get pose frames from the sensor(s).")]
        public PoseUsageType getPoseFrames = PoseUsageType.None;
        public enum PoseUsageType : int { None = 0, RawPoseData = 1, DisplayInfo = 10, UpdateTransform = 20 }

        [Tooltip("Whether to get body frames from the body tracker.")]
        public BodyTextureType getBodyFrames = BodyTextureType.RawBodyData;
        public enum BodyTextureType : int { None = 0, RawBodyData = 1, BodyTexture = 2, UserTexture = 3  /**, BodyTexture, UserTexture, CutOutTexture */ }

        [Tooltip("Whether to poll the sensor frames in separate threads or in the Update-method.")]
        private bool pollFramesInThread = true;

        [Tooltip("Whether to synchronize depth and color frames.")]
        public bool syncDepthAndColor = false;

        [Tooltip("Whether to synchronize body and depth frames.")]
        public bool syncBodyAndDepth = false;

        //[Tooltip("List of additional data frames to be computed from the latest depth and color frames. Please note, these data frames require getting both depth & color frames, as well as sync between them.")]
        //public List<AdditionalFrameType> additionalFrames = new List<AdditionalFrameType>();
        //public enum AdditionalFrameType : int { Depth2ColorCoordinatesFrame, Color2DepthCoordinatesFrame, AlignedDepth2ColorFrame, AlignedColor2DepthFrame, PointCloudMeshFrame, PointCloudVerticesFrame, PointCloudUvFrame, PointCloudColorFrame }

        [Header("User Detection")]

        [Tooltip("Minimum distance to user, in order to be considered for body processing. Value of 0 means no minimum distance limitation.")]
        [Range(0f, 10f)]
        public float minUserDistance = 0f;

        [Tooltip("Maximum distance to user, in order to be considered for body processing. Value of 0 means no maximum distance limitation.")]
        [Range(0f, 10f)]
        public float maxUserDistance = 0f;

        [Tooltip("Maximum left or right distance to user, in order to be considered for body processing. Value of 0 means no left/right distance limitation.")]
        [Range(0f, 5f)]
        public float maxLeftRightDistance = 0f;

        [Tooltip("Maximum number of users, who may be tracked simultaneously. Value of 0 means no limitation.")]
        public int maxTrackedUsers = 0;

        [Tooltip("Whether to display only the users within the allowed distances, or all users.")]
        public bool showAllowedUsersOnly = false;

        public enum UserDetectionOrder : int { Appearance = 0, Distance = 1, LeftToRight = 2 }
        [Tooltip("How to assign users to player indices - by order of appearance, distance or left-to-right.")]
        public UserDetectionOrder userDetectionOrder = UserDetectionOrder.Appearance;

        [Tooltip("Whether to ignore the inferred joints, or consider them as tracked joints.")]
        public bool ignoreInferredJoints = false;

        [Tooltip("Whether to ignore the Z-coordinates of the joints (for 2D-scenes) or not.")]
        public bool ignoreZCoordinates = false;

        [Tooltip("Whether to apply the bone orientation constraints.")]
        public bool boneOrientationConstraints = true;

        [Tooltip("Wait time in seconds, before a lost user gets removed. This is to prevent sporadical user switches.")]
        protected float waitTimeBeforeRemove = 0.1f;

        [Tooltip("Calibration pose required, to start tracking the respective user.")]
        public GestureType playerCalibrationPose = GestureType.None;

        [Tooltip("User manager, used to track the users. KM creates one, if not set.")]
        public KinectUserManager userManager;

        //[Tooltip("List of the avatar controllers in the scene. If the list is empty, the available avatar controllers are detected at the scene start up.")]
        //public List<AvatarController> avatarControllers = new List<AvatarController>();

        [Header("Gesture Detection")]

        //[Tooltip("List of common gestures, to be detected for each player.")]
        //public List<GestureType> playerCommonGestures = new List<GestureType>();

        //[Tooltip("Minimum time between gesture detections (in seconds).")]
        //public float minTimeBetweenGestures = 0.7f;

        [Tooltip("Gesture manager, used to detect user gestures. KM creates one, if not set.")]
        public KinectGestureManager gestureManager;

        //[Tooltip("List of the gesture listeners in the scene. If the list is empty, the available gesture listeners will be detected at the scene start up.")]
        //public List<MonoBehaviour> gestureListeners = new List<MonoBehaviour>();

        [Header("On-Screen Info")]

        [Tooltip("List of images to display on the screen.")]
        public List<DisplayImageType> displayImages = new List<DisplayImageType>();
        public enum DisplayImageType : int
        {
            None = 0,
            Sensor0ColorImage = 0x01, Sensor0DepthImage = 0x02, Sensor0InfraredImage = 0x03,
            Sensor1ColorImage = 0x11, Sensor1DepthImage = 0x12, Sensor1InfraredImage = 0x13,
            Sensor2ColorImage = 0x21, Sensor2DepthImage = 0x22, Sensor2InfraredImage = 0x23,
            UserBodyImage = 0x101
        }

        [Tooltip("Single image width, as percent of the screen width. The height is estimated according to the image's aspect ratio.")]
        [Range(0.1f, 0.5f)]
        public float displayImageWidthPercent = 0.2f;

        [Tooltip("UI-Text to display status messages.")]
        public UnityEngine.UI.Text statusInfoText;



        // Bool to keep track of whether Kinect has been initialized
        protected bool kinectInitialized = false;

        // The singleton instance of KinectManager
        protected static KinectManager instance = null;

        // available sensor interfaces
        protected List<DepthSensorInterface> sensorInterfaces = new List<DepthSensorInterface>();
        // the respective SensorData structures
        protected List<KinectInterop.SensorData> sensorDatas = new List<KinectInterop.SensorData>();

        // body frame data
        protected ulong lastBodyFrameTime = 0;
        protected uint trackedBodiesCount = 0;
        protected KinectInterop.BodyData[] alTrackedBodies = new KinectInterop.BodyData[0];  // new List<KinectInterop.BodyData>();

        protected int btSensorIndex = -1;
        protected int selectedBodyIndex = 255;
        protected bool bLimitedUsers = false;

        protected BoneOrientationConstraints boneConstraints = null;

        // play mode
        protected bool isPlayModeEnabled = false;
        protected string playModeData = string.Empty;


        /// <summary>
        /// Gets the single KinectManager instance.
        /// </summary>
        /// <value>The KinectManager instance.</value>
        public static KinectManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Determines if the KinectManager-component is initialized and ready to use.
        /// </summary>
        /// <returns><c>true</c> if KinectManager is initialized; otherwise, <c>false</c>.</returns>
        public bool IsInitialized()
        {
            return kinectInitialized;
        }

        /// <summary>
        /// Returns the number of utilized depth sensors.
        /// </summary>
        /// <returns>The number of depth sensors.</returns>
        public int GetSensorCount()
        {
            return sensorDatas.Count;
        }

        ///// <summary>
        ///// Gets the sensor-data structure of the 1st sensor (this structure should not be modified, because it is used internally).
        ///// </summary>
        ///// <returns>The sensor data.</returns>
        //internal KinectInterop.SensorData GetSensorData()
        //{
        //    return GetSensorData(0);
        //}

        /// <summary>
        /// Gets the sensor-data structure of the given sensor (this structure should not be modified, because it is used internally).
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The sensor data.</returns>
        internal KinectInterop.SensorData GetSensorData(int sensorIndex)
        {
            if(sensorIndex >= 0  && sensorIndex < sensorDatas.Count)
            {
                return sensorDatas[sensorIndex];
            }

            return null;
        }

        /// <summary>
        /// Gets the minimum distance tracked by the sensor, in meters.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>Minimum distance tracked by the sensor, in meters.</returns>
        public float GetSensorMinDistance(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if(sensorData != null && sensorData.sensorInterface != null)
            {
                return ((DepthSensorBase)sensorData.sensorInterface).minDistance;
            }

            return 0f;
        }

        /// <summary>
        /// Gets the maximum distance tracked by the sensor, in meters.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>Maximum distance tracked by the sensor, in meters.</returns>
        public float GetSensorMaxDistance(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData != null && sensorData.sensorInterface != null)
            {
                return ((DepthSensorBase)sensorData.sensorInterface).maxDistance;
            }

            return 0f;
        }

        /// <summary>
        /// Gets the last color frame time, as returned by the sensor.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The color frame time.</returns>
        public ulong GetColorFrameTime(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.lastColorFrameTime : 0;
        }

        /// <summary>
        /// Gets the width of the color image, returned by the sensor.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The color image width.</returns>
        public int GetColorImageWidth(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.colorImageWidth : 0;
        }

        /// <summary>
        /// Gets the height of the color image, returned by the sensor.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The color image height.</returns>
        public int GetColorImageHeight(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.colorImageHeight : 0;
        }

        /// <summary>
        /// Gets the color image scale.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The color image scale.</returns>
        public Vector3 GetColorImageScale(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.colorImageScale : Vector3.one;
        }

        /// <summary>
        /// Gets the color image texture.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The color image texture.</returns>
        public Texture GetColorImageTex(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.colorImageTexture : null;
        }

        /// <summary>
        /// Gets the last depth frame time, as returned by the sensor.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The depth frame time.</returns>
        public ulong GetDepthFrameTime(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.lastDepthFrameTime : 0;
        }

        /// <summary>
        /// Gets the last IR frame time, as returned by the sensor.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The IR frame time.</returns>
        public ulong GetInfraredFrameTime(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.lastInfraredFrameTime : 0;
        }

        /// <summary>
        /// Gets the width of the depth image, returned by the sensor.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The depth image width.</returns>
        public int GetDepthImageWidth(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.depthImageWidth : 0;
        }

        /// <summary>
        /// Gets the height of the depth image, returned by the sensor.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The depth image height.</returns>
        public int GetDepthImageHeight(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.depthImageHeight : 0;
        }

        /// <summary>
        /// Gets the raw depth data, if ComputeUserMap is true.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The raw depth map.</returns>
        public ushort[] GetRawDepthMap(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.depthImage : null;
        }

        /// <summary>
        /// Gets the raw infrared data, if ComputeInfraredMap is true.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The raw infrared map.</returns>
        public ushort[] GetRawInfraredMap(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.infraredImage : null;
        }

        /// <summary>
        /// Gets the depth image scale.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The depth image scale.</returns>
        public Vector3 GetDepthImageScale(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.depthImageScale : Vector3.one;
        }

        /// <summary>
        /// Gets the infrared image scale.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The infrared image scale.</returns>
        public Vector3 GetInfraredImageScale(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.infraredImageScale : Vector3.one;
        }

        /// <summary>
        /// Gets the sensor space scale.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The sensor space scale.</returns>
        public Vector3 GetSensorSpaceScale(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.sensorSpaceScale : Vector3.one;
        }

        /// <summary>
        /// Gets the depth image texture.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The depth texture.</returns>
        public Texture GetDepthImageTex(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.depthImageTexture : null;
        }

        /// <summary>
        /// Gets the infrared image texture.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The infrared texture.</returns>
        public Texture GetInfraredImageTex(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.infraredImageTexture : null;
        }

        /// <summary>
        /// Gets the depth value for the specified pixel, if ComputeUserMap is true.
        /// </summary>
        /// <returns>The depth value.</returns>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        public ushort GetDepthForPixel(int sensorIndex, int x, int y)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData != null && sensorData.depthImage != null)
            {
                int index = y * sensorData.depthImageWidth + x;

                if (index >= 0 && index < sensorData.depthImage.Length)
                {
                    return sensorData.depthImage[index];
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the depth value for the specified pixel, if ComputeUserMap is true.
        /// </summary>
        /// <returns>The depth value.</returns>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <param name="index">Depth index.</param>
        public ushort GetDepthForIndex(int sensorIndex, int index)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData != null && sensorData.depthImage != null)
            {
                if (index >= 0 && index < sensorData.depthImage.Length)
                {
                    return sensorData.depthImage[index];
                }
            }

            return 0;
        }


        /// <summary>
        /// Returns the respective sensor-to-world matrix.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>Sensor-to-world matrix.</returns>
        public Matrix4x4 GetSensorToWorldMatrix(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return KinectInterop.GetSensorToWorldMatrix(sensorData);
        }


        /// <summary>
        /// Returns the sensor transform reference. Please note transform updates depend on the getPoseFrames-KM setting.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>Sensor transorm or null, if sensorIndex is invalid.</returns>
        public Transform GetSensorTransform(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return KinectInterop.GetSensorTransform(sensorData);
        }


        /// <summary>
        /// Returns the depth camera space coordinates of a depth-image point, or Vector3.zero if the sensor is not initialized.
        /// </summary>
        /// <returns>The space coordinates.</returns>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <param name="posPoint">Depth image coordinates</param>
        /// <param name="depthValue">Depth value</param>
        /// <param name="bWorldCoords">If set to <c>true</c>, applies the sensor height and angle to the space coordinates.</param>
        public Vector3 MapDepthPointToSpaceCoords(int sensorIndex, Vector2 posPoint, ushort depthValue, bool bWorldCoords)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData != null)
            {
                Vector3 posSpace = KinectInterop.MapDepthPointToSpaceCoords(sensorData, posPoint, depthValue);

                if (bWorldCoords)
                {
                    Vector3 spaceScale = sensorData.sensorSpaceScale;
                    posSpace = new Vector3(posSpace.x * spaceScale.x, posSpace.y * spaceScale.y, posSpace.z * spaceScale.z);

                    Matrix4x4 sensorToWorld = KinectInterop.GetSensorToWorldMatrix(sensorData);
                    posSpace = sensorToWorld.MultiplyPoint3x4(posSpace);
                }

                return posSpace;
            }

            return Vector3.zero;
        }


        /// <summary>
        /// Returns the depth-image coordinates of a depth camera space point, or Vector2.zero if the sensor is not initialized.
        /// </summary>
        /// <returns>The depth-image coordinates.</returns>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <param name="posPoint">Space point coordinates</param>
        public Vector2 MapSpacePointToDepthCoords(int sensorIndex, Vector3 posPoint)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData != null)
            {
                return KinectInterop.MapSpacePointToDepthCoords(sensorData, posPoint);
            }

            return Vector2.zero;
        }


        /// <summary>
        /// Returns the color camera space coordinates of a color-image point, or Vector3.zero if the sensor is not initialized.
        /// </summary>
        /// <returns>The space coordinates.</returns>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <param name="posPoint">Color image coordinates</param>
        /// <param name="distance">Distance in mm</param>
        public Vector3 MapColorPointToSpaceCoords(int sensorIndex, Vector2 posPoint, ushort distance)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData != null)
            {
                return KinectInterop.MapColorPointToSpaceCoords(sensorData, posPoint, distance);
            }

            return Vector3.zero;
        }


        /// <summary>
        /// Returns the color-image coordinates of a color camera space point, or Vector2.zero if the sensor is not initialized.
        /// </summary>
        /// <returns>The color-image coordinates.</returns>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <param name="posPoint">Space point coordinates</param>
        public Vector2 MapSpacePointToColorCoords(int sensorIndex, Vector3 posPoint)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData != null)
            {
                return KinectInterop.MapSpacePointToColorCoords(sensorData, posPoint);
            }

            return Vector2.zero;
        }


        /// <summary>
        /// Returns the color-image coordinates of a depth-image point.
        /// </summary>
        /// <returns>The color-image coordinates.</returns>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <param name="posDepth">Depth image coordinates</param>
        /// <param name="depthValue">Depth value</param>
        public Vector2 MapDepthPointToColorCoords(int sensorIndex, Vector2 posDepth, ushort depthValue)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData != null)
            {
                return KinectInterop.MapDepthPointToColorCoords(sensorData, posDepth, depthValue);
            }

            return Vector2.zero;
        }


        /// <summary>
        /// Returns the depth-image coordinates of a color-image point.
        /// </summary>
        /// <returns>The depth-image coordinates.</returns>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <param name="posColor">Color image coordinates</param>
        public Vector2 MapColorPointToDepthCoords(int sensorIndex, Vector2 posColor)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData != null)
            {
                return KinectInterop.MapColorPointToDepthCoords(sensorData, posColor);
            }

            return Vector2.zero;
        }


        ///// <summary>
        ///// Maps the depth frame to space coordinates.
        ///// </summary>
        ///// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
        ///// <param name="sensorIndex">The sensor index.</param>
        ///// <param name="avSpaceCoords">Buffer for the depth-to-space coordinates.</param>
        //public bool MapDepthFrameToSpaceCoords(int sensorIndex, ref Vector3[] avSpaceCoords)
        //{
        //    bool bResult = false;

        //    KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
        //    if (sensorData != null && sensorData.depthImage != null)
        //    {
        //        if (avSpaceCoords == null || avSpaceCoords.Length == 0)
        //        {
        //            avSpaceCoords = new Vector3[sensorData.depthImageWidth * sensorData.depthImageHeight];
        //        }

        //        bResult = KinectInterop.MapDepthFrameToSpaceCoords(sensorData, ref avSpaceCoords);
        //    }

        //    return bResult;
        //}


        ///// <summary>
        ///// Returns the depth-map coordinates of a color point.
        ///// </summary>
        ///// <returns>The depth coords.</returns>
        ///// <param name="sensorIndex">The sensor index.</param>
        ///// <param name="colorPos">Color position.</param>
        ///// <param name="bReadDepthCoordsIfNeeded">If set to <c>true</c> allows reading of depth coords, if needed.</param>
        //public Vector2 MapColorPointToDepthCoords(int sensorIndex, Vector2 colorPos, bool bReadDepthCoordsIfNeeded)
        //{
        //    KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
        //    if (sensorData != null && sensorData.colorImageTexture != null && sensorData.depthImage != null)
        //    {
        //        return KinectInterop.MapColorPointToDepthCoords(sensorData, colorPos, bReadDepthCoordsIfNeeded);
        //    }

        //    return Vector2.zero;
        //}


        ///// <summary>
        ///// Maps the depth frame to color coordinates.
        ///// </summary>
        ///// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
        ///// <param name="sensorIndex">The sensor index.</param>
        ///// <param name="avColorCoords">Buffer for depth-to-color coordinates.</param>
        //public bool MapDepthFrameToColorCoords(int sensorIndex, ref Vector2[] avColorCoords)
        //{
        //    bool bResult = false;

        //    KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
        //    if (sensorData != null && sensorData.depthImage != null && sensorData.colorImageTexture != null)
        //    {
        //        if (avColorCoords == null || avColorCoords.Length == 0)
        //        {
        //            avColorCoords = new Vector2[sensorData.depthImageWidth * sensorData.depthImageHeight];
        //        }

        //        bResult = KinectInterop.MapDepthFrameToColorCoords(sensorData, ref avColorCoords);
        //    }

        //    return bResult;
        //}


        ///// <summary>
        ///// Maps the color frame to depth coordinates.
        ///// </summary>
        ///// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
        ///// <param name="sensorIndex">The sensor index.</param>
        ///// <param name="avDepthCoords">Buffer for color-to-depth coordinates.</param>
        //public bool MapColorFrameToDepthCoords(int sensorIndex, ref Vector2[] avDepthCoords)
        //{
        //    bool bResult = false;

        //    KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
        //    if (sensorData != null && sensorData.colorImageTexture != null && sensorData.depthImage != null)
        //    {
        //        if (avDepthCoords == null || avDepthCoords.Length == 0)
        //        {
        //            avDepthCoords = new Vector2[sensorData.colorImageWidth * sensorData.colorImageWidth];
        //        }

        //        bResult = KinectInterop.MapColorFrameToDepthCoords(sensorData, ref avDepthCoords);
        //    }

        //    return bResult;
        //}


        /// <summary>
        /// Gets the last body frame time, as returned by the sensor.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The body frame time.</returns>
        public ulong GetBodyFrameTime(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.lastBodyFrameTime : 0;
        }

        /// <summary>
        /// Gets the last body index frame time, as returned by the sensor.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The body index frame time.</returns>
        public ulong GetBodyIndexFrameTime(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.lastBodyIndexFrameTime : 0;
        }

        /// <summary>
        /// Gets the users' image texture.
        /// </summary>
        /// <returns>The user bodies texture.</returns>
        public Texture GetUsersImageTex()
        {
            return GetUsersImageTex(btSensorIndex);
        }

        /// <summary>
        /// Gets the users' image texture.
        /// </summary>
        /// <param name="sensorIndex">The sensor index.</param>
        /// <returns>The user bodies texture.</returns>
        public Texture GetUsersImageTex(int sensorIndex)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            return sensorData != null ? sensorData.bodyImageTexture : null;
        }

        /// <summary>
        /// Determines whether the user with the specified index is currently detected by the sensor
        /// </summary>
        /// <returns><c>true</c> if the user is detected; otherwise, <c>false</c>.</returns>
        /// <param name="i">The user index.</param>
        public bool IsUserDetected(int i)
        {
            if (i >= 0 && i < KinectInterop.Constants.MaxBodyCount)
            {
                return (userManager.aUserIndexIds[i] != 0);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the user with the specified userId is in the list of tracked users or not.
        /// </summary>
        /// <returns><c>true</c> if the user with the specified userId is tracked; otherwise, <c>false</c>.</returns>
        /// <param name="userId">User identifier.</param>
        public bool IsUserTracked(ulong userId)
        {
            return userManager.dictUserIdToIndex.ContainsKey(userId);
        }

        /// <summary>
        /// Gets the number of currently tracked users.
        /// </summary>
        /// <returns>The users count.</returns>
        public int GetUsersCount()
        {
            return userManager.alUserIds.Count;
        }

        /// <summary>
        /// Gets the IDs of all currently tracked users.
        /// </summary>
        /// <returns>The list of all currently tracked users.</returns>
        public List<ulong> GetAllUserIds()
        {
            return new List<ulong>(userManager.alUserIds);
        }

        /// <summary>
        /// Gets the max player-index of the currently tracked users.
        /// </summary>
        /// <returns>The max player-index of the tracked users.</returns>
        public int GetMaxUserIndex()
        {
            int maxIndex = -1;

            for (int i = KinectInterop.Constants.MaxBodyCount - 1; i >= 0; i--)
            {
                if (userManager.aUserIndexIds[i] != 0)
                {
                    maxIndex = i;
                    break;
                }
            }

            return maxIndex;
        }

        /// <summary>
        /// Gets the player indices of all currently tracked users.
        /// </summary>
        /// <returns>The list of player-indices of all tracked users.</returns>
        public List<int> GetAllUserIndices()
        {
            List<int> alIndices = new List<int>();

            for (int i = 0; i < KinectInterop.Constants.MaxBodyCount; i++)
            {
                if (userManager.aUserIndexIds[i] != 0)
                {
                    alIndices.Add(i);
                }
            }

            return alIndices;
        }

        /// <summary>
        /// Gets the user ID by the specified user index.
        /// </summary>
        /// <returns>The user ID by index.</returns>
        /// <param name="i">The user index.</param>
        public ulong GetUserIdByIndex(int i)
        {
            if (i >= 0 && i < KinectInterop.Constants.MaxBodyCount)
            {
                return userManager.aUserIndexIds[i];
            }

            return 0;
        }

        /// <summary>
        /// Gets the user index by the specified user ID.
        /// </summary>
        /// <returns>The user index by user ID.</returns>
        /// <param name="userId">User ID</param>
        public int GetUserIndexById(ulong userId)
        {
            if (userId == 0)
                return -1;

            for (int i = 0; i < userManager.aUserIndexIds.Length; i++)
            {
                if (userManager.aUserIndexIds[i] == userId)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the body index by the specified user ID, or -1 if the user ID does not exist.
        /// </summary>
        /// <returns>The body index by user ID.</returns>
        /// <param name="userId">User ID</param>
        public int GetBodyIndexByUserId(ulong userId)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    int bodyIndex = alTrackedBodies[index].iBodyIndex;
                    return bodyIndex;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the list of tracked body indices.
        /// </summary>
        /// <returns>The list of body indices.</returns>
        public List<int> GetTrackedBodyIndices()
        {
            List<int> alBodyIndices = new List<int>(userManager.dictUserIdToIndex.Values);
            return alBodyIndices;
        }

        /// <summary>
        /// Determines whether the tracked users are limited by their number or distance or not.
        /// </summary>
        /// <returns><c>true</c> if the users are limited by number or distance; otherwise, <c>false</c>.</returns>
        public bool IsTrackedUsersLimited()
        {
            return bLimitedUsers;
        }

        /// <summary>
        /// Gets the UserID of the primary user (the first or the closest one), or 0 if no user is detected.
        /// </summary>
        /// <returns>The primary user ID.</returns>
        public ulong GetPrimaryUserID()
        {
            return userManager.liPrimaryUserId;
        }

        /// <summary>
        /// Sets the primary user ID, in order to change the active user.
        /// </summary>
        /// <returns><c>true</c>, if primary user ID was set, <c>false</c> otherwise.</returns>
        /// <param name="userId">User ID</param>
        public bool SetPrimaryUserID(ulong userId)
        {
            bool bResult = false;

            if (userManager.alUserIds.Contains(userId) || (userId == 0))
            {
                userManager.liPrimaryUserId = userId;
                bResult = true;
            }

            return bResult;
        }

        /// <summary>
        /// Gets the body index, if there is single body selected to be displayed on the user map, or -1 if all bodies are displayed.
        /// </summary>
        /// <returns>The displayed body index, or -1 if all bodies are displayed.</returns>
        public int GetDisplayedBodyIndex()
        {
            return selectedBodyIndex != 255 ? selectedBodyIndex : -1;
        }

        /// <summary>
        /// Sets the body index, if a single body must be displayed on the user map, or -1 if all bodies must be displayed.
        /// </summary>
        /// <returns><c>true</c>, if the change was successful, <c>false</c> otherwise.</returns>
        /// <param name="iBodyIndex">The single body index, or -1 if all bodies must be displayed.</param>
        public void SetDisplayedBodyIndex(int iBodyIndex)
        {
            selectedBodyIndex = (byte)(iBodyIndex >= 0 ? iBodyIndex : 255);
        }

        /// <summary>
        /// Gets the last body frame timestamp.
        /// </summary>
        /// <returns>The last body frame timestamp.</returns>
        public ulong GetBodyFrameTimestamp()
        {
            return lastBodyFrameTime;
        }

        // do not change the data in the structure directly
        /// <summary>
        /// Gets the user body data (for internal purposes only).
        /// </summary>
        /// <returns>The user body data.</returns>
        /// <param name="userId">User ID</param>
        internal KinectInterop.BodyData GetUserBodyData(ulong userId)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount)
                {
                    return alTrackedBodies[index];
                }
            }

            return new KinectInterop.BodyData((int)KinectInterop.JointType.Count);
        }

        /// <summary>
        /// Gets the user position in Kinect coordinate system, in meters.
        /// </summary>
        /// <returns>The user kinect position.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="applyDepthScale">Whether to apply the sensor space scale or not</param>
        public Vector3 GetUserKinectPosition(ulong userId, bool applySpaceScale)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    Vector3 userKinectPos = alTrackedBodies[index].kinectPos;

                    if (applySpaceScale && btSensorIndex >= 0 && btSensorIndex < sensorDatas.Count)
                    {
                        Vector3 spaceScale = sensorDatas[btSensorIndex].sensorSpaceScale;
                        return new Vector3(userKinectPos.x * spaceScale.x, userKinectPos.y * spaceScale.y, userKinectPos.z);
                    }
                    else
                    {
                        return userKinectPos;
                    }
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the user position, relative to the sensor, in meters.
        /// </summary>
        /// <returns>The user position.</returns>
        /// <param name="userId">User ID</param>
        public Vector3 GetUserPosition(ulong userId)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    return alTrackedBodies[index].position;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the user orientation.
        /// </summary>
        /// <returns>The user rotation.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="flip">If set to <c>true</c>, this means non-mirrored rotation.</param>
        public Quaternion GetUserOrientation(ulong userId, bool flip)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (flip)
                        return alTrackedBodies[index].normalRotation;
                    else
                        return alTrackedBodies[index].mirroredRotation;
                }
            }

            return Quaternion.identity;
        }

        /// <summary>
        /// Gets the index of the sensor, used for the primary body tracking.
        /// </summary>
        /// <returns></returns>
        public int GetPrimaryBodySensorIndex()
        {
            return btSensorIndex;
        }

        /// <summary>
        /// Gets the number of bodies, tracked by the sensor.
        /// </summary>
        /// <returns>The body count.</returns>
        public int GetBodyCount()
        {
            return (int)trackedBodiesCount;
        }

        /// <summary>
        /// Gets the maximum possible number of bodies, tracked by the sensor.
        /// </summary>
        /// <returns>The maximum body count.</returns>
        public int GetMaxBodyCount()
        {
            return KinectInterop.Constants.MaxBodyCount;
        }

        /// <summary>
        /// Gets the the number of body joints, tracked by the sensor.
        /// </summary>
        /// <returns>The count of joints.</returns>
        public int GetJointCount()
        {
            return (int)KinectInterop.JointType.Count;
        }

        /// <summary>
        /// Gets the parent joint of the given joint.
        /// </summary>
        /// <returns>The parent joint.</returns>
        /// <param name="joint">Joint.</param>
        public KinectInterop.JointType GetParentJoint(KinectInterop.JointType joint)
        {
            return KinectInterop.GetParentJoint(joint);
        }

        /// <summary>
        /// Gets the next joint of the given joint.
        /// </summary>
        /// <returns>The next joint.</returns>
        /// <param name="joint">Joint.</param>
        public KinectInterop.JointType GetNextJoint(KinectInterop.JointType joint)
        {
            return KinectInterop.GetNextJoint(joint);
        }

        /// <summary>
        /// Gets the tracking state of the joint.
        /// </summary>
        /// <returns>The joint tracking state.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        public KinectInterop.TrackingState GetJointTrackingState(ulong userId, int joint)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
                    {
                        return alTrackedBodies[index].joint[joint].trackingState;
                    }
                }
            }

            return KinectInterop.TrackingState.NotTracked;
        }

        /// <summary>
        /// Determines whether the given joint of the specified user is being tracked.
        /// </summary>
        /// <returns><c>true</c> if this instance is joint tracked the specified userId joint; otherwise, <c>false</c>.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        public bool IsJointTracked(ulong userId, int joint)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
                    {
                        KinectInterop.JointData jointData = alTrackedBodies[index].joint[joint];

                        return ignoreInferredJoints ? ((int)jointData.trackingState >= (int)KinectInterop.TrackingState.Tracked) :
                            (jointData.trackingState != KinectInterop.TrackingState.NotTracked);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the joint position of the specified user, in Kinect coordinate system, in meters.
        /// </summary>
        /// <returns>The joint kinect position.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        /// <param name="applySpaceScale">Whether to apply the sensor space scale or not</param>
        public Vector3 GetJointKinectPosition(ulong userId, int joint, bool applySpaceScale)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
                    {
                        KinectInterop.JointData jointData = alTrackedBodies[index].joint[joint];
                        Vector3 jointKinectPos = jointData.kinectPos;

                        if (applySpaceScale && btSensorIndex >= 0 && btSensorIndex < sensorDatas.Count)
                        {
                            Vector3 spaceScale = sensorDatas[btSensorIndex].sensorSpaceScale;
                            return new Vector3(jointKinectPos.x * spaceScale.x, jointKinectPos.y * spaceScale.y, jointKinectPos.z);
                        }
                        else
                        {
                            return jointKinectPos;
                        }
                    }
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the joint position of the specified user, in meters.
        /// </summary>
        /// <returns>The joint position.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        public Vector3 GetJointPosition(ulong userId, int joint)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
                    {
                        KinectInterop.JointData jointData = alTrackedBodies[index].joint[joint];
                        return jointData.position;
                    }
                }
            }

            return Vector3.zero;
        }

        ///// <summary>
        ///// Gets the joint velocity for the specified user and joint, in meters/s.
        ///// </summary>
        ///// <returns>The joint velocity.</returns>
        ///// <param name="userId">User ID.</param>
        ///// <param name="joint">Joint index.</param>
        //public Vector3 GetJointVelocity(ulong userId, int joint)
        //{
        //    if (userManager.dictUserIdToIndex.ContainsKey(userId))
        //    {
        //        int index = userManager.dictUserIdToIndex[userId];

        //        if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
        //        {
        //            if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
        //            {
        //                return alTrackedBodies[index].joint[joint].posVel;
        //            }
        //        }
        //    }

        //    return Vector3.zero;
        //}

        /// <summary>
        /// Gets the joint direction of the specified user, relative to its parent joint.
        /// </summary>
        /// <returns>The joint direction.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        /// <param name="flipX">If set to <c>true</c> flips the X-coordinate</param>
        /// <param name="flipZ">If set to <c>true</c> flips the Z-coordinate</param>
        public Vector3 GetJointDirection(ulong userId, int joint, bool flipX, bool flipZ)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
                    {
                        KinectInterop.JointData jointData = alTrackedBodies[index].joint[joint];
                        Vector3 jointDir = jointData.direction;

                        if (flipX)
                            jointDir.x = -jointDir.x;

                        if (flipZ)
                            jointDir.z = -jointDir.z;

                        return jointDir;
                    }
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the direction between the given joints of the specified user.
        /// </summary>
        /// <returns>The direction between joints.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="firstJoint">First joint index</param>
        /// <param name="secondJoint">Second joint index</param>
        /// <param name="flipX">If set to <c>true</c> flips the X-coordinate</param>
        /// <param name="flipZ">If set to <c>true</c> flips the Z-coordinate</param>
        public Vector3 GetDirectionBetweenJoints(ulong userId, int firstJoint, int secondJoint, bool flipX, bool flipZ)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    KinectInterop.BodyData bodyData = alTrackedBodies[index];

                    if (firstJoint >= 0 && firstJoint < (int)KinectInterop.JointType.Count &&
                        secondJoint >= 0 && secondJoint < (int)KinectInterop.JointType.Count)
                    {
                        Vector3 firstJointPos = bodyData.joint[firstJoint].position;
                        Vector3 secondJointPos = bodyData.joint[secondJoint].position;
                        Vector3 jointDir = secondJointPos - firstJointPos;

                        if (flipX)
                            jointDir.x = -jointDir.x;

                        if (flipZ)
                            jointDir.z = -jointDir.z;

                        return jointDir;
                    }
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the joint orientation of the specified user.
        /// </summary>
        /// <returns>The joint rotation.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        /// <param name="flip">If set to <c>true</c>, this means non-mirrored rotation</param>
        public Quaternion GetJointOrientation(ulong userId, int joint, bool flip)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (flip)
                        return alTrackedBodies[index].joint[joint].normalRotation;
                    else
                        return alTrackedBodies[index].joint[joint].mirroredRotation;
                }
            }

            return Quaternion.identity;
        }

        /// <summary>
        /// Gets the angle between bones at the given joint.
        /// </summary>
        /// <returns>The angle at joint.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        public float GetAngleAtJoint(ulong userId, int joint)
        {
            int pjoint = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)joint);
            int njoint = (int)KinectInterop.GetNextJoint((KinectInterop.JointType)joint);

            if (pjoint != joint && njoint != joint)
            {
                Vector3 pos1 = GetJointPosition(userId, pjoint);
                Vector3 pos2 = GetJointPosition(userId, joint);
                Vector3 pos3 = GetJointPosition(userId, njoint);

                if (pos1 != Vector3.zero && pos2 != Vector3.zero && pos3 != Vector3.zero)
                {
                    Vector3 dirP = pos1 - pos2;
                    Vector3 dirN = pos3 - pos2;
                    float fAngle = Vector3.Angle(dirP, dirN);

                    return fAngle;
                }
            }

            return 0f;
        }

        /// <summary>
        /// Gets the user bounding box as min & max points, in space coordinates.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="foregroundCamera">Foreground camera, in case of image overlay</param>
        /// <param name="sensorIndex">Sensor index, in case of image overlay</param>
        /// <param name="backgroundRect">Background rectangle, in case of image overlay</param>
        /// <param name="posMin">Returned min point</param>
        /// <param name="posMax">Returned max point</param>
        /// <returns>true on success, false otherwise</returns>
        public bool GetUserBoundingBox(ulong userId, Camera foregroundCamera, int sensorIndex, Rect backgroundRect, 
            out Vector3 posMin, out Vector3 posMax)
        {
            if(userId == 0 || !IsUserTracked(userId))
            {
                posMin = Vector3.zero;
                posMax = Vector3.zero;
                return false;
            }

            float xMin = float.MaxValue, xMax = float.MinValue;
            float yMin = float.MaxValue, yMax = float.MinValue;
            float zMin = float.MaxValue, zMax = float.MinValue;

            int jCount = GetJointCount();
            for (int j = 0; j < jCount; j++)
            {
                if (IsJointTracked(userId, j))
                {
                    Vector3 jPos = foregroundCamera != null ? 
                        GetJointPosColorOverlay(userId, j, sensorIndex, foregroundCamera, backgroundRect) :
                        GetJointPosition(userId, j);

                    if (jPos.x < xMin) xMin = jPos.x;
                    if (jPos.y < yMin) yMin = jPos.y;
                    if (jPos.z < zMin) zMin = jPos.z;

                    if (jPos.x > xMax) xMax = jPos.x;
                    if (jPos.y > yMax) yMax = jPos.y;
                    if (jPos.z > zMax) zMax = jPos.z;
                }
            }

            posMin = new Vector3(xMin, yMin, zMin);
            posMax = new Vector3(xMax, yMax, zMax);

            bool bSuccess = xMin != float.MaxValue && xMax != float.MinValue &&
                        yMin != float.MaxValue && yMax != float.MinValue &&
                        zMin != float.MaxValue && zMax != float.MinValue;

            return bSuccess;
        }

        /// <summary>
        /// Gets the foreground rectangle of the depth image.
        /// </summary>
        /// <param name="sensorIndex">Sensor index.</param>
        /// <param name="foregroundCamera">The foreground camera, or null if there is no foreground camera.</param>
        /// <returns>The foreground rectangle.</returns>
        public Rect GetForegroundRectDepth(int sensorIndex, Camera foregroundCamera)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData == null)
                return new Rect();

            Rect cameraRect = foregroundCamera ? foregroundCamera.pixelRect : new Rect(0, 0, Screen.width, Screen.height);
            float rectHeight = cameraRect.height;
            float rectWidth = cameraRect.width;

            if (rectWidth > rectHeight)
                rectWidth = rectHeight * sensorData.depthImageWidth / sensorData.depthImageHeight;
            else
                rectHeight = rectWidth * sensorData.depthImageHeight / sensorData.depthImageWidth;

            float foregroundOfsX = (cameraRect.width - rectWidth) / 2;
            float foregroundOfsY = (cameraRect.height - rectHeight) / 2;

            Rect foregroundImgRect = new Rect(foregroundOfsX, foregroundOfsY, rectWidth, rectHeight);

            return foregroundImgRect;
        }

        /// <summary>
        /// Gets the foreground rectangle of the color image..
        /// </summary>
        /// <param name="sensorIndex">Sensor index.</param>
        /// <param name="foregroundCamera">The foreground camera, or null if there is no foreground camera.</param>
        /// <returns>The foreground rectangle.</returns>
        public Rect GetForegroundRectColor(int sensorIndex, Camera foregroundCamera)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);
            if (sensorData == null)
                return new Rect();

            Rect cameraRect = foregroundCamera ? foregroundCamera.pixelRect : new Rect(0, 0, Screen.width, Screen.height);
            float rectHeight = cameraRect.height;
            float rectWidth = cameraRect.width;

            if (rectWidth > rectHeight)
                rectWidth = rectHeight * sensorData.colorImageWidth / sensorData.colorImageHeight;
            else
                rectHeight = rectWidth * sensorData.colorImageHeight / sensorData.colorImageWidth;

            float foregroundOfsX = (cameraRect.width - rectWidth) / 2;
            float foregroundOfsY = (cameraRect.height - rectHeight) / 2;

            Rect foregroundImgRect = new Rect(foregroundOfsX, foregroundOfsY, rectWidth, rectHeight);

            return foregroundImgRect;
        }

        /// <summary>
        /// Gets the 3d overlay position of a point over the depth-image.
        /// </summary>
        /// <returns>The 3d position for depth overlay.</returns>
        /// <param name="dx">Depth image X</param>
        /// <param name="dy">Depth image X</param>
        /// <param name="depth">Distance in mm. If it is 0, the function will try to read the current depth value.</param>
        /// <param name="camera">Camera used to visualize the 3d overlay position</param>
        /// <param name="imageRect">Depth image rectangle on the screen</param>
        public Vector3 GetPosDepthOverlay(int dx, int dy, ushort depth, int sensorIndex, Camera camera, Rect imageRect)
        {
            KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);

            float xScaled = (float)dx * imageRect.width / sensorData.depthImageWidth;
            float yScaled = (float)dy * imageRect.height / sensorData.depthImageHeight;

            float xScreen = imageRect.x + (sensorData.depthImageScale.x > 0f ? xScaled : imageRect.width - xScaled);
            float yScreen = imageRect.y + (sensorData.depthImageScale.y > 0f ? yScaled : imageRect.height - yScaled);

            if(depth == 0)
            {
                depth = sensorData.depthImage[dx + dy * sensorData.depthImageWidth];
            }

            if (depth != 0)
            {
                float zDistance = (float)depth / 1000f;
                Vector3 vPosJoint = camera.ScreenToWorldPoint(new Vector3(xScreen, yScreen, zDistance));

                return vPosJoint;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the 3d overlay position of the given joint over the depth-image.
        /// </summary>
        /// <returns>The joint position for depth overlay.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        /// <param name="camera">Camera used to visualize the 3d overlay position</param>
        /// <param name="imageRect">Depth image rectangle on the screen</param>
        public Vector3 GetJointPosDepthOverlay(ulong userId, int joint, int sensorIndex, Camera camera, Rect imageRect)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId) && camera != null)
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
                    {
                        KinectInterop.JointData jointData = alTrackedBodies[index].joint[joint];
                        Vector3 posJointRaw = jointData.kinectPos;

                        if (posJointRaw != Vector3.zero)
                        {
                            // 3d position to depth
                            Vector2 posDepth = MapSpacePointToDepthCoords(sensorIndex, posJointRaw);

                            if (posDepth != Vector2.zero)
                            {
                                if (!float.IsInfinity(posDepth.x) && !float.IsInfinity(posDepth.y))
                                {
                                    KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);

                                    float xScaled = (float)posDepth.x * imageRect.width / sensorData.depthImageWidth;
                                    float yScaled = (float)posDepth.y * imageRect.height / sensorData.depthImageHeight;

                                    float xScreen = imageRect.x + (sensorData.depthImageScale.x > 0f ? xScaled : imageRect.width - xScaled);
                                    //float yScreen = camera.pixelHeight - (imageRect.y + yScaled);
                                    float yScreen = imageRect.y + (sensorData.depthImageScale.y > 0f ? yScaled : imageRect.height - yScaled);

                                    Plane cameraPlane = new Plane(camera.transform.forward, camera.transform.position);
                                    float zDistance = cameraPlane.GetDistanceToPoint(posJointRaw);

                                    Vector3 vPosJoint = camera.ScreenToWorldPoint(new Vector3(xScreen, yScreen, zDistance));

                                    return vPosJoint;
                                }
                            }
                        }
                    }
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the 3d overlay position of the given joint over the color-image.
        /// </summary>
        /// <returns>The joint position for color overlay.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        /// <param name="camera">Camera used to visualize the 3d overlay position</param>
        /// <param name="imageRect">Color image rectangle on the screen</param>
        public Vector3 GetJointPosColorOverlay(ulong userId, int joint, int sensorIndex, Camera camera, Rect imageRect)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId) && camera != null)
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
                    {
                        KinectInterop.JointData jointData = alTrackedBodies[index].joint[joint];
                        Vector3 posJointRaw = jointData.kinectPos;

                        if (posJointRaw != Vector3.zero)
                        {
                            // 3d position to depth
                            Vector2 posDepth = MapSpacePointToDepthCoords(sensorIndex, posJointRaw);
                            ushort depthValue = GetDepthForPixel(sensorIndex, (int)posDepth.x, (int)posDepth.y);

                            if (posDepth != Vector2.zero && depthValue > 0)
                            {
                                // depth pos to color pos
                                Vector2 posColor = MapDepthPointToColorCoords(sensorIndex, posDepth, depthValue);
                                //Vector2 posDepth2 = MapColorPointToDepthCoords(sensorIndex, posColor);

                                if (posColor.x != 0f && !float.IsInfinity(posColor.x))
                                {
                                    KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);

                                    float xScaled = (float)posColor.x * imageRect.width / sensorData.colorImageWidth;
                                    float yScaled = (float)posColor.y * imageRect.height / sensorData.colorImageHeight;

                                    float xScreen = imageRect.x + (sensorData.colorImageScale.x > 0f ? xScaled : imageRect.width - xScaled);
                                    //float yScreen = camera.pixelHeight - (imageRect.y + yScaled);
                                    float yScreen = imageRect.y + (sensorData.colorImageScale.y > 0f ? yScaled : imageRect.height - yScaled);

                                    //Plane cameraPlane = new Plane(camera.transform.forward, camera.transform.position);
                                    //float zDistance = cameraPlane.GetDistanceToPoint(posJointRaw);
                                    ////float zDistance = (posJointRaw - camera.transform.position).magnitude;
                                    float zDistance = posJointRaw.z;

                                    //Vector3 vPosJoint = camera.ViewportToWorldPoint(new Vector3(xNorm, yNorm, zDistance));
                                    Vector3 vPosJoint = camera.ScreenToWorldPoint(new Vector3(xScreen, yScreen, zDistance));

                                    return vPosJoint;
                                }
                            }
                        }
                    }
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the 2d overlay position of the given joint over the given image.
        /// </summary>
        /// <returns>The 2d joint position for color overlay.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        /// <param name="imageRect">Color image rectangle on the screen</param>
        public Vector2 GetJointPosColorOverlay(ulong userId, int joint, int sensorIndex, Rect imageRect)
        {
            if (userManager.dictUserIdToIndex.ContainsKey(userId))
            {
                int index = userManager.dictUserIdToIndex[userId];

                if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                {
                    if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
                    {
                        KinectInterop.JointData jointData = alTrackedBodies[index].joint[joint];
                        Vector3 posJointRaw = jointData.kinectPos;

                        if (posJointRaw != Vector3.zero)
                        {
                            // 3d position to depth
                            Vector2 posDepth = MapSpacePointToDepthCoords(sensorIndex, posJointRaw);
                            ushort depthValue = GetDepthForPixel(sensorIndex, (int)posDepth.x, (int)posDepth.y);

                            if (posDepth != Vector2.zero && depthValue > 0)
                            {
                                // depth pos to color pos
                                Vector2 posColor = MapDepthPointToColorCoords(sensorIndex, posDepth, depthValue);

                                if (posColor.x != 0f && !float.IsInfinity(posColor.x))
                                {
                                    KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);

                                    float xScaled = (float)posColor.x * imageRect.width / sensorData.colorImageWidth;
                                    float yScaled = (float)posColor.y * imageRect.height / sensorData.colorImageHeight;

                                    float xImage = imageRect.x + (sensorData.colorImageScale.x > 0f ? xScaled : imageRect.width - xScaled);
                                    float yImage = imageRect.y + (sensorData.colorImageScale.y > 0f ? yScaled : imageRect.height - yScaled);

                                    return new Vector2(xImage, yImage);
                                }
                            }
                        }
                    }
                }
            }

            return Vector2.zero;
        }

        /// <summary>
        /// Gets the joint position on the depth map texture.
        /// </summary>
        /// <returns>The joint position in texture coordinates.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        public Vector2 GetJointDepthMapPos(ulong userId, int joint, int sensorIndex)
        {
            Vector2 posDepth = Vector2.zero;

            Vector3 posJointRaw = GetJointKinectPosition(userId, joint, false);
            if (posJointRaw != Vector3.zero)
            {
                posDepth = MapSpacePointToDepthCoords(sensorIndex, posJointRaw);

                if (posDepth != Vector2.zero)
                {
                    KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);

                    float xScaled = (float)posDepth.x / sensorData.depthImageWidth;
                    float yScaled = (float)posDepth.y / sensorData.depthImageHeight;

                    float xImage = sensorData.depthImageScale.x > 0f ? xScaled : 1f - xScaled;
                    float yImage = sensorData.depthImageScale.y > 0f ? yScaled : 1f - yScaled;

                    posDepth = new Vector2(xImage, yImage);
                }
            }

            return posDepth;
        }

        /// <summary>
        /// Gets the joint position on the color map texture.
        /// </summary>
        /// <returns>The joint position in texture coordinates.</returns>
        /// <param name="userId">User ID</param>
        /// <param name="joint">Joint index</param>
        public Vector2 GetJointColorMapPos(ulong userId, int joint, int sensorIndex)
        {
            Vector2 posColor = Vector2.zero;

            Vector3 posJointRaw = GetJointKinectPosition(userId, joint, false);
            if (posJointRaw != Vector3.zero)
            {
                // 3d position to depth
                Vector2 posDepth = MapSpacePointToDepthCoords(sensorIndex, posJointRaw);
                ushort depthValue = GetDepthForPixel(sensorIndex, (int)posDepth.x, (int)posDepth.y);

                if (posDepth != Vector2.zero && depthValue > 0)
                {
                    // depth pos to color pos
                    posColor = MapDepthPointToColorCoords(sensorIndex, posDepth, depthValue);

                    if (posColor.x != 0f && !float.IsInfinity(posColor.x))
                    {
                        KinectInterop.SensorData sensorData = GetSensorData(sensorIndex);

                        float xScaled = (float)posColor.x / sensorData.colorImageWidth;
                        float yScaled = (float)posColor.y / sensorData.colorImageHeight;

                        float xImage = sensorData.colorImageScale.x > 0f ? xScaled : 1f - xScaled;
                        float yImage = sensorData.colorImageScale.y > 0f ? yScaled : 1f - yScaled;

                        posColor = new Vector2(xScaled, 1f - yScaled);
                    }
                    else
                    {
                        posColor = Vector2.zero;
                    }
                }
            }

            return posColor;
        }


        /// <summary>
        /// Returns array of colors, one for each body index.
        /// </summary>
        /// <returns>Array of body index colors.</returns>
        public Color[] GetBodyIndexColors()
        {
            int numUserIndices = userManager.aUserIndexIds.Length;

            for (int i = 0; i < numUserIndices; i++)
            {
                ulong userId = userManager.aUserIndexIds[i];

                if (userId != 0)
                {
                    //Debug.Log("BI-Colors - UserId: " + userId);
                    int index = userManager.dictUserIdToIndex[userId];
                    if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
                    {
                        int bi = alTrackedBodies[index].iBodyIndex;
                        clrUsers[bi] = (i == 0) ? Color.yellow : _bodyIndexColors[i % _bodyIndexColors.Length];
                        //Debug.Log(string.Format("{0} - id: {1}, bi: {2}, clr: {3}", i, userId, bi, clrUsers[bi]));
                    }
                }
            }

            return clrUsers;
        }

        // user colors
        private static readonly Color[] _bodyIndexColors = { Color.red, Color.green, Color.blue, Color.magenta };
        private Color[] clrUsers = new Color[KinectInterop.Constants.MaxBodyCount];


        /// <summary>
        /// Resets the joint data filters.
        /// </summary>
        public void ResetJointFilters()
        {
            //if (jointPositionFilter != null)
            //{
            //    jointPositionFilter.Reset();
            //}

            //if (jointVelocityFilter != null)
            //{
            //    jointVelocityFilter.Reset();
            //}
        }


        /// <summary>
        /// Removes all currently detected users, allowing new user-detection process to start.
        /// </summary>
        public void ClearKinectUsers()
        {
            if (!kinectInitialized)
                return;

            // remove current users
            for (int i = userManager.alUserIds.Count - 1; i >= 0; i--)
            {
                ulong userId = userManager.alUserIds[i];
                RemoveUser(userId);
            }

            ResetJointFilters();
        }


        /// <summary>
        /// Gets the body frame as one csv line, or returns empty string if there is no new body frame.
        /// </summary>
        /// <returns>The body frame as a csv line.</returns>
        /// <param name="liRelTime">Reference to variable, used to compare frame times.</param>
        /// <param name="fUnityTime">Reference to variable, used to save the current Unity time.</param>
        public string GetBodyFrameData(ref float fUnityTime, char delimiter)
        {
            Vector3 spaceScale = GetSensorSpaceScale(btSensorIndex);
            return KinectInterop.GetBodyFrameAsCsv(ref alTrackedBodies, trackedBodiesCount, lastBodyFrameTime, spaceScale, ref fUnityTime, delimiter);
        }


        /// <summary>
        /// Determines whether the play mode is enabled or not.
        /// </summary>
        /// <returns><c>true</c> if the play mode is enabled; otherwise, <c>false</c>.</returns>
        public bool IsPlayModeEnabled()
        {
            return isPlayModeEnabled;
        }


        /// <summary>
        /// Enables or displables the play mode.
        /// </summary>
        /// <param name="bEnabled">If set to <c>true</c> enables the play mode.</param>
        public void EnablePlayMode(bool bEnabled)
        {
            isPlayModeEnabled = bEnabled;
            playModeData = string.Empty;
        }

        /// <summary>
        /// Sets the body frame from the given csv line.
        /// </summary>
        /// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
        /// <param name="sLine">The body frame as csv line.</param>
        public bool SetBodyFrameData(string sLine)
        {
            if (isPlayModeEnabled)
            {
                playModeData = sLine;
                return true;
            }

            return false;
        }

        // internal methods

        void Awake()
        {
            // initializes the singleton instance of KinectManager
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else if (instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            // set graphics shader level
            KinectInterop.SetGraphicsShaderLevel(SystemInfo.graphicsShaderLevel);

            // user manager by default 
            if (userManager == null)
            {
                userManager = gameObject.GetComponent<KinectUserManager>();

                if(userManager == null)
                {
                    userManager = gameObject.AddComponent<KinectUserManager>();
                }
            }

            // gesture manager by default
            if (gestureManager == null)
            {
                gestureManager = gameObject.GetComponent<KinectGestureManager>();

                if(gestureManager == null)
                {
                    gestureManager = gameObject.AddComponent<KinectGestureManager>();
                }
            }

            // bone orientation constraints
            if (boneOrientationConstraints)
            {
                boneConstraints = new BoneOrientationConstraints();
                boneConstraints.AddDefaultConstraints();
                boneConstraints.SetDebugText(statusInfoText);
            }

            // locate and start the available depth-sensors
            StartDepthSensors();
        }


        // gets the frame-source flags
        private KinectInterop.FrameSource GetFrameSourceFlags()
        {
            KinectInterop.FrameSource dwFlags = KinectInterop.FrameSource.TypeNone;

            if (getDepthFrames != DepthTextureType.None)
                dwFlags |= KinectInterop.FrameSource.TypeDepth;
            if (getColorFrames != ColorTextureType.None)
                dwFlags |= KinectInterop.FrameSource.TypeColor;
            if (getInfraredFrames != InfraredTextureType.None)
                dwFlags |= KinectInterop.FrameSource.TypeInfrared;
            if (getPoseFrames != PoseUsageType.None)
                dwFlags |= KinectInterop.FrameSource.TypePose;
            if (getBodyFrames != BodyTextureType.None)
                dwFlags |= (KinectInterop.FrameSource.TypeBody | KinectInterop.FrameSource.TypeBodyIndex);

            return dwFlags;
        }


        // locates and starts the available depth-sensors and their interfaces
        private void StartDepthSensors()
        {
            try
            {
                // try to initialize the available sensors
                KinectInterop.FrameSource dwFlags = GetFrameSourceFlags();

                // locate the available depth-sensor interfaces in the scene
                List<DepthSensorBase> sensorInts = new List<DepthSensorBase>();
                sensorInts.AddRange(gameObject.GetComponents<DepthSensorBase>());  // FindObjectsOfType<MonoBehaviour>();
                sensorInts.AddRange(gameObject.GetComponentsInChildren<DepthSensorBase>());

                if (sensorInts.Count == 0)
                {
                    // by-default add K4A interface
                    transform.position = new Vector3(0f, 1f, 0f);
                    transform.rotation = Quaternion.identity;

                    DepthSensorBase sensorInt = gameObject.AddComponent<Kinect4AzureInterface>();
                    sensorInts.Add(sensorInt);
                }

                for (int i = 0; i < sensorInts.Count; i++)
                {
                    if (sensorInts[i] is DepthSensorBase)
                    {
                        DepthSensorBase sensorInt = (DepthSensorBase)sensorInts[i];
                        if(!sensorInt.enabled || sensorInt.deviceStreamingMode == KinectInterop.DeviceStreamingMode.Disabled || sensorInt.deviceIndex < 0)
                        {
                            Debug.Log(string.Format("S{0}: {1} disabled.", i, sensorInt.GetType().Name));
                            continue;
                        }

                        try
                        {
                            Debug.Log(string.Format("Opening S{0}: {1}, device-index: {2}", i, sensorInt.GetType().Name, sensorInt.deviceIndex));
                            KinectInterop.SensorData sensorData = sensorInt.OpenSensor(dwFlags, syncDepthAndColor, syncBodyAndDepth);

                            if(sensorData != null)
                            {
                                //Debug.Log("Succeeded opening " + sensorInt.GetType().Name);

                                sensorData.sensorInterface = sensorInt;
                                KinectInterop.InitSensorData(sensorData, this);

                                sensorInterfaces.Add(sensorInt);
                                sensorDatas.Add(sensorData);

                                if(pollFramesInThread)
                                {
                                    sensorData.threadStopEvent = new AutoResetEvent(false);
                                    sensorData.pollFramesThread = new Thread(() => PollFramesThread(sensorData));
                                    sensorData.pollFramesThread.Name = sensorInt.GetType().Name + sensorInt.deviceIndex;
                                    sensorData.pollFramesThread.IsBackground = true;
                                    sensorData.pollFramesThread.Start();
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Debug.LogException(ex);
                            Debug.LogError("Failed opening " + sensorInt.GetType().Name + ", device-index: " + sensorInt.deviceIndex);
                        }
                    }
                }

                Debug.Log(string.Format("{0} sensor(s) opened.", sensorDatas.Count));

                // set initialization status
                if (sensorInterfaces.Count > 0)
                {
                    kinectInitialized = true;
                }
                else
                {
                    kinectInitialized = false;

                    string sErrorMessage = "No suitable depth-sensor found. Please check the connected devices and installed SDKs.";
                    Debug.LogError(sErrorMessage);

                    if (statusInfoText != null)
                    {
                        statusInfoText.text = sErrorMessage;
                    }
                }
            }
            //catch (DllNotFoundException ex)
            //{
            //    string message = ex.Message + " cannot be loaded. Please check the respective SDK installation.";

            //    Debug.LogError(message);
            //    Debug.LogException(ex);

            //    if (calibrationText != null)
            //    {
            //        calibrationText.text = message;
            //    }

            //    return;
            //}
            catch (Exception ex)
            {
                string message = ex.Message;

                Debug.LogError(message);
                Debug.LogException(ex);

                if (statusInfoText != null)
                {
                    statusInfoText.text = message;
                }

                return;
            }
        }


        // polls for frames and updates the depth-sensor data in a thread
        private void PollFramesThread(KinectInterop.SensorData sensorData)
        {
            if (sensorData == null)
                return;

            while (!sensorData.threadStopEvent.WaitOne(0))
            {
                if (kinectInitialized)
                {
                    KinectInterop.PollSensorFrames(sensorData);
                }
            }
        }


        void OnApplicationQuit()
        {
            OnDestroy();
        }


        void OnDestroy()
        {
            if (instance == null || instance != this)
                return;

            //Debug.Log("KM was destroyed");

            // shut down the polling threads and stop the sensors
            if (kinectInitialized)
            {
                // close the opened sensors and release respective data
                for(int i = sensorDatas.Count - 1; i >= 0; i--)
                {
                    KinectInterop.SensorData sensorData = sensorDatas[i];
                    DepthSensorInterface sensorInt = sensorData.sensorInterface;
                    Debug.Log(string.Format("Closing S{0}: {1}", i, sensorInt.GetType().Name));

                    if (sensorData.pollFramesThread != null)
                    {
                        //Debug.Log("Stopping thread: " + sensorData.pollFramesThread.Name);

                        // stop the frame-polling thread
                        sensorData.threadStopEvent.Set();
                        sensorData.pollFramesThread.Join();

                        sensorData.pollFramesThread = null;
                        sensorData.threadStopEvent.Dispose();
                        sensorData.threadStopEvent = null;

                        //Debug.Log("Thread stopped.");
                    }

                    // close the sensor
                    KinectInterop.CloseSensor(sensorData);
                    //Debug.Log("Sensor closed.");

                    sensorDatas.RemoveAt(i);
                    sensorInterfaces.RemoveAt(i);
                }

                kinectInitialized = false;
            }

            instance = null;
        }


        void Update()
        {
            if (!kinectInitialized)
                return;

            if (!pollFramesInThread)
            {
                for (int i = 0; i < sensorDatas.Count; i++)
                {
                    KinectInterop.SensorData sensorData = sensorDatas[i];
                    KinectInterop.PollSensorFrames(sensorData);
                }
            }

            // update the sensor data, as needed
            for (int i = 0; i < sensorDatas.Count; i++)
            {
                KinectInterop.SensorData sensorData = sensorDatas[i];
                if(KinectInterop.UpdateSensorData(sensorData, this, isPlayModeEnabled))
                {
                    UpdateTrackedBodies(i, sensorData);
                }
            }

            if(!isPlayModeEnabled)
            {
                // update the sensor textures, if needed
                for (int i = 0; i < sensorDatas.Count; i++)
                {
                    KinectInterop.UpdateSensorTextures(sensorDatas[i], this);
                }
            }
        }


        void OnGUI()
        {
            if (!kinectInitialized)
                return;

            // display the selected images on screen
            for (int i = 0; i < displayImages.Count; i++)
            {
                Vector2 imageScale = Vector3.one;
                Texture imageTex = null;

                DisplayImageType imageType = displayImages[i];
                switch (imageType)
                {
                    case DisplayImageType.None:
                        break;

                    case DisplayImageType.Sensor0ColorImage:
                    case DisplayImageType.Sensor1ColorImage:
                    case DisplayImageType.Sensor2ColorImage:
                        int si = imageType == DisplayImageType.Sensor0ColorImage ? 0 : imageType == DisplayImageType.Sensor1ColorImage ? 1 : imageType == DisplayImageType.Sensor2ColorImage ? 2 : -1;
                        if (si >= 0 && si < sensorDatas.Count)
                        {
                            KinectInterop.SensorData sensorData = sensorDatas[si];
                            imageScale = sensorData.colorImageScale;
                            imageTex = sensorData.colorImageTexture;
                        }
                        break;

                    case DisplayImageType.Sensor0DepthImage:
                    case DisplayImageType.Sensor1DepthImage:
                    case DisplayImageType.Sensor2DepthImage:
                        si = imageType == DisplayImageType.Sensor0DepthImage ? 0 : imageType == DisplayImageType.Sensor1DepthImage ? 1 : imageType == DisplayImageType.Sensor2DepthImage ? 2 : -1;
                        if (si >= 0 && si < sensorDatas.Count)
                        {
                            KinectInterop.SensorData sensorData = sensorDatas[si];
                            imageScale = sensorData.depthImageScale;
                            imageTex = sensorData.depthImageTexture;
                        }
                        break;

                    case DisplayImageType.Sensor0InfraredImage:
                    case DisplayImageType.Sensor1InfraredImage:
                    case DisplayImageType.Sensor2InfraredImage:
                        si = imageType == DisplayImageType.Sensor0InfraredImage ? 0 : imageType == DisplayImageType.Sensor1InfraredImage ? 1 : imageType == DisplayImageType.Sensor2InfraredImage ? 2 : -1;
                        if (si >= 0 && si < sensorDatas.Count)
                        {
                            KinectInterop.SensorData sensorData = sensorDatas[si];
                            imageScale = sensorData.infraredImageScale;
                            imageTex = sensorData.infraredImageTexture;
                        }
                        break;

                    case DisplayImageType.UserBodyImage:
                        si = 0;  // sensor 0
                        if (si >= 0 && si < sensorDatas.Count)
                        {
                            KinectInterop.SensorData sensorData = sensorDatas[si];
                            imageScale = sensorData.depthImageScale;
                            imageTex = sensorData.bodyImageTexture;
                        }
                        break;
                }

                // display the image on screen
                if(imageTex != null)
                {
                    KinectInterop.DisplayGuiTexture(i, displayImageWidthPercent, imageScale, imageTex);
                }
            }
        }


        // updates the global list of tracked bodies
        protected void UpdateTrackedBodies(int sensorIndex, KinectInterop.SensorData sensorData)
        {
            if(isPlayModeEnabled)
            {
                if(!string.IsNullOrEmpty(playModeData))
                {
                    // use the 1st sensor
                    Matrix4x4 sensorToWorld = GetSensorToWorldMatrix(0);

                    trackedBodiesCount = KinectInterop.SetBodyFrameFromCsv(playModeData, ";", ref alTrackedBodies, ref sensorToWorld, 
                        ignoreZCoordinates, out lastBodyFrameTime);
                    playModeData = string.Empty;
                }
            }
            else if(sensorIndex == 0 && lastBodyFrameTime != sensorData.lastBodyFrameTime)
            {
                btSensorIndex = sensorIndex;
                lastBodyFrameTime = sensorData.lastBodyFrameTime;

                // take the tracked bodies from sensor 0
                trackedBodiesCount = sensorData.trackedBodiesCount;

                if (alTrackedBodies.Length < trackedBodiesCount)
                {
                    //alTrackedBodies.Add(new KinectInterop.BodyData((int)KinectInterop.JointType.Count));
                    Array.Resize<KinectInterop.BodyData>(ref alTrackedBodies, (int)trackedBodiesCount);

                    for (int i = 0; i < trackedBodiesCount; i++)
                    {
                        alTrackedBodies[i] = new KinectInterop.BodyData((int)KinectInterop.JointType.Count);
                    }
                }

                for (int i = 0; i < trackedBodiesCount; i++)
                {
                    //alTrackedBodies[i] = sensorData.alTrackedBodies[i];
                    sensorData.alTrackedBodies[i].CopyTo(ref alTrackedBodies[i]);

                    // filter orientation constraints
                    if (boneOrientationConstraints && boneConstraints != null)
                    {
                        boneConstraints.Constrain(ref alTrackedBodies[i]);
                    }
                }
            }
            else
            {
                return;
            }

            // process the tracked bodies
            ProcessTrackedBodies();

            //// set first user index
            //sensorData.firstUserIndex = (userManager.liPrimaryUserId != 0 && userManager.dictUserIdToIndex.ContainsKey(userManager.liPrimaryUserId) ?
            //    userManager.dictUserIdToIndex[userManager.liPrimaryUserId] : 255);

            //if (userManager.liPrimaryUserId != 0)
            //    Debug.Log("liPrimaryUserId: " + userManager.liPrimaryUserId + ", index: " + sensorData.firstUserIndex);

            // update user gestures
            foreach (ulong userId in userManager.alUserIds)
            {
                gestureManager.UpdateUserGestures(userId, this);
            }
        }


        // processes the tracked bodies
        private void ProcessTrackedBodies()
        {
            List<ulong> addedUsers = new List<ulong>();
            List<int> addedIndexes = new List<int>();

            List<ulong> lostUsers = new List<ulong>();
            lostUsers.AddRange(userManager.alUserIds);

            bLimitedUsers = showAllowedUsersOnly && 
                (maxTrackedUsers > 0 || minUserDistance >= 0.01f || maxUserDistance >= 0.01f || maxLeftRightDistance >= 0.01f);

            for (int i = 0; i < trackedBodiesCount; i++)
            {
                KinectInterop.BodyData bodyData = alTrackedBodies[i];
                ulong userId = bodyData.liTrackingID;

                //Debug.Log("  (M)User ID: " + userId + ", body: " + i + ", pos: " + bodyData.kinectPos);

                if (bodyData.bIsTracked && userId != 0 && Mathf.Abs(bodyData.position.z) >= minUserDistance &&
                   (maxUserDistance < 0.01f || Mathf.Abs(bodyData.position.z) <= maxUserDistance) &&
                   (maxLeftRightDistance < 0.01f || Mathf.Abs(bodyData.position.x) <= maxLeftRightDistance))
                {
                    // add userId to the list of new users
                    if (!addedUsers.Contains(userId))
                    {
                        addedUsers.Add(userId);
                        addedIndexes.Add(i);
                    }

                    lostUsers.Remove(userId);
                    userManager.dictUserIdToTime[userId] = Time.time;
                }
                else
                {
                    // consider body as not tracked
                    bodyData.bIsTracked = false;
                    alTrackedBodies[i] = bodyData;
                }
            }

            // remove the lost users, if any
            if (lostUsers.Count > 0)
            {
                foreach (ulong userId in lostUsers)
                {
                    // prevent user removal upon sporadical tracking failures
                    if ((Time.time - userManager.dictUserIdToTime[userId]) >= waitTimeBeforeRemove)
                    {
                        RemoveUser(userId);
                    }
                }

                lostUsers.Clear();
            }

            if (addedUsers.Count > 0)
            {
                // calibrate the newly detected users
                for (int i = 0; i < addedUsers.Count; i++)
                {
                    ulong userId = addedUsers[i];
                    int userIndex = addedIndexes[i];

                    if (!userManager.alUserIds.Contains(userId))
                    {
                        if (maxTrackedUsers == 0 || userManager.alUserIds.Count < maxTrackedUsers)
                        {
                            CalibrateUser(userId, userIndex);
                        }
                    }
                }

                // update body indices, as needed
                for (int i = 0; i < addedUsers.Count; i++)
                {
                    ulong userId = addedUsers[i];
                    int userIndex = addedIndexes[i];

                    int bi = userManager.dictUserIdToIndex.ContainsKey(userId) ? userManager.dictUserIdToIndex[userId] : -1;
                    if (bi >= 0 && bi != userIndex)
                    {
                        // update body index if needed
                        userManager.dictUserIdToIndex[userId] = userIndex;

                        int uidIndex = Array.IndexOf(userManager.aUserIndexIds, userId);
                        Debug.Log("Updating user " + uidIndex + ", ID: " + userId + ", Body: " + userIndex + ", Time: " + Time.time);
                    }
                }

                addedUsers.Clear();
                addedIndexes.Clear();
            }
        }


        // Adds UserId to the list of users
        private void CalibrateUser(ulong userId, int bodyIndex)
        {
            int uidIndex = userManager.CalibrateUser(userId, bodyIndex, ref alTrackedBodies, userDetectionOrder, playerCalibrationPose, gestureManager);

            if (uidIndex >= 0)
            {
                Debug.Log("Adding user " + uidIndex + ", ID: " + userId + ", Body: " + bodyIndex + ", Time: " + userManager.dictUserIdToTime[userId]);

                // update userIds of the avatar controllers
                //RefreshAvatarUserIds();

                // notify the gesture manager for the newly detected user
                gestureManager.UserWasAdded(userId, uidIndex);

                // reset filters
                ResetJointFilters();

                // fire event
                userManager.FireOnUserAdded(userId, uidIndex);
            }
        }


        // Remove a lost UserId
        private void RemoveUser(ulong userId)
        {
            int bodyIndex = userManager.dictUserIdToIndex[userId];
            int uidIndex = userManager.RemoveUser(userId, userDetectionOrder);

            if(uidIndex >= 0)
            {
                Debug.Log("Removing user " + uidIndex + ", ID: " + userId + ", Body: " + bodyIndex + ", Time: " + Time.time);

                // clear gestures list for this user
                gestureManager.UserWasRemoved(userId, uidIndex);

                // update userIds of the avatar controllers
                //RefreshAvatarUserIds();

                // fire event
                userManager.FireOnUserRemoved(userId, uidIndex);
            }
        }

    }
}
