using UnityEngine;

namespace cylvester
{
    class TexturePanelBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject panel = null;

        private Renderer renderer_;
        private static readonly int KinectTexture = Shader.PropertyToID("_kinectTexture");

        void Start()
        {
            renderer_ = panel.GetComponent<Renderer>();
        }

        public void OnTextureReceived(Texture texture)
        {
            renderer_.material.SetTexture(KinectTexture, texture);
        }
    }
}

