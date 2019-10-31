using UnityEngine;

namespace cylvester
{
    class IRPanelBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject panel = null;

        private Renderer renderer_;
        private static readonly int KinectTexture = Shader.PropertyToID("_kinectTexture");

        void Start()
        {
            renderer_ = panel.GetComponent<Renderer>();
        }

        public void OnInfraredFrameReceived(Texture2D texture)
        {
            renderer_.material.SetTexture(KinectTexture, texture);
        }
    }
}

