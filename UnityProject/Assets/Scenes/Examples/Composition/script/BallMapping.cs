using UnityEngine;

namespace cylvester
{
    public class BallMapping : MonoBehaviour
    {
        [SerializeField] private GameObject ball = null;
        [SerializeField] private float mapToSize = 10f;
        [SerializeField] private RmsAnalyzer analyzer = null;
        
        void Update()
        {
            var currentRms = analyzer.RMS;
            var scale = currentRms * mapToSize;
            
            ball.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
