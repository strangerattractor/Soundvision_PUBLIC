using UnityEngine;

namespace cylvester
{
    public interface ICombMesh
    {
        Vector3[] Vertices { get; }
        int[] Indices { get; }
        void Update(float[] spectrum);
    }
    
    public class CombMesh : ICombMesh
    {
        public Vector3[] Vertices { get; }
        public int[] Indices { get; }
        private IPdArray fftArray_;
        private int numberOfTeeth_;
        
        public CombMesh (int numberOfTeeth, float gap)
        {
            numberOfTeeth_ = numberOfTeeth;
            Vertices = CreateVertices(gap);
            Indices = CreateIndices();
        }

        public void Update(float[] spectrum)
        {
            var index = numberOfTeeth_ * 2;

            for (var i = 0; i < numberOfTeeth_; ++i)
            {
                Vertices[index++].y = spectrum[i];
                Vertices[index++].y = spectrum[i];
            }
        }

        private Vector3[] CreateVertices(float gap)
        {
            var numVerticesPerLine = numberOfTeeth_ * 2;
            var numTotalLineVertices = numberOfTeeth_ * 4;
            var currentPos = -1f;
            var step = 2f / (numberOfTeeth_ - 1);
            var width = (1f - gap) * step;

            var vertices = new Vector3[numTotalLineVertices];
            for (var i = 0; i < numberOfTeeth_ - 1; ++i)
            {
                var index = i * 2;
                vertices[index] = new Vector3(currentPos, 0, 0);
                vertices[index+1] = new Vector3(currentPos + width , 0, 0);
                
                vertices[index+numVerticesPerLine] = new Vector3(currentPos, 1, 0);
                vertices[index+1+numVerticesPerLine] = new Vector3(currentPos + width, 1, 0);

                currentPos += step;
            }

            return vertices;
        }

        private int[] CreateIndices( )
        {
            var  numIndices = numberOfTeeth_ * 6;
            
            var offset = numberOfTeeth_ * 2;
            var indices = new int[numIndices];
            var index = 0;
            for (var i = 0; i < numberOfTeeth_; ++i)
            {
                var onset = i * 2;
                indices[index++] = 0 + onset;
                indices[index++] = offset + onset;
                indices[index++] = 1 + onset;

                indices[index++] = offset + onset;
                indices[index++] = offset + 1 + onset;
                indices[index++] = 1 + onset;
            }

            return indices;
        }


    }
}