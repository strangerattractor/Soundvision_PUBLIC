using System;
using System.IO;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable] public class UnityInfraredCameraEvent : UnityEvent<Texture2D>{ }
    [Serializable] public class UnitySkeletonEvent : UnityEvent<Body[]>{ }
    
    public class KinectManagerBehaviour : MonoBehaviour
    {
        [SerializeField] private bool infrared;
        [SerializeField] public UnityInfraredCameraEvent infraredFrameReceived;

        [SerializeField] private bool skeleton;
        [SerializeField] public UnitySkeletonEvent skeletonDataReceived;
        
        private KinectSensor sensor_;
        private InfraredFrameReader infraredFrameReader_;
        private BodyFrameReader bodyFrameReader_;

        private ushort [] irData_;
        private Texture2D infraredTexture_;
        private Body[] bodies_;
        
        private EventHandler<InfraredFrameArrivedEventArgs> onInfraredFrameArrived_;
        private EventHandler<BodyFrameArrivedEventArgs> onSkeletonFrameArrived_;
        
        private void Start()
        {
            sensor_ = KinectSensor.GetDefault();
            if (sensor_ == null)
                throw new IOException("cannot find Kinect Sensor ");
            
            InitInfraredCamera();
            InitSkeletonTracking();
            
            if (!sensor_.IsOpen)
                sensor_.Open();
        }

        private void InitInfraredCamera()
        {
            infraredFrameReader_ = sensor_.InfraredFrameSource.OpenReader();
            var frameDesc = sensor_.InfraredFrameSource.FrameDescription;
            irData_ = new ushort[frameDesc.LengthInPixels];
            infraredTexture_ = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.R16, false);
            
            onInfraredFrameArrived_ = (frameReader, eventArgs) =>
            {
                if(!infrared)
                    return;
                
                using (var infraredFrame = eventArgs.FrameReference.AcquireFrame())
                {
                    if (infraredFrame == null) 
                        return;
                    
                    infraredFrame.CopyFrameDataToArray(irData_);
                    unsafe
                    {
                        fixed (ushort* irDataPtr = irData_)
                        {
                            infraredTexture_.LoadRawTextureData((IntPtr) irDataPtr, sizeof(ushort) * irData_.Length);
                        }
                    }

                    infraredTexture_.Apply();
                }
                infraredFrameReceived.Invoke(infraredTexture_);

            };
            infraredFrameReader_.FrameArrived += onInfraredFrameArrived_;
        }

        private void InitSkeletonTracking()
        {
            bodies_ = new Body[1];

            bodyFrameReader_ = sensor_.BodyFrameSource.OpenReader();
            onSkeletonFrameArrived_ = (frameReader, eventArgs) =>
            {
                if(!skeleton)
                    return;

                using (var bodyFrame = eventArgs.FrameReference.AcquireFrame())
                {
                    if (bodyFrame == null)
                        return;
                    Array.Resize(ref bodies_, bodyFrame.BodyCount);
                    bodyFrame.GetAndRefreshBodyData(bodies_);
                    skeletonDataReceived.Invoke(bodies_);
                }
            };
            bodyFrameReader_.FrameArrived += onSkeletonFrameArrived_;
        }

        private void OnDestroy()
        {
            infraredFrameReader_.FrameArrived -= onInfraredFrameArrived_;
            bodyFrameReader_.FrameArrived -= onSkeletonFrameArrived_;
        }
    }
}


