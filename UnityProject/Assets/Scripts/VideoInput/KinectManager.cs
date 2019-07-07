using UnityEngine;

namespace VideoInput
{
    public class KinectManager : MonoBehaviour
    {
        public IKinectSensor KinectSensor { get; private set; }
    
        private void Start()
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


