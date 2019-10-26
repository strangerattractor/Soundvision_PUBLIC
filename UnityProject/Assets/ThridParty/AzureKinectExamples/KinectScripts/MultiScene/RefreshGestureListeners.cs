using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    public class RefreshGestureListeners : MonoBehaviour
    {

        void Start()
        {
            KinectManager kinectManager = KinectManager.Instance;

            if (kinectManager)
            {
                // remove all users & filters
                kinectManager.ClearKinectUsers();

                // refresh gesture listeners
                KinectGestureManager gestureManager = kinectManager.gestureManager;
                if(gestureManager)
                {
                    gestureManager.RefreshGestureListeners();
                }
            }
        }

    }
}
