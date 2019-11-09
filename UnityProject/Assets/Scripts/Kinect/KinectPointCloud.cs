using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace cylvester
{
    public class KinectPointCloud : MonoBehaviour
    {
        [SerializeField] private VisualEffect visualEffect;

        public void OnDepthImageReceived(Texture2D texture)
        {
            visualEffect.SetTexture("DepthImage", texture);
        }

        public void OnMovementImageReceived(Texture2D texture)
        {
            visualEffect.SetTexture("MovementImage", texture);
        }
    }
}

