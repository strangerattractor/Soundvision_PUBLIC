using System.IO;
using UnityEngine;
using Windows.Kinect;

namespace VideoInput
{
    public interface IInfraredCamera
    {
        Texture2D Data { get; }
    }
    
    public class InfraredCamera : IInfraredCamera
    {
        public Texture2D Data { get; }
        
        private Windows.Kinect.KinectSensor sensor_;
        private readonly InfraredFrameReader reader_;
        private readonly ushort[] irData_;
        private readonly byte[] rawData_;

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
            rawData_ = new byte[frameDesc.LengthInPixels * 4];
            Data = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.BGRA32, false);
            
            if (!sensor_.IsOpen)
                sensor_.Open();
        }
        
        void Update()
        {
            if (reader_ == null)
                throw new IOException("Kinect reader not opened");
            
            var frame = reader_.AcquireLatestFrame();
            if (frame == null)
                return;

            frame.CopyFrameDataToArray(irData_);
            
            var index = 0;
            foreach(var ir in irData_)
            {
                var intensity = (byte)(ir >> 8);
                rawData_[index++] = intensity;
                rawData_[index++] = intensity;
                rawData_[index++] = intensity;
                rawData_[index++] = 255; // Alpha
            }
        
            Data.LoadRawTextureData(rawData_);
            Data.Apply();
            
            frame.Dispose();
        }
    }
}

