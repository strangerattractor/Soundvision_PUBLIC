using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace com.rfilkov.kinect
{
    public class DummyK2Interface : DepthSensorBase
    {

        public override KinectInterop.DepthSensorPlatform GetSensorPlatform()
        {
            return KinectInterop.DepthSensorPlatform.DummyK2;
        }

        //public override bool InitSensorInterface(bool bCopyLibs, ref bool bNeedRestart)
        //{
        //    bool bOnceRestarted = bNeedRestart;
        //    bNeedRestart = false;

        //    if (!bCopyLibs)
        //    {
        //        // skip this interface on the 1st pass
        //        return bOnceRestarted;
        //    }

        //    return true;
        //}

        //public override void FreeSensorInterface(bool bDeleteLibs)
        //{
        //}

        public override List<KinectInterop.SensorDeviceInfo> GetAvailableSensors()
        {
            List<KinectInterop.SensorDeviceInfo> alSensorInfo = new List<KinectInterop.SensorDeviceInfo>();

            KinectInterop.SensorDeviceInfo sensorInfo = new KinectInterop.SensorDeviceInfo();
            sensorInfo.sensorId = "DummyK2";
            sensorInfo.sensorName = "Dummy Kinect-v2";
            sensorInfo.sensorCaps = KinectInterop.FrameSource.TypeAll & ~KinectInterop.FrameSource.TypePose;

            alSensorInfo.Add(sensorInfo);

            return alSensorInfo;
        }

        public override KinectInterop.SensorData OpenSensor(KinectInterop.FrameSource dwFlags, bool bSyncDepthAndColor, bool bSyncBodyAndDepth)
        {
            // save initial parameters
            base.OpenSensor(dwFlags, bSyncDepthAndColor, bSyncBodyAndDepth);

            List<KinectInterop.SensorDeviceInfo> alSensors = GetAvailableSensors();
            if (deviceIndex < 0 || deviceIndex >= alSensors.Count)
                return null;

            KinectInterop.SensorData sensorData = new KinectInterop.SensorData();

            sensorData.colorImageWidth = 1920;
            sensorData.colorImageHeight = 1080;

            // flip color & depth images vertically
            sensorData.colorImageScale = new Vector3(1f, -1f, 1f);
            sensorData.depthImageScale = new Vector3(1f, -1f, 1f);
            sensorData.infraredImageScale = new Vector3(1f, -1f, 1f);
            sensorData.sensorSpaceScale = new Vector3(1f, 1f, 1f);

            sensorData.depthImageWidth = 512;
            sensorData.depthImageHeight = 424;

            Debug.Log("DummyK2-sensor opened");

            return sensorData;
        }

        public override void CloseSensor(KinectInterop.SensorData sensorData)
        {
            Debug.Log("DummyK2-sensor closed");
        }

    }
}
