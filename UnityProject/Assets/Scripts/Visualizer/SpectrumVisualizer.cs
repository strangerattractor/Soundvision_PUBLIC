using UnityEngine;

namespace cylvester
{
    
    public interface ISpectrumVisualizer
    {
        float[] Spectrum { set; }
    }
    
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SpectrumVisualizer : MonoBehaviour, ISpectrumVisualizer
    {
        private MeshFilter meshFilter_;
        private ICombMesh combMesh_;

        private void Start()
        {
            combMesh_ = new CombMesh(512, 0.1f);

            meshFilter_ = GetComponent<MeshFilter>();
            meshFilter_.mesh = new Mesh
            {
                vertices = combMesh_.Vertices,
                triangles = combMesh_.Indices
            };
        }

        public float[] Spectrum
        {
            set
            {
                combMesh_.Update(value);
                meshFilter_.mesh.vertices = combMesh_.Vertices;
            }
        }
    }
}