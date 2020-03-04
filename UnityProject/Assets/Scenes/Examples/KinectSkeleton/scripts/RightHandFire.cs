using UnityEngine;
using UnityEngine.VFX;

namespace cylvester
{
    public class RightHandFire : MonoBehaviour
    {
        [SerializeField] private VisualEffect visualEffect;

        private Vector3 previousPosition_;
        private float previousCallback_;
        
        public void OnJointDataReceived(Windows.Kinect.Joint joint)
        {
            var newPosition = new Vector3(joint.Position.X * 10f, joint.Position.Y * 10f, joint.Position.Z * 10f);
            var delta = newPosition - previousPosition_;
            
            var vector = delta/(Time.time - previousCallback_);
            previousCallback_ = Time.time;
            previousPosition_ = newPosition;
            
            visualEffect.SetVector3("EmissionVector", vector);
            visualEffect.SetVector3("SourcePosition", newPosition);
        }
    }
}
