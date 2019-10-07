using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    public class Threshold : MonoBehaviour
    {
        [SerializeField] private float threshold;
        [SerializeField] private UnityEvent thresholdExceeded;
        private bool over_;

        public void OnValueReceived(float value)
        {
			Debug.Log(value);
            if (value > threshold && !over_)
            {
                over_ = true;
                thresholdExceeded.Invoke();
            }

            if (value < threshold && over_)
                over_ = false;
        }
    }
}