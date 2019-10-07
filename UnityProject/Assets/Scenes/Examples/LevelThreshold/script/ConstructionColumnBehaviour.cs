using UnityEngine;

namespace cylvester
{
    public class ConstructionColumnBehaviour : MonoBehaviour
    {
        private Vector3 direction_;
        
        private void Start()
        {
            direction_ = new Vector3(Random.Range(-1.0f, 1.0f), 0f,  Random.Range(-10.0f, 10.0f));
        }

        private void Update()
        {
            
        }
    }
}