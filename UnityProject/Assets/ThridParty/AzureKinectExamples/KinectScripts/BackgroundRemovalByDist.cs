using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.rfilkov.kinect
{
    /// <summary>
    /// BackgroundRemovalByDist filters part of the real environment, according to the given spatial limits.
    /// </summary>
    public class BackgroundRemovalByDist : MonoBehaviour
    {
        [Tooltip("Horizontal limit - minimum, in meters.")]
        [Range(-5f, 5f)]
        public float xMin = -1f;

        [Tooltip("Horizontal limit - maximum, in meters.")]
        [Range(-5f, 5f)]
        public float xMax = 1f;

        [Tooltip("Vertical limit - minimum, in meters.")]
        [Range(-1f, 5f)]
        public float yMin = 0f;

        [Tooltip("Vertical limit - maximum, in meters.")]
        [Range(-1f, 5f)]
        public float yMax = 2f;

        [Tooltip("Distance limit - minimum, in meters.")]
        [Range(0.5f, 10f)]
        public float zMin = 1f;

        [Tooltip("Distance limit - maximum, in meters.")]
        [Range(0.5f, 10f)]
        public float zMax = 3f;

        // foreground filter shader
        private ComputeShader foregroundFilterShader = null;
        private int foregroundFilterKernel = -1;


        void Start()
        {
            foregroundFilterShader = Resources.Load("ForegroundFiltDistShader") as ComputeShader;
            foregroundFilterKernel = foregroundFilterShader != null ? foregroundFilterShader.FindKernel("FgFiltDist") : -1;
        }


        /// <summary>
        /// Applies vertex filter by distance.
        /// </summary>
        /// <param name="vertexTexture">The vertex texture</param>
        public void ApplyVertexFilter(RenderTexture vertexTexture, Matrix4x4 sensorWorldMatrix)
        {
            //foregroundFilterShader.SetMatrix("Transform", sensorWorldMatrix);
            Matrix4x4 matWorldKinect = sensorWorldMatrix.inverse;

            Vector3 posMin = matWorldKinect.MultiplyPoint3x4(new Vector3(xMin, yMin, zMin));
            Vector3 posMaxX = matWorldKinect.MultiplyPoint3x4(new Vector3(xMax, yMin, zMin)) - posMin;
            Vector3 posMaxY = matWorldKinect.MultiplyPoint3x4(new Vector3(xMin, yMax, zMin)) - posMin;
            Vector3 posMaxZ = matWorldKinect.MultiplyPoint3x4(new Vector3(xMin, yMin, zMax)) - posMin;
            Vector3 posDot = new Vector3(Vector3.Dot(posMaxX, posMaxX), Vector3.Dot(posMaxY, posMaxY), Vector3.Dot(posMaxZ, posMaxZ));

            foregroundFilterShader.SetVector("_PosMin", posMin);
            foregroundFilterShader.SetVector("_PosMaxX", posMaxX);
            foregroundFilterShader.SetVector("_PosMaxY", posMaxY);
            foregroundFilterShader.SetVector("_PosMaxZ", posMaxZ);
            foregroundFilterShader.SetVector("_PosDot", posDot);

            foregroundFilterShader.SetTexture(foregroundFilterKernel, "VertexTex", vertexTexture);
            foregroundFilterShader.Dispatch(foregroundFilterKernel, vertexTexture.width / 8, vertexTexture.height / 8, 1);
        }

    }
}
