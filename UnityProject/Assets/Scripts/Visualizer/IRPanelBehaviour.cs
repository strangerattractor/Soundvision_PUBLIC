using UnityEngine;
using VideoInput;

namespace Visualizer
{
    class IRPanelBehaviour : MonoBehaviour
    {
        #pragma warning disable 649
        [SerializeField] private KinectManagerBehaviour kinectManagerBehaviour;
        [SerializeField] private GameObject panel;
        #pragma warning restore 649

        private Renderer renderer_;
        private Texture2D texture2D_;

        void Start()
        {
            renderer_ = panel.GetComponent<Renderer>();
            texture2D_ = new Texture2D(512, 512);
        }

        void Update()
        {
            renderer_.material.SetTexture("_BaseColorMap", kinectManagerBehaviour.KinectSensor.InfraredCamera.Data);
        }
    }
}