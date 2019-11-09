using UnityEngine;

namespace cylvester
{
    public class RightHandMarker : MonoBehaviour
    {
        private float previousCallback_;
        
        public void OnJointDataReceived(Windows.Kinect.Joint joint)
        {
            transform.position = new Vector3(joint.Position.X * 10f, joint.Position.Y * 10f, joint.Position.Z * 10f);
        }
    }
}