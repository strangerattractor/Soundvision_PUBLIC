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
        
        void Start()
        {
            renderer_ = panel.GetComponent<Renderer>();
            renderer_.material.mainTexture = kinectManagerBehaviour.KinectSensor.InfraredCamera.Data;
        }
    }
}