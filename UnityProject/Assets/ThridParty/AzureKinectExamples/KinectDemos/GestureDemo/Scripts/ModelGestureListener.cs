using UnityEngine;
using System.Collections;
using System;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// This gesture listener detects continuous gestures - zoom out/in and wheel, as well as hand raise poses.
    /// </summary>
    public class ModelGestureListener : MonoBehaviour, GestureListenerInterface
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("UI-Text to display gesture-listener messages and gesture information.")]
        public UnityEngine.UI.Text gestureInfo;

        // singleton instance of the class
        private static ModelGestureListener instance = null;

        // internal variables to track if progress message has been displayed for too long
        private bool progressDisplayed;
        private float progressGestureTime;

        // whether the needed gesture has been detected or not
        private bool zoomOut;
        private bool zoomIn;
        private float zoomFactor = 1f;

        private bool wheel;
        private float wheelAngle = 0f;

        private bool raiseHand = false;


        /// <summary>
        /// Gets the singleton ModelGestureListener instance.
        /// </summary>
        /// <value>The ModelGestureListener instance.</value>
        public static ModelGestureListener Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Determines whether the user is zooming out.
        /// </summary>
        /// <returns><c>true</c> if the user is zooming out; otherwise, <c>false</c>.</returns>
        public bool IsZoomingOut()
        {
            return zoomOut;
        }

        /// <summary>
        /// Determines whether the user is zooming in.
        /// </summary>
        /// <returns><c>true</c> if the user is zooming in; otherwise, <c>false</c>.</returns>
        public bool IsZoomingIn()
        {
            return zoomIn;
        }

        /// <summary>
        /// Gets the zoom factor.
        /// </summary>
        /// <returns>The zoom factor.</returns>
        public float GetZoomFactor()
        {
            return zoomFactor;
        }

        /// <summary>
        /// Determines whether the user is turning wheel.
        /// </summary>
        /// <returns><c>true</c> if the user is turning wheel; otherwise, <c>false</c>.</returns>
        public bool IsTurningWheel()
        {
            return wheel;
        }

        /// <summary>
        /// Gets the wheel angle.
        /// </summary>
        /// <returns>The wheel angle.</returns>
        public float GetWheelAngle()
        {
            return wheelAngle;
        }

        /// <summary>
        /// Determines whether the user has raised his left or right hand.
        /// </summary>
        /// <returns><c>true</c> if the user has raised his left or right hand; otherwise, <c>false</c>.</returns>
        public bool IsRaiseHand()
        {
            if (raiseHand)
            {
                raiseHand = false;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Invoked when a new user is detected. Here you can start gesture tracking by invoking KinectManager.DetectGesture()-function.
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
            gestureManager.DetectGesture(userId, GestureType.ZoomOut);
            gestureManager.DetectGesture(userId, GestureType.ZoomIn);
            gestureManager.DetectGesture(userId, GestureType.Wheel);

            gestureManager.DetectGesture(userId, GestureType.RaiseLeftHand);
            gestureManager.DetectGesture(userId, GestureType.RaiseRightHand);

            if (gestureInfo != null)
            {
                gestureInfo.text = "Zoom-in or wheel to rotate the model.\nRaise hand to reset it.";
            }
        }

        /// <summary>
        /// Invoked when a user gets lost. All tracked gestures for this user are cleared automatically.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userIndex">User index</param>
        public void UserLost(ulong userId, int userIndex)
        {
            // the gestures are allowed for the primary user only
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
            // the gestures are allowed for the primary user only
            if (userIndex != playerIndex)
                return;

            if (gesture == GestureType.ZoomOut)
            {
                if (progress > 0.5f)
                {
                    zoomOut = true;
                    zoomFactor = screenPos.z;

                    if (gestureInfo != null)
                    {
                        string sGestureText = string.Format("{0} factor: {1:F0}%", gesture, screenPos.z * 100f);
                        gestureInfo.text = sGestureText;

                        progressDisplayed = true;
                        progressGestureTime = Time.realtimeSinceStartup;
                    }
                }
                else
                {
                    zoomOut = false;
                }
            }
            else if (gesture == GestureType.ZoomIn)
            {
                if (progress > 0.5f)
                {
                    zoomIn = true;
                    zoomFactor = screenPos.z;

                    if (gestureInfo != null)
                    {
                        string sGestureText = string.Format("{0} factor: {1:F0}%", gesture, screenPos.z * 100f);
                        gestureInfo.text = sGestureText;

                        progressDisplayed = true;
                        progressGestureTime = Time.realtimeSinceStartup;
                    }
                }
                else
                {
                    zoomIn = false;
                }
            }
            else if (gesture == GestureType.Wheel)
            {
                if (progress > 0.5f)
                {
                    wheel = true;
                    wheelAngle = screenPos.z;

                    if (gestureInfo != null)
                    {
                        string sGestureText = string.Format("Wheel angle: {0:F0} deg.", screenPos.z);
                        gestureInfo.text = sGestureText;

                        progressDisplayed = true;
                        progressGestureTime = Time.realtimeSinceStartup;
                    }
                }
                else
                {
                    wheel = false;
                }
            }
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
            // the gestures are allowed for the primary user only
            if (userIndex != playerIndex)
                return false;

            if (gesture == GestureType.RaiseLeftHand)
                raiseHand = true;
            else if (gesture == GestureType.RaiseRightHand)
                raiseHand = true;

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
            // the gestures are allowed for the primary user only
            if (userIndex != playerIndex)
                return false;

            if (gesture == GestureType.ZoomOut)
            {
                zoomOut = false;
            }
            else if (gesture == GestureType.ZoomIn)
            {
                zoomIn = false;
            }
            else if (gesture == GestureType.Wheel)
            {
                wheel = false;
            }

            if (gestureInfo != null && progressDisplayed)
            {
                progressDisplayed = false;
                gestureInfo.text = "Zoom-in or wheel to rotate the model.\nRaise hand to reset it."; ;
            }

            return true;
        }


        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (progressDisplayed && ((Time.realtimeSinceStartup - progressGestureTime) > 2f))
            {
                progressDisplayed = false;
                gestureInfo.text = string.Empty;

                Debug.Log("Forced progress to end.");
            }
        }

    }
}
