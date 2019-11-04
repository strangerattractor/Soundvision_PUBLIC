using UnityEngine;

namespace cylvester
{
    public class KinectPointCloud : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;



        private void Start()
        {
            var mesh = new Mesh {vertices = CreatePlane(512, 424)};
            mesh.SetIndices(CreateIndices(512 * 424), MeshTopology.Points, 0);
            meshFilter.mesh = mesh;
        }

        private Vector3[] CreatePlane(int columns, int rows)
        {
            var plane = new Vector3[columns * rows];

            var count = 0;
            for (var i = 0; i < rows; ++i)
            {
                var y = (float) i / (rows - 1) * 2f - 1f;
                for (var j = 0; j < columns; ++j)
                {
                    var x = (float) j / (columns - 1) * 2f - 1f;
                    plane[count] = new Vector3(x, y, 0f);
                    count++;
                }
            }

            return plane;
        }

        private int[] CreateIndices(int num)
        {
            var indices = new int[num];
            for (var i = 0; i < num; ++i)
                indices[i] = i;
            return indices;
        }    
        
        public void OnInfraredFrameReceived( Texture2D data)
        {
            
        }
    }

}

