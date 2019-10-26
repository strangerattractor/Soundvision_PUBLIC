using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using System;

namespace com.rfilkov.kinect
{
    public class Kinect4AzureInterface : DepthSensorBase
    {
        [Tooltip("Color camera resolution.")]
        public ColorCameraMode colorCameraMode = ColorCameraMode._1920_x_1080_30Fps;
        public enum ColorCameraMode : int { _1280_x_720_30Fps = 1, _1920_x_1080_30Fps = 2, _2560_x_1440_30Fps = 3, _2048_x_1536_30Fps = 4, _3840_x_2160_30Fps = 5, _4096_x_3072_15Fps = 6 }

        [Tooltip("Depth camera mode.")]
        public DepthCameraMode depthCameraMode = DepthCameraMode._640_x_576_30Fps_3_86mNfov;
        public enum DepthCameraMode : int { _320_x_288_30Fps_5_46mNfov = 1, _640_x_576_30Fps_3_86mNfov = 2, _512_x_512_30Fps_2_88mWfov = 3, _1024x1024_15Fps_2_21mWfov = 4, PassiveIR_30Fps = 5 }

        [Tooltip("Device sync mode, in case of multiple wired sensors.")]
        public WiredSyncMode deviceSyncMode = WiredSyncMode.Standalone;

        [Tooltip("Subordinate device delay off master (in usec), in case of multiple wired sensors.")]
        public int subDeviceDelayUsec = 0;

        [Tooltip("Whether to flip the IMU rotation.")]
        public bool flipImuRotation = false;


        // references to the sensor
        public Device kinectSensor = null;
        public Calibration coordMapper;
        public Transformation coordMapperTransform = null;
        private Image d2cColorData = null;
        private Image c2dDepthData = null;

        // playback and record
        public Playback kinectPlayback = null;
        private long playbackStartTime = 0;

        // status of the cameras
        private bool isCamerasStarted = false;
        private bool isImuStarted = false;

        // current frame number
        private ulong currentFrameNumber = 0;
        private TimeSpan timeToWait = TimeSpan.FromMilliseconds(0);

        // imu
        private ImuSample lastImuSample = null;
        private ImuSample curImuSample = null;

        private AHRS.MahonyAHRS imuAhrs = null;
        private Quaternion imuTurnRot1 = Quaternion.identity;
        private Quaternion imuTurnRot2 = Quaternion.identity;
        //private bool lastFlipImuRot = false;

        private Quaternion imuRotation = Quaternion.identity;
        private float prevRotZ = 0f;
        private float[] prevQuat = new float[4];

        private Vector3 imuYawAtStart = Vector3.zero;
        private Quaternion imuFixAtStart = Quaternion.identity;
        private bool imuYawAtStartSet = false;

        // TODO: to be removed later
        //public event Action<ImuSample, ulong, ulong> OnImuSample;



        public override KinectInterop.DepthSensorPlatform GetSensorPlatform()
        {
            return KinectInterop.DepthSensorPlatform.Kinect4Azure;
        }

        public override List<KinectInterop.SensorDeviceInfo> GetAvailableSensors()
        {
            List<KinectInterop.SensorDeviceInfo> alSensorInfo = new List<KinectInterop.SensorDeviceInfo>();

            int deviceCount = Device.GetInstalledCount();
            for (int i = 0; i < deviceCount; i++)
            {
                KinectInterop.SensorDeviceInfo sensorInfo = new KinectInterop.SensorDeviceInfo();
                sensorInfo.sensorId = i.ToString();
                sensorInfo.sensorName = "Kinect4Azure";

                sensorInfo.sensorCaps = KinectInterop.FrameSource.TypeAll;

                Debug.Log(string.Format("  D{0}: {1}, id: {2}", i, sensorInfo.sensorName, sensorInfo.sensorId));

                alSensorInfo.Add(sensorInfo);
            }

            //if (alSensorInfo.Count == 0)
            //{
            //    Debug.Log("  No sensor devices found.");
            //}

            return alSensorInfo;
        }

        public override KinectInterop.SensorData OpenSensor(KinectInterop.FrameSource dwFlags, bool bSyncDepthAndColor, bool bSyncBodyAndDepth)
        {
            // save initial parameters
            base.OpenSensor(dwFlags, bSyncDepthAndColor, bSyncBodyAndDepth);

            // try to open the sensor or play back the recording
            KinectInterop.SensorData sensorData = new KinectInterop.SensorData();

            if (deviceStreamingMode == KinectInterop.DeviceStreamingMode.PlayRecording)
            {
                if (string.IsNullOrEmpty(recordingFile))
                {
                    Debug.LogError("Playback selected, but the path to recording file is missing.");
                    return null;
                }

                if (!System.IO.File.Exists(recordingFile))
                {
                    Debug.LogError("PlayRecording selected, but the recording file cannot be found: " + recordingFile);
                    return null;
                }

                Debug.Log("Playing back: " + recordingFile);
                kinectPlayback = new Playback(recordingFile);

                colorCameraMode = (ColorCameraMode)kinectPlayback.playback_config.color_resolution;
                depthCameraMode = (DepthCameraMode)kinectPlayback.playback_config.depth_mode;
                deviceSyncMode = kinectPlayback.playback_config.wired_sync_mode;

                coordMapper = kinectPlayback.playback_calibration;
                playbackStartTime = 0;

                Debug.Log(string.Format("color_track_enabled: {0}, depth_track_enabled: {1}, ir_track_enabled: {2}, imu_track_enabled: {3}, depth_delay_off_color_usec: {4}",
                    kinectPlayback.playback_config.color_track_enabled, kinectPlayback.playback_config.depth_track_enabled,
                    kinectPlayback.playback_config.ir_track_enabled, kinectPlayback.playback_config.imu_track_enabled,
                    kinectPlayback.playback_config.depth_delay_off_color_usec));
            }
            else
            {
                List<KinectInterop.SensorDeviceInfo> alSensors = GetAvailableSensors();
                if (deviceIndex >= alSensors.Count)
                {
                    Debug.Log("  D" + deviceIndex + " is not available. You can set the device index to -1, to disable it.");
                    return null;
                }

                // try to open the sensor
                kinectSensor = Device.Open(deviceIndex);
                if (kinectSensor == null)
                {
                    Debug.LogError("  D" + recordingFile + " cannot be opened.");
                    return null;
                }

                if(deviceSyncMode == WiredSyncMode.Master && (dwFlags & KinectInterop.FrameSource.TypeColor) == 0)
                {
                    // fix by Andreas Pedroni - master requires color camera on
                    dwFlags |= KinectInterop.FrameSource.TypeColor;
                }

                DeviceConfiguration kinectConfig = new DeviceConfiguration();
                kinectConfig.SynchronizedImagesOnly = isSyncDepthAndColor;
                kinectConfig.WiredSyncMode = deviceSyncMode;
                kinectConfig.SuboridinateDelayOffMaster = new TimeSpan(subDeviceDelayUsec * 10);

                // color
                kinectConfig.ColorFormat = ImageFormat.ColorBGRA32;
                if ((dwFlags & KinectInterop.FrameSource.TypeColor) != 0)
                {
                    kinectConfig.ColorResolution = (ColorResolution)colorCameraMode;
                }
                else
                {
                    kinectConfig.ColorResolution = ColorResolution.Off;
                }

                // depth
                if ((dwFlags & KinectInterop.FrameSource.TypeDepth) != 0)
                {
                    kinectConfig.DepthMode = (DepthMode)depthCameraMode;
                }
                else
                {
                    kinectConfig.DepthMode = DepthMode.Off;
                }

                // fps
                if(colorCameraMode != ColorCameraMode._4096_x_3072_15Fps && depthCameraMode != DepthCameraMode._1024x1024_15Fps_2_21mWfov)
                {
                    kinectConfig.CameraFPS = FPS.FPS30;
                }
                else
                {
                    kinectConfig.CameraFPS = FPS.FPS15;
                }

                // infrared
                if ((dwFlags & KinectInterop.FrameSource.TypeInfrared) != 0)
                {
                    // ??
                }

                // start the cameras
                kinectSensor.StartCameras(kinectConfig);
                isCamerasStarted = true;

                if ((dwFlags & KinectInterop.FrameSource.TypePose) != 0)
                {
                    // start the IMU
                    kinectSensor.StartImu();
                    isImuStarted = true;
                }

                // get reference to the coordinate mapper
                coordMapper = kinectSensor.GetCalibration();
            }

            // reset the frame number
            currentFrameNumber = 0;

            // flip color & depth image vertically
            sensorData.colorImageScale = new Vector3(-1f, -1f, 1f);
            sensorData.depthImageScale = new Vector3(-1f, -1f, 1f);
            sensorData.infraredImageScale = new Vector3(-1f, -1f, 1f);
            sensorData.sensorSpaceScale = new Vector3(-1f, -1f, 1f);

            // depth camera offset & matrix z-flip
            sensorRotOffset = new Vector3(6f, 0f, 0f);   // the depth camera is tilted 6 degrees downwards
            sensorRotFlipZ = true;
            sensorRotIgnoreY = true;

            // color camera data & intrinsics
            sensorData.colorImageFormat = TextureFormat.BGRA32;
            sensorData.colorImageStride = 4;  // 4 bytes per pixel

            if ((dwFlags & KinectInterop.FrameSource.TypeColor) != 0)
            {
                CameraCalibration colorCamera = coordMapper.ColorCameraCalibration;
                sensorData.colorImageWidth = colorCamera.ResolutionWidth;
                sensorData.colorImageHeight = colorCamera.ResolutionHeight;

                rawColorImage = new byte[sensorData.colorImageWidth * sensorData.colorImageHeight * 4];

                sensorData.colorImageTexture = new Texture2D(sensorData.colorImageWidth, sensorData.colorImageHeight, TextureFormat.BGRA32, false);
                sensorData.colorImageTexture.wrapMode = TextureWrapMode.Clamp;
                sensorData.colorImageTexture.filterMode = FilterMode.Point;
            }

            // depth camera data & intrinsics
            if ((dwFlags & KinectInterop.FrameSource.TypeDepth) != 0)
            {
                CameraCalibration depthCamera = coordMapper.DepthCameraCalibration;
                sensorData.depthImageWidth = depthCamera.ResolutionWidth;
                sensorData.depthImageHeight = depthCamera.ResolutionHeight;

                rawDepthImage = new ushort[sensorData.depthImageWidth * sensorData.depthImageHeight];
                sensorData.depthImage = new ushort[sensorData.depthImageWidth * sensorData.depthImageHeight];
            }

            // infrared data
            if ((dwFlags & KinectInterop.FrameSource.TypeInfrared) != 0)
            {
                if (sensorData.depthImageWidth == 0 || sensorData.depthImageHeight == 0)
                {
                    CameraCalibration depthCamera = coordMapper.DepthCameraCalibration;
                    sensorData.depthImageWidth = depthCamera.ResolutionWidth;
                    sensorData.depthImageHeight = depthCamera.ResolutionHeight;
                }

                rawInfraredImage = new ushort[sensorData.depthImageWidth * sensorData.depthImageHeight];
                sensorData.infraredImage = new ushort[sensorData.depthImageWidth * sensorData.depthImageHeight];

                minInfraredValue = 0f;
                maxInfraredValue = 1000f;
            }

            // imu data
            if (kinectPlayback != null || (dwFlags & KinectInterop.FrameSource.TypePose) != 0)
            {
                imuAhrs = new AHRS.MahonyAHRS(0.0006f, 5f);  // 600us

                imuTurnRot1 = Quaternion.Euler(0f, 90f, 90f);
                imuTurnRot2 = Quaternion.Euler(90f, !flipImuRotation ? 90f : -90f, 0f);
                //lastFlipImuRot = flipImuRotation;
            }

            // calibration data
            GetCameraIntrinsics(CalibrationDeviceType.Color, coordMapper.ColorCameraCalibration, ref sensorData.colorCamIntr);
            GetCameraIntrinsics(CalibrationDeviceType.Depth, coordMapper.DepthCameraCalibration, ref sensorData.depthCamIntr);
            GetCameraExtrinsics(coordMapper.ColorCameraCalibration.Extrinsics, ref sensorData.depth2ColorExtr);
            GetCameraExtrinsics(coordMapper.DepthCameraCalibration.Extrinsics, ref sensorData.color2DepthExtr);

            // body & body-index data
            if ((dwFlags & KinectInterop.FrameSource.TypeBody) != 0)
            {
                InitBodyTracking(dwFlags, sensorData, coordMapper, true);
            }

            Debug.Log("Kinect4Azure-sensor opened.");

            return sensorData;
        }


        public override void CloseSensor(KinectInterop.SensorData sensorData)
        {
            base.CloseSensor(sensorData);

            if(d2cColorData != null)
            {
                d2cColorData.Dispose();
                d2cColorData = null;
            }

            if (c2dDepthData != null)
            {
                c2dDepthData.Dispose();
                c2dDepthData = null;
            }

            if (coordMapperTransform != null)
            {
                // dispose coord-mapper transform
                coordMapperTransform.Dispose();
                coordMapperTransform = null;
            }

            if(kinectPlayback != null)
            {
                // close the playback file
                kinectPlayback.Dispose();
                kinectPlayback = null;
            }

            if (kinectSensor != null)
            {
                // stop cameras, if needed
                if(isCamerasStarted)
                {
                    isCamerasStarted = false;
                    kinectSensor.StopCameras();
                }

                // stop IMU, if needed
                if(isImuStarted)
                {
                    isImuStarted = false;
                    kinectSensor.StopImu();
                }

                // close the sensor
                kinectSensor.Dispose();
                kinectSensor = null;
            }

            Debug.Log("Kinect4Azure-sensor closed.");

        }


        public override void PollSensorFrames(KinectInterop.SensorData sensorData)
        {
            try
            {
                if (kinectPlayback != null)
                {
                    if (kinectPlayback.IsEndOfStream())
                        return;

                    if(playbackStartTime == 0)
                    {
                        // start time
                        playbackStartTime = DateTime.Now.Ticks;
                    }

                    long currentPlayTime = DateTime.Now.Ticks - playbackStartTime;

                    if ((frameSourceFlags & (KinectInterop.FrameSource)0x7F) != 0)
                    {
                        kinectPlayback.SeekTimestamp((ulong)(currentPlayTime / 10));
                        Capture capture = kinectPlayback.GetNextCapture();

                        if (capture != null)
                        {
                            ProcessCameraFrames(sensorData, capture);
                            capture.Dispose();

                            currentFrameNumber++;
                        }
                        else
                        {
                            Debug.Log("End of recording detected.");
                        }
                    }

                    if ((frameSourceFlags & KinectInterop.FrameSource.TypePose) != 0)
                    {
                        ImuSample imuSample = kinectPlayback.GetNextImuSample();

                        while (imuSample != null)
                        {
                            ProcessImuFrame(imuSample);

                            ulong imuTimestamp = (ulong)imuSample.AccelerometerTimestamp.Ticks;
                            if (kinectPlayback.IsEndOfStream() || imuTimestamp >= rawDepthTimestamp)
                                break;

                            imuSample = kinectPlayback.GetNextImuSample();
                        }
                    }
                }
                else
                {
                    if (isCamerasStarted)
                    {
                        Capture capture = kinectSensor.GetCapture(timeToWait);
                        ProcessCameraFrames(sensorData, capture);
                        capture.Dispose();

                        currentFrameNumber++;
                    }

                    if (isImuStarted)
                    {
                        ImuSample imuSample = kinectSensor.GetImuSample(timeToWait);

                        while(imuSample != null)
                        {
                            ProcessImuFrame(imuSample);
                            imuSample = kinectSensor.GetImuSample(timeToWait);
                        }
                    }
                }
            }
            catch (System.TimeoutException)
            {
                // do nothing
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }


        // processes the camera frames
        private void ProcessCameraFrames(KinectInterop.SensorData sensorData, Capture capture)
        {
            //// check for color & depth sync
            //if (isSyncDepthAndColor && (capture.Color == null || capture.Depth == null))
            //    return;

            Capture btCapture = null;

            try
            {
                if (bodyTracker != null)
                {
                    // check for available body frame
                    btCapture = PollBodyFrame(sensorData, capture);

                    if(isSyncBodyAndDepth)
                    {
                        // process the body tracking capture
                        capture = btCapture;
                        //if(capture != null)
                        //    Debug.Log("BtDepthTimestamp: " + capture.Depth.DeviceTimestamp.Ticks);
                    }
                }

                // check for body & depth sync
                if (!isSyncBodyAndDepth || btCapture != null /**&& (btQueueCount == 0 && sensorData.lastBodyFrameTime == rawDepthTimestamp)*/)  // currentBodyTimestamp
                {
                    // color frame
                    if (capture.Color != null && rawColorImage != null && rawColorTimestamp == sensorData.lastColorFrameTime)
                    {
                        if (kinectPlayback != null)
                            WaitForPlaybackTimestamp("color", capture.Color.DeviceTimestamp.Ticks);

                        lock (colorFrameLock)
                        {
                            //capture.Color.CopyBytesTo(rawColorImage, 0, 0, rawColorImage.Length);
                            KinectInterop.CopyBytes(capture.Color.GetBuffer(), (int)capture.Color.Size, rawColorImage, rawColorImage.Length, sizeof(byte));
                            rawColorTimestamp = (ulong)capture.Color.DeviceTimestamp.Ticks;
                            //Debug.Log("RawColorTimestamp: " + rawColorTimestamp);
                        }
                    }

                    // depth frame
                    if (capture.Depth != null && rawDepthImage != null && rawDepthTimestamp == sensorData.lastDepthFrameTime)
                    {
                        if (kinectPlayback != null)
                            WaitForPlaybackTimestamp("depth", capture.Depth.DeviceTimestamp.Ticks);

                        lock (depthFrameLock)
                        {
                            //capture.Depth.CopyTo(rawDepthImage, 0, 0, rawDepthImage.Length);
                            KinectInterop.CopyBytes(capture.Depth.GetBuffer(), (int)capture.Depth.Size, rawDepthImage, rawDepthImage.Length, sizeof(ushort));
                            rawDepthTimestamp = (ulong)capture.Depth.DeviceTimestamp.Ticks;
                            //Debug.Log("RawDepthTimestamp: " + rawDepthTimestamp);
                        }
                    }

                    // infrared frame
                    if (capture.IR != null && rawInfraredImage != null && rawInfraredTimestamp == sensorData.lastInfraredFrameTime)
                    {
                        if (kinectPlayback != null)
                            WaitForPlaybackTimestamp("ir", capture.IR.DeviceTimestamp.Ticks);

                        lock (infraredFrameLock)
                        {
                            //capture.IR.CopyTo(rawInfraredImage, 0, 0, rawInfraredImage.Length);
                            KinectInterop.CopyBytes(capture.IR.GetBuffer(), (int)capture.IR.Size, rawInfraredImage, rawInfraredImage.Length, sizeof(ushort));
                            rawInfraredTimestamp = (ulong)capture.IR.DeviceTimestamp.Ticks;
                            //Debug.Log("RawInfraredTimestamp: " + rawInfraredTimestamp);
                        }
                    }

                    // transformation data frames
                    if ((depth2ColorDataFrame != null || color2DepthDataFrame != null) && capture.Color != null && capture.Depth != null &&
                        (rawColorTimestamp != sensorData.lastColorFrameTime || rawDepthTimestamp != sensorData.lastDepthFrameTime))
                    {
                        if (coordMapperTransform == null)
                        {
                            coordMapperTransform = coordMapper.CreateTransformation();
                        }

                        if (depth2ColorDataFrame != null)
                        {
                            if(d2cColorData == null)
                            {
                                d2cColorData = new Image(ImageFormat.ColorBGRA32, sensorData.depthImageWidth, sensorData.depthImageHeight, sensorData.depthImageWidth * sensorData.colorImageStride);
                            }

                            lock (depth2ColorFrameLock)
                            {
                                coordMapperTransform.ColorImageToDepthCamera(capture.Depth, capture.Color, d2cColorData);
                                d2cColorData.CopyTo<byte>(depth2ColorDataFrame, 0, 0, depth2ColorDataFrame.Length);
                                lastDepth2ColorFrameTime = (ulong)capture.Depth.DeviceTimestamp.Ticks;
                            }
                        }

                        if (color2DepthDataFrame != null)
                        {
                            if (c2dDepthData == null)
                            {
                                c2dDepthData = new Image(ImageFormat.Depth16, sensorData.colorImageWidth, sensorData.colorImageHeight, sensorData.colorImageWidth * sizeof(ushort));
                            }

                            lock (color2DepthFrameLock)
                            {
                                coordMapperTransform.DepthImageToColorCamera(capture.Depth, c2dDepthData);
                                c2dDepthData.CopyTo<ushort>(color2DepthDataFrame, 0, 0, color2DepthDataFrame.Length);
                                lastColor2DepthFrameTime = (ulong)capture.Color.DeviceTimestamp.Ticks;
                            }
                        }
                    }

                }
                else
                {
                    // ignore the capture
                    capture = null;
                }

                if (btCapture != null)
                {
                    // dispose body capture
                    btCapture.Dispose();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }


        // in playback mode - waits until the given time stamp
        private void WaitForPlaybackTimestamp(string frameSource, long frameTimestamp)
        {
            //Debug.Log(string.Format("{0} ts: {1}, sys: {2}", frameSource, frameTimestamp, (DateTime.Now.Ticks - playbackStartTime)));

            long currentPlayTime = DateTime.Now.Ticks - playbackStartTime;

            while (currentPlayTime < frameTimestamp)
            {
                currentPlayTime = DateTime.Now.Ticks - playbackStartTime;
            }
        }

        // processes the IMU frame
        private void ProcessImuFrame(ImuSample imuSample)
        {
            if (kinectPlayback != null)
            {
                //WaitForPlaybackTimestamp("acc", imuSample.AccelerometerTimestampInUsec);
                //WaitForPlaybackTimestamp("gyro", imuSample.GyroTimestampInUsec);
            }

            lastImuSample = curImuSample;
            curImuSample = imuSample;

            if (!flipImuRotation && lastImuSample == null && Mathf.Abs(imuSample.AccelerometerSample.Y) >= 1f)
            {
                // check for rolled imu
                flipImuRotation = true;
                imuTurnRot2 = Quaternion.Euler(90f, -90f, 0f);
            }

            if (lastImuSample != null && imuAhrs != null && imuSample.AccelerometerTimestamp.Ticks != lastImuSample.AccelerometerTimestamp.Ticks)
            {
                prevRotZ = imuRotation.eulerAngles.z;
                Array.Copy(imuAhrs.Quaternion, prevQuat, prevQuat.Length);

                imuAhrs.SamplePeriod = ((imuSample.AccelerometerTimestamp.Ticks - lastImuSample.AccelerometerTimestamp.Ticks) / 10) * 0.000001f;
                imuAhrs.Update(imuSample.GyroSample.X, imuSample.GyroSample.Y, imuSample.GyroSample.Z, imuSample.AccelerometerSample.X, imuSample.AccelerometerSample.Y, imuSample.AccelerometerSample.Z);

                float[] ahrsQuat = imuAhrs.Quaternion;
                imuRotation = new Quaternion(ahrsQuat[1], ahrsQuat[2], ahrsQuat[3], ahrsQuat[0]);
                Quaternion transRotation = imuTurnRot2 * imuRotation * imuTurnRot1;

                if (imuAhrs.E2 < 0.0001f && Mathf.Abs(imuRotation.eulerAngles.z - prevRotZ) <= 0.1f)
                {
                    // correct the continuous yaw, when imu doesn't move
                    Array.Copy(prevQuat, imuAhrs.Quaternion, prevQuat.Length);

                    ahrsQuat = prevQuat;
                    imuRotation = new Quaternion(ahrsQuat[1], ahrsQuat[2], ahrsQuat[3], ahrsQuat[0]);
                    transRotation = imuTurnRot2 * imuRotation * imuTurnRot1;

                    if (!imuYawAtStartSet)
                    {
                        // initial yaw
                        imuYawAtStartSet = true;
                        imuYawAtStart = new Vector3(0f, transRotation.eulerAngles.y, 0f);
                        imuFixAtStart = Quaternion.Inverse(Quaternion.Euler(imuYawAtStart));
                    }
                }

                if (imuYawAtStartSet)
                {
                    // fix the initial yaw
                    transRotation = imuFixAtStart * transRotation;
                }

                lock (poseFrameLock)
                {
                    // rawPosePosition
                    rawPoseRotation = transRotation;

                    rawPoseTimestamp = (ulong)imuSample.AccelerometerTimestamp.Ticks;
                    //poseFrameNumber = currentFrameNumber;
                }
            }

            //var onImuSample = OnImuSample;
            //if(onImuSample != null)
            //{
            //    onImuSample.Invoke(imuSample, rawDepthTimestamp, rawColorTimestamp);
            //}
        }


        //public override bool UpdateSensorData(KinectInterop.SensorData sensorData, KinectManager kinectManager)
        //{
        //    bool bSuccess = base.UpdateSensorData(sensorData, kinectManager);

        //    if (lastFlipImuRot != flipImuRotation)
        //    {
        //        Vector3 imuRot2 = imuTurnRot2.eulerAngles;
        //        imuRot2.y = -imuRot2.y;
        //        imuTurnRot2 = Quaternion.Euler(imuRot2);
        //    }

        //    return bSuccess;
        //}


        // gets the given camera intrinsics
        private void GetCameraIntrinsics(CalibrationDeviceType camType, CameraCalibration camParams, ref KinectInterop.CameraIntrinsics intr)
        {
            Intrinsics camIntr = camParams.Intrinsics;
            if (camIntr.Parameters.Length < 15)
                throw new System.Exception("Intrinsics length is less than expected: " + camIntr.ParameterCount);

            intr = new KinectInterop.CameraIntrinsics();

            intr.cameraType = (int)camType;
            intr.width = camParams.ResolutionWidth;
            intr.height = camParams.ResolutionHeight;

            // 0        float cx;
            // 1        float cy;
            intr.ppx = camIntr.Parameters[0];
            intr.ppy = camIntr.Parameters[1];

            // 2        float fx;            /**< Focal length x */
            // 3        float fy;            /**< Focal length y */
            intr.fx = camIntr.Parameters[2];
            intr.fy = camIntr.Parameters[3];

            // 4        float k1;
            // 5        float k2;
            // 6        float k3;
            // 7        float k4;
            // 8        float k5;
            // 9        float k6;
            intr.distCoeffs = new float[6];
            intr.distCoeffs[0] = camIntr.Parameters[4];
            intr.distCoeffs[1] = camIntr.Parameters[5];
            intr.distCoeffs[2] = camIntr.Parameters[6];
            intr.distCoeffs[3] = camIntr.Parameters[7];
            intr.distCoeffs[4] = camIntr.Parameters[8];
            intr.distCoeffs[5] = camIntr.Parameters[9];

            if (camIntr.Type == CalibrationModelType.Theta)
                intr.distType = KinectInterop.DistortionType.Theta;
            else if (camIntr.Type == CalibrationModelType.Polynomial3K)
                intr.distType = KinectInterop.DistortionType.Polynomial3K;
            else if (camIntr.Type == CalibrationModelType.Rational6KT)
                intr.distType = KinectInterop.DistortionType.Rational6KT;
            else
                intr.distType = (KinectInterop.DistortionType)camIntr.Type;

            // 10            float codx;
            // 11            float cody;
            intr.codx = camIntr.Parameters[10];
            intr.cody = camIntr.Parameters[11];

            // 12            float p2;
            // 13            float p1;
            intr.p2 = camIntr.Parameters[12];
            intr.p1 = camIntr.Parameters[13];

            // 14           float metric_radius;
            intr.maxRadius = camIntr.Parameters[14];

            EstimateFOV(intr);
        }


        // gets the given camera extrinsics
        private void GetCameraExtrinsics(Extrinsics camExtr, ref KinectInterop.CameraExtrinsics extr)
        {
            extr = new KinectInterop.CameraExtrinsics();

            extr.rotation = new float[camExtr.Rotation.Length];
            camExtr.Rotation.CopyTo(extr.rotation, 0);

            extr.translation = new float[camExtr.Translation.Length];
            camExtr.Translation.CopyTo(extr.translation, 0);
        }


        public override Vector2 MapDepthPointToColorCoords(KinectInterop.SensorData sensorData, Vector2 depthPos, ushort depthVal)
        {
            if (sensorData.depthCamIntr != null && sensorData.colorCamIntr != null && sensorData.depth2ColorExtr != null)
            {
                Vector3 depthSpacePos = UnprojectPoint(sensorData.depthCamIntr, depthPos, (float)depthVal);
                Vector3 colorSpacePos = TransformPoint(sensorData.depth2ColorExtr, depthSpacePos);
                Vector2 colorPos = ProjectPoint(sensorData.colorCamIntr, colorSpacePos);

                return colorPos;
            }

            return Vector2.zero;
        }


        // unprojects plane point into the space
        protected override Vector3 UnprojectPoint(KinectInterop.CameraIntrinsics intr, Vector2 pixel, float depth)
        {
            if (depth <= 0f)
                return Vector3.zero;

            System.Numerics.Vector2 fPixel = new System.Numerics.Vector2(pixel.x, pixel.y);
            System.Numerics.Vector3? fPoint = coordMapper.TransformTo3D(fPixel, depth, (CalibrationDeviceType)intr.cameraType, (CalibrationDeviceType)intr.cameraType);
            Vector3 point = fPoint.HasValue ? new Vector3(fPoint.Value.X, fPoint.Value.Y, fPoint.Value.Z) : Vector3.zero;

            return point;
        }


        // projects space point onto a plane
        protected override Vector2 ProjectPoint(KinectInterop.CameraIntrinsics intr, Vector3 point)
        {
            if (point == Vector3.zero)
                return Vector2.zero;

            System.Numerics.Vector3 fPoint = new System.Numerics.Vector3(point.x, point.y, point.z);
            System.Numerics.Vector2? fPixel = coordMapper.TransformTo2D(fPoint, (CalibrationDeviceType)intr.cameraType, (CalibrationDeviceType)intr.cameraType);
            Vector2 pixel = fPixel.HasValue ? new Vector2(fPixel.Value.X, fPixel.Value.Y) : Vector2.zero;

            return pixel;
        }


        // transforms a point from one space to another
        protected override Vector3 TransformPoint(KinectInterop.CameraExtrinsics extr, Vector3 point)
        {
            float toPointX = extr.rotation[0] * point.x + extr.rotation[1] * point.y + extr.rotation[2] * point.z + extr.translation[0];
            float toPointY = extr.rotation[3] * point.x + extr.rotation[4] * point.y + extr.rotation[5] * point.z + extr.translation[1];
            float toPointZ = extr.rotation[6] * point.x + extr.rotation[7] * point.y + extr.rotation[8] * point.z + extr.translation[2];

            return new Vector3(toPointX, toPointY, toPointZ);
        }


        //public override bool MapDepthFrameToColorData(KinectInterop.SensorData sensorData, ref byte[] vColorFrameData)
        //{
        //    if(coordMapperTransform == null)
        //    {
        //        coordMapperTransform = coordMapper.CreateTransformation();
        //    }

        //    if(vColorFrameData == null)
        //    {
        //        vColorFrameData = new byte[sensorData.colorImageWidth * sensorData.colorImageHeight * sensorData.colorImageStride];
        //    }

        //    return false;
        //}


        //public override bool MapColorFrameToDepthData(KinectInterop.SensorData sensorData, ref ushort[] vDepthFrameData)
        //{
        //    return false;
        //}


    }
}
