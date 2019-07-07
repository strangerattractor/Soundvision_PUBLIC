using UnityEngine;
using VideoInput;

namespace Visualizer
{
    public class PointCloudBehaviour : MonoBehaviour
    {
        #pragma warning disable 649
        [SerializeField] private KinectManagerBehaviour kinectManagerBehaviour;
        [SerializeField] private MeshFilter meshFilter;
        #pragma warning restore 649

 
        private void Start()
        {
            var texture = kinectManagerBehaviour.KinectSensor.InfraredCamera.Data;
            var numPixels = texture.height * texture.width;

            meshFilter.mesh = new Mesh
            {
                vertices = MakeVertices(texture),
                uv = MakeTexCoord(texture)
            };
            meshFilter.mesh.SetIndices(MakeIndecies(numPixels), MeshTopology.Points, 0, false);
            gameObject.GetComponent<Renderer>().material.mainTexture = kinectManagerBehaviour.KinectSensor.InfraredCamera.Data;
        }

        private void Update()
        {
            
        }

        private Vector3[] MakeVertices(Texture2D texture)
        {
            var vertices = new Vector3[texture.width * texture.height];
            for (var i = 0; i < texture.height; ++i)
            {
                var offset = texture.width * i;
                var hPhase = (float) i / texture.height;
                for (var j = 0; j < texture.width; ++j)
                {
                    var wPhase = (float) j / texture.width;
                    vertices[offset + j].x = wPhase * 10f - 5f;
                    vertices[offset + j].y = hPhase * 10f - 5f;
                    vertices[offset + j].z = 0f;
                }
            }
            return vertices;
        }
        
        private Vector2[] MakeTexCoord(Texture2D texture)
        {
            var texCoords = new Vector2[texture.width * texture.height];
            for (var i = 0; i < texture.height; ++i)
            {
                var offset = texture.width * i;
                var hPhase = (float) i / texture.height;
                for (var j = 0; j < texture.width; ++j)
                {
                    var wPhase = (float) j / texture.width;
                    texCoords[offset + j].x = wPhase;
                    texCoords[offset + j].y = hPhase;
                }
            }
            return texCoords;
        }
        
        private int[] MakeIndecies(int numPixels)
        {
            var array = new int[numPixels];
            for (var i = 0; i < numPixels; ++i)
                array[i] = i;
            return array;
        }
    }
}