using UnityEngine;

namespace Scenes.Examples.Example1.script
{
    public class BallMapping : MonoBehaviour
    {
        [SerializeField] private GameObject ball;
        [SerializeField] private float mapToSize = 10f;
        [SerializeField] private float mapToColor = 0f;
        [SerializeField] private RmsAnalyzer analyzer;

        private Renderer renderer_;
        
        void Start()
        {
            renderer_ = GetComponent<Renderer>();
            
        }
        
        void Update()
        {
            var currentRms = analyzer.RMS;
            var scale = currentRms * mapToSize;
            var red = Mathf.Clamp(currentRms * mapToColor, 0f, 1f);
            
            ball.transform.localScale = new Vector3(scale, scale, scale);
            renderer_.material.color = new Color(red, 1f - red, 0f);
        }
    }
}
