using UnityEngine;

namespace cylvester
{
    public class KinectManagerBehaviour : MonoBehaviour
    {
        public IKinectSensor KinectSensor { get; private set; }

        private void Awake()
        {
            var componentFactory = new ComponentFactory();
            KinectSensor = componentFactory.CreateKinectSensor();
        }

        private void Update()
        {
            KinectSensor.Update();
        }
    }
}


