using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace com.rfilkov.kinect
{
    public interface DepthSensorInterface
    {
        // returns the depth sensor platform
        KinectInterop.DepthSensorPlatform GetSensorPlatform();

        // returns the list of available sensors, controlled by this sensor interface
        List<KinectInterop.SensorDeviceInfo> GetAvailableSensors();

        // opens the given sensor and inits needed resources. returns new sensor-data object
        KinectInterop.SensorData OpenSensor(KinectInterop.FrameSource dwFlags, bool bSyncDepthAndColor, bool bSyncBodyAndDepth);

        // initializes the secondary sensor data, after sensor initialization
        void InitSensorData(KinectInterop.SensorData sensorData, KinectManager kinectManager);

        // closes the sensor and frees used resources
        void CloseSensor(KinectInterop.SensorData sensorData);

        // polls data frames in the sensor-specific thread
        void PollSensorFrames(KinectInterop.SensorData sensorData);

        // polls coordinate transformation frames and data in the sensor-specific thread
        void PollCoordTransformFrames(KinectInterop.SensorData sensorData);

        // post-processes the sensor data after polling
        void ProcessSensorDataInThread(KinectInterop.SensorData sensorData);

        // updates sensor data, if needed
        // returns true if update is successful, false otherwise
        bool UpdateSensorData(KinectInterop.SensorData sensorData, KinectManager kinectManager, bool isPlayMode);

        // updates transformed frame textures, if needed
        // returns true if update is successful, false otherwise
        bool UpdateTransformedFrameTextures(KinectInterop.SensorData sensorData, KinectManager kinectManager);

        // updates the selected sensor textures, if needed
        // returns true if update is successful, false otherwise
        bool UpdateSensorTextures(KinectInterop.SensorData sensorData, KinectManager kinectManager, ulong prevDepthFrameTime);

        // returns sensor-to-world matrix
        Matrix4x4 GetSensorToWorldMatrix();

        // returns sensor transform. Please note transform updates depend on the getPoseFrames-KM setting.
        Transform GetSensorTransform();

        // returns the depth camera space table
        Vector3[] GetDepthCameraSpaceTable(KinectInterop.SensorData sensorData);

        // returns the color camera space table
        Vector3[] GetColorCameraSpaceTable(KinectInterop.SensorData sensorData);

        // returns depth camera space coordinates for the given depth image point
        Vector3 MapDepthPointToSpaceCoords(KinectInterop.SensorData sensorData, Vector2 depthPos, ushort depthVal);

        // returns depth image coordinates for the given depth camera space point
        Vector2 MapSpacePointToDepthCoords(KinectInterop.SensorData sensorData, Vector3 spacePos);

        // returns color camera space coordinates for the given color image point
        Vector3 MapColorPointToSpaceCoords(KinectInterop.SensorData sensorData, Vector2 colorPos, ushort depthVal);

        // returns color image coordinates for the given color camera space point
        Vector2 MapSpacePointToColorCoords(KinectInterop.SensorData sensorData, Vector3 spacePos);

        // returns color image coordinates for the given depth image point
        Vector2 MapDepthPointToColorCoords(KinectInterop.SensorData sensorData, Vector2 depthPos, ushort depthVal);

        // returns depth image coordinates for the given color image point
        Vector2 MapColorPointToDepthCoords(KinectInterop.SensorData sensorData, Vector2 colorPos);

        // returns the resolution in pixels of the point-cloud textures
        Vector2Int GetPointCloudTexResolution(KinectInterop.SensorData sensorData);
    }
}
