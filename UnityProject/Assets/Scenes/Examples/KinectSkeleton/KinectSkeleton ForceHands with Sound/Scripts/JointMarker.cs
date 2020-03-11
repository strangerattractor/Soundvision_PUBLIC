using UnityEngine;

namespace cylvester
{
    public class JointMarker : MonoBehaviour
    {

        private Vector3 velocity;
        [SerializeField] private Vector3 offset;
        [SerializeField] public float smoothTime = .5f;

        private float previousCallback_;

        public void OnJointDataReceived(Windows.Kinect.Joint joint)
        {
            Vector3 newPosition = new Vector3(joint.Position.X * 10f, joint.Position.Y * 10f, joint.Position.Z * 10f);
            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime) + offset;
        }
    }
}