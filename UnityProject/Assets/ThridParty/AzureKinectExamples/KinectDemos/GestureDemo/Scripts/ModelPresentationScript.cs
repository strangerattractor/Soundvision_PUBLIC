using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace com.rfilkov.components
{
    /// <summary>
    /// ModelPresentationScript processes the zoom out/in & wheel gestures to zoom and rotate the model, as well as hand raise pose to reset it.
    /// </summary>
    public class ModelPresentationScript : MonoBehaviour
    {
        [Tooltip("Camera used for screen-to-world calculations. This is usually the main camera.")]
        public Camera screenCamera;

        [Tooltip("Speed of rotation, when the presentation model spins.")]
        public float spinSpeed = 10;

        // reference to the gesture listener
        private ModelGestureListener gestureListener;

        // model's initial rotation
        private Quaternion initialRotation;


        void Start()
        {
            // hide mouse cursor
            //Cursor.visible = false;

            // by default set the main-camera to be screen-camera
            if (screenCamera == null)
            {
                screenCamera = Camera.main;
            }

            // get model initial rotation
            initialRotation = screenCamera ? Quaternion.Inverse(screenCamera.transform.rotation) * transform.rotation : transform.rotation;

            // get the gestures listener
            gestureListener = ModelGestureListener.Instance;
        }

        void Update()
        {
            // dont run Update() if there is no gesture listener
            if (!gestureListener)
                return;

            if (gestureListener.IsZoomingIn() || gestureListener.IsZoomingOut())
            {
                // zoom the model
                float zoomFactor = gestureListener.GetZoomFactor();

                Vector3 newLocalScale = new Vector3(zoomFactor, zoomFactor, zoomFactor);
                transform.localScale = Vector3.Lerp(transform.localScale, newLocalScale, spinSpeed * Time.deltaTime);
            }

            if (gestureListener.IsTurningWheel())
            {
                // rotate the model
                float turnAngle = Mathf.Clamp(gestureListener.GetWheelAngle(), -30f, 30f);
                float updateAngle = Mathf.Lerp(0, turnAngle, spinSpeed * Time.deltaTime);

                if (screenCamera)
                    transform.RotateAround(transform.position, screenCamera.transform.TransformDirection(Vector3.up), updateAngle);
                else
                    transform.Rotate(Vector3.up * turnAngle, Space.World);
            }

            if (gestureListener.IsRaiseHand())
            {
                // reset the model
                Vector3 newLocalScale = Vector3.one;
                transform.localScale = newLocalScale;

                transform.rotation = screenCamera ? screenCamera.transform.rotation * initialRotation : initialRotation;
            }

        }

    }
}
