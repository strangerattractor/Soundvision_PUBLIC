using System.IO;
using UnityEngine;
using Windows.Kinect;

namespace VideoInput
{
    public interface IInfraredCamera
    {
        Texture2D Data { get; }

        void Update();
    }
    
    public class InfraredCamera : IInfraredCamera
    {
        public Texture2D Data { get; }
        
        private Windows.Kinect.KinectSensor sensor_;
        private readonly InfraredFrameReader reader_;
        private readonly ushort [] irData_;

        public InfraredCamera()
        {
            sensor_ = Windows.Kinect.KinectSensor.GetDefault();
            if (sensor_ == null)
            {
                throw new IOException("cannot find Kinect Sensor ");
            }
            
            reader_ = sensor_.InfraredFrameSource.OpenReader();
            
            var frameDesc = sensor_.InfraredFrameSource.FrameDescription;
            irData_ = new ushort[frameDesc.LengthInPixels];
            
            Data = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.R8, false);
            
            if (!sensor_.IsOpen)
                sensor_.Open();
        }
        
        public void Update()
        {
            if (reader_ == null)
                throw new IOException("Kinect reader not opened");
            
            var frame = reader_.AcquireLatestFrame();
            if (frame == null) return;

            frame.CopyFrameDataToArray(irData_);
            var rawTextureData = Data.GetRawTextureData();
            for (var i = 0; i < irData_.Length; i++)
                rawTextureData[i] = (byte)(irData_[i] >> 8);
            
            Data.Apply();
            frame.Dispose();
        }
    }
}

