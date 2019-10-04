using UnityEngine;
using VideoInput;

namespace Visualizer
{
    class IRPanelBehaviour : MonoBehaviour
    {
        [SerializeField] private KinectManagerBehaviour kinectManagerBehaviour = null;
        [SerializeField] private GameObject panel = null;

        private Renderer renderer_;
        private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");

        void Start()
        {
            renderer_ = panel.GetComponent<Renderer>();
        }

        void Update()
        {
            renderer_.material.SetTexture(BaseColorMap, kinectManagerBehaviour.KinectSensor.InfraredCamera.Data);
        }
    }
}