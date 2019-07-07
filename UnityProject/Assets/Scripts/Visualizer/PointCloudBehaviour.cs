using UnityEngine;
using UnityEngine.Rendering;
using VideoInput;

namespace Visualizer
{
    public class PointCloudBehaviour : MonoBehaviour
    {
        #pragma warning disable 649

        [Header("Modifier Parameters")] 
        [SerializeField] private float zScale;
        
        [Header("Connections")]
        [SerializeField] private KinectManagerBehaviour kinectManagerBehaviour;
        [SerializeField] private MeshFilter meshFilter;
        #pragma warning restore 649
        
        private Material material_;
        private static readonly int Scale = Shader.PropertyToID("_Scale");

        private void Start()
        {
            var texture = kinectManagerBehaviour.KinectSensor.InfraredCamera.Data;
            var numPixels = texture.height * texture.width;

            meshFilter.mesh = new Mesh {
                vertices = MakeVertices(texture), 
                uv = MakeTexCoord(texture),
                indexFormat = IndexFormat.UInt32
            };
            meshFilter.mesh.SetIndices(MakeIndecies(numPixels), MeshTopology.Points, 0, false);

            material_ = GetComponent<Renderer>().material;
            material_.mainTexture = kinectManagerBehaviour.KinectSensor.InfraredCamera.Data;
        }

        private void Update()
        {
            material_.SetFloat(Scale, zScale);
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
                    vertices[offset + j] = new Vector3(wPhase * 10f - 5f, (hPhase * 10f - 5f) * -1f, 0f);
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