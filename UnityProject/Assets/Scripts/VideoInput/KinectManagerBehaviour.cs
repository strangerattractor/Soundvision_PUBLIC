using System;
using System.IO;
using System.Linq;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable] public class UnityInfraredCameraEvent : UnityEvent<Texture2D>{ }
    [Serializable] public class UnitySkeletonEvent : UnityEvent<Body, int>{ }
    
    public class KinectManagerBehaviour : MonoBehaviour
    {
        [SerializeField] private bool infrared;
        [SerializeField] public UnityInfraredCameraEvent infraredFrameReceived;

        [SerializeField] private bool skeleton;
        [SerializeField] public UnitySkeletonEvent skeletonDataReceived;

        [SerializeField, Range(1, 6)] private int numberOfBodiesTobeTracked = 2;
        
        private KinectSensor sensor_;
        private InfraredFrameReader infraredFrameReader_;
        private BodyFrameReader bodyFrameReader_;
        private BodyIndexFrameReader bodyIndexFrameReader_;

        private ushort [] irData_;
        private Texture2D infraredTexture_;
        private Body[] bodies_;
        
        private EventHandler<InfraredFrameArrivedEventArgs> onInfraredFrameArrived_;
        private EventHandler<BodyFrameArrivedEventArgs> onBodyFrameArrived_;
        private EventHandler<BodyIndexFrameArrivedEventArgs> onBodyIndexFrameArrived_;
        private Holder<ulong> trackedIds_;
        
        private void Start()
        {
            sensor_ = KinectSensor.GetDefault();
            if (sensor_ == null)
                throw new IOException("cannot find Kinect Sensor ");
            
            InitInfraredCamera();
            InitSkeletonTracking();

            if (!sensor_.IsOpen)
            {
                sensor_.Open();
            }
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
            bodies_ = new Body[6];
            trackedIds_ = new Holder<ulong>(numberOfBodiesTobeTracked);
            InitBodyFrameReader();
        }

        private void InitBodyFrameReader()
        {
            bodyFrameReader_ = sensor_.BodyFrameSource.OpenReader();
            onBodyFrameArrived_ = (frameReader, eventArgs) =>
            {
                if(!skeleton)
                    return;

                using (var bodyFrame = eventArgs.FrameReference.AcquireFrame())
                {
                    if (bodyFrame == null)
                        return;
                    
                    bodyFrame.GetAndRefreshBodyData(bodies_);
                    foreach (var body in bodies_.Where(body => body.IsTracked))
                    {
                        if (trackedIds_.Exist(body.TrackingId))
                        {
                            var idNumber = trackedIds_.IndexOf(body.TrackingId);
                            if(idNumber.HasValue)
                                skeletonDataReceived.Invoke(body, idNumber.Value);
                        }
                        else
                        {
                            if (trackedIds_.Add(body.TrackingId))
                            {
                                var idNumber = trackedIds_.IndexOf(body.TrackingId);
                                if (idNumber.HasValue) 
                                    skeletonDataReceived.Invoke(body, idNumber.Value);
                            }
                        }
                    }
                }
            };
            bodyFrameReader_.FrameArrived += onBodyFrameArrived_;
        }
        
        private void OnDestroy()
        {
            infraredFrameReader_.FrameArrived -= onInfraredFrameArrived_;
            bodyFrameReader_.FrameArrived -= onBodyFrameArrived_;
        }
    }
}


