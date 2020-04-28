using System;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    
    [Serializable] class UnityJointEvent : UnityEvent<Windows.Kinect.Joint> { }

    public class KinectJointBind : MonoBehaviour
    {
        [SerializeField, Range(0, 5)] private int bodyId = 0;
        [SerializeField] private JointType jointType;
        [SerializeField] private UnityJointEvent JointDataReceived;
        
        public void OnSkeletonDataReceived(Body body, int id)
        {
            if (id != bodyId)
                return;

            if (!body.IsTracked)
                return;

            var joint = body.Joints[jointType];
            if (joint.TrackingState != TrackingState.Tracked)
                return;
            
            JointDataReceived.Invoke(joint);
        }
    }
}
