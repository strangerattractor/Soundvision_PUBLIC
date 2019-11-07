using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace cylvester
{
    public class KinectPointCloud : MonoBehaviour
    {
        [SerializeField] private VisualEffect visualEffect;

        public void OnDepthImageReceived(Texture texture)
        {
            visualEffect.SetTexture("DepthImage", texture);
        }

        public void OnMovementImageReceived(Texture texture)
        {
            visualEffect.SetTexture("MovementImage", texture);
        }

    }

}

