using UnityEngine;

namespace cylvester
{
    class IRPanelBehaviour : MonoBehaviour
    {
        [SerializeField] private KinectManagerBehaviour kinectManagerBehaviour = null;
        [SerializeField] private GameObject panel = null;

        private Renderer renderer_;
        private static readonly int KinectTexture = Shader.PropertyToID("_kinectTexture");

        void Start()
        {
            renderer_ = panel.GetComponent<Renderer>();
        }

        void Update()
        {
            renderer_.material.SetTexture(KinectTexture, kinectManagerBehaviour.KinectSensor.InfraredCamera.InfraredTexture);
        }
    }
}

