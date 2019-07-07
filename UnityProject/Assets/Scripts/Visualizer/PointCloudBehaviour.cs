using UnityEngine;
using VideoInput;

namespace Visualizer
{
    class PointCloudBehaviour : MonoBehaviour
    {
        [SerializeField] private KinectManagerBehaviour kinectManagerBehaviour;
        [SerializeField] private GameObject debugPanel;

        private Renderer renderer_;
        
        void Start()
        {
            renderer_ = debugPanel.GetComponent<Renderer>();
            renderer_.material.mainTexture = kinectManagerBehaviour.KinectSensor.InfraredCamera.Data;
        }

        void Update()
        {
        }
        
    }
}