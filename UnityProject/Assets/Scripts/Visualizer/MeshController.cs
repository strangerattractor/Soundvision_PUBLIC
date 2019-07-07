using UnityEngine;

namespace Visualizer
{
    public interface IMeshController
    {
        void Update();
    }
    
    public class MeshController : IMeshController
    {
        private Vector3[] points_;

        MeshController(int numPoints)
        {

        }


        public void Update()
        {
        }
    }
}