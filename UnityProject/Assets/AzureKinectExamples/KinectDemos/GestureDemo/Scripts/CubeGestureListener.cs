using UnityEngine;
using System.Collections;
using System;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// This gesture listener detects discrete gestures - hand swipes (left, right and up).
    /// </summary>
    public class CubeGestureListener : MonoBehaviour, GestureListenerInterface
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("UI-Text to display gesture-listener messages and gesture information.")]
        public UnityEngine.UI.Text gestureInfo;

        // singleton instance of the class
        private static CubeGestureListener instance = null;

        // whether the needed gesture has been detected or not
        private bool swipeLeft = false;
        private bool swipeRight = false;
        private bool swipeUp = false;


        /// <summary>
        /// Gets the singleton CubeGestureListener instance.
        /// </summary>
        /// <value>The CubeGestureListener instance.</value>
        public static CubeGestureListener Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Determines whether swipe left is detected.
        /// </summary>
        /// <returns><c>true</c> if swipe left is detected; otherwise, <c>false</c>.</returns>
        public bool IsSwipeLeft()
        {
            if (swipeLeft)
            {
                swipeLeft = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether swipe right is detected.
        /// </summary>
        /// <returns><c>true</c> if swipe right is detected; otherwise, <c>false</c>.</returns>
        public bool IsSwipeRight()
        {
            if (swipeRight)
            {
                swipeRight = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether swipe up is detected.
        /// </summary>
        /// <returns><c>true</c> if swipe up is detected; otherwise, <c>false</c>.</returns>
        public bool IsSwipeUp()
        {
            if (swipeUp)
            {
                swipeUp = false;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Invoked when a new user is detected. Here you can start gesture tracking by invoking KinectGestureManager.DetectGesture()-function.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        public void UserDetected(ulong userId, int userIndex)
        {
            // the gestures are allowed for the selected user only
            KinectGestureManager gestureManager = KinectManager.Instance.gestureManager;
            if (!gestureManager || (userIndex != playerIndex))
                return;

            // set the gestures to detect
            gestureManager.DetectGesture(userId, GestureType.SwipeLeft);
            gestureManager.DetectGesture(userId, GestureType.SwipeRight);
            gestureManager.DetectGesture(userId, GestureType.SwipeUp);

            if (gestureInfo != null)
            {
                gestureInfo.text = "Swipe left, right or up to change the slides.";
            }
        }

        /// <summary>
        /// Invoked when a user gets lost. All tracked gestures for this user are cleared automatically.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        public void UserLost(ulong userId, int userIndex)
        {
            // the gestures are allowed for the selected user only
            if (userIndex != playerIndex)
                return;

            if (gestureInfo != null)
            {
                gestureInfo.text = string.Empty;
            }
        }

        /// <summary>
        /// Invoked when a gesture is in progress.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        /// <param name="gesture">Gesture type</param>
        /// <param name="progress">Gesture progress [0..1]</param>
        /// <param name="joint">Joint type</param>
        /// <param name="screenPos">Normalized viewport position</param>
        public void GestureInProgress(ulong userId, int userIndex, GestureType gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
        {
            // not needed for this demo
        }

        /// <summary>
        /// Invoked if a gesture is completed.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        /// <param name="gesture">Gesture type</param>
        /// <param name="joint">Joint type</param>
        /// <param name="screenPos">Normalized viewport position</param>
        public bool GestureCompleted(ulong userId, int userIndex, GestureType gesture, KinectInterop.JointType joint, Vector3 screenPos)
        {
            // the gestures are allowed for the selected user only
            if (userIndex != playerIndex)
                return false;

            if (gestureInfo != null)
            {
                string sGestureText = gesture + " detected";
                gestureInfo.text = sGestureText;
            }

            if (gesture == GestureType.SwipeLeft)
                swipeLeft = true;
            else if (gesture == GestureType.SwipeRight)
                swipeRight = true;
            else if (gesture == GestureType.SwipeUp)
                swipeUp = true;

            return true;
        }

        /// <summary>
        /// Invoked if a gesture is cancelled.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        /// <param name="gesture">Gesture type</param>
        /// <param name="joint">Joint type</param>
        public bool GestureCancelled(ulong userId, int userIndex, GestureType gesture, KinectInterop.JointType joint)
        {
            // not needed for this demo
            return true;
        }


        void Awake()
        {
            instance = this;
        }

    }
}
