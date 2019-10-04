using UnityEngine;

namespace Scenes.Examples.Example1.script
{
    public class BallMapping : MonoBehaviour
    {
        [SerializeField] private GameObject ball;
        [SerializeField] private float mapToSize = 10f;
        [SerializeField] private RmsAnalyzer analyzer;
        
        void Update()
        {
            var currentRms = analyzer.RMS;
            var scale = currentRms * mapToSize;
            
            ball.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
