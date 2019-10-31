using System;
using System.Collections.Generic;
using System.IO;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable] public class UnityInfraredCameraEvent : UnityEvent<Texture2D>{ }
    [Serializable] public class UnitySkeletonEvent : UnityEvent<IList<Body>>{ }
    
    public class KinectManagerBehaviour : MonoBehaviour
    {
        [SerializeField] private bool infrared;
        [SerializeField] public UnityInfraredCameraEvent infraredFrameReceived;

        [SerializeField] private bool skeleton;
        [SerializeField] public UnitySkeletonEvent skeletonDataReveived; 

        private Texture2D InfraredTexture { get; set;}
        
        private KinectSensor sensor_;
        private InfraredFrameReader infraredFrameReader_;
        private BodyFrameReader bodyFrameReader_;
        private ushort [] irData_;

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
            InfraredTexture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.R16, false);
            
            onInfraredFrameArrived_ = (frameReader, eventArgs) =>
            {
                if(!infrared)
                    return;
                
                using (var infraredFrame = infraredFrameReader_.AcquireLatestFrame())
                {
                    if (infraredFrame == null) return;
                    infraredFrame.CopyFrameDataToArray(irData_);

                    unsafe
                    {
                        fixed (ushort* irDataPtr = irData_)
                        {
                            InfraredTexture.LoadRawTextureData((IntPtr) irDataPtr, sizeof(ushort) * irData_.Length);
                        }
                    }

                    InfraredTexture.Apply();
                }
                infraredFrameReceived.Invoke(InfraredTexture);
            };
            infraredFrameReader_.FrameArrived += onInfraredFrameArrived_;
        }

        private void InitSkeletonTracking()
        {

            
            bodyFrameReader_ = sensor_.BodyFrameSource.OpenReader();
            onSkeletonFrameArrived_ = (frameReader, eventArgs) =>
            {
                if(!skeleton)
                    return;
                
                using (var bodyFrame = eventArgs.FrameReference.AcquireFrame())
                {

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


