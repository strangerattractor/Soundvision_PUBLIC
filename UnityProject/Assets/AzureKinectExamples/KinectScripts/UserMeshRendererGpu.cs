using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// UserMeshRendererGpu renders virtual mesh of a user in the scene, as detected by the given sensor. This component uses GPU for mesh processing rather than CPU. 
    /// </summary>
    public class UserMeshRendererGpu : MonoBehaviour
    {
        [Tooltip("Index of the player, tracked by this component. 0 - the 1st player only, 1 - the 2nd player only, etc.")]
        public int playerIndex = 0;

        [Tooltip("Depth sensor index - 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Tooltip("Resolution of the images used to generate the scene.")]
        private DepthSensorBase.PointCloudResolution sourceImageResolution = DepthSensorBase.PointCloudResolution.DepthCameraResolution;

        [Tooltip("Whether to show the mesh as point cloud, or as solid mesh.")]
        public bool showAsPointCloud = true;

        [Tooltip("Mesh coarse factor.")]
        [Range(1, 4)]
        public int coarseFactor = 1;

        [Tooltip("Time interval between scene mesh updates, in seconds. 0 means no wait.")]
        private float updateMeshInterval = 0f;

        [Tooltip("Time interval between mesh-collider updates, in seconds. 0 means no mesh-collider updates.")]
        private float updateColliderInterval = 0f;


        // reference to object's mesh
        private Mesh mesh = null;
        private Transform trans = null;

        // references to KM and data
        private KinectManager kinectManager = null;
        private KinectInterop.SensorData sensorData = null;
        private DepthSensorBase sensorInt = null;
        //private Vector3 spaceScale = Vector3.zero;
        private Material meshShaderMat = null;

        // space table
        private Vector3[] spaceTable = null;
        private ComputeBuffer spaceTableBuffer = null;

        // render textures 
        private RenderTexture colorTexture = null;
        private RenderTexture colorTextureCopy = null;
        private bool colorTextureCreated = false;
        private bool depthBufferCreated = false;
        private bool bodyIndexBufferCreated = false;

        // times
        private ulong lastDepthFrameTime = 0;
        private float lastMeshUpdateTime = 0f;
        private float lastColliderUpdateTime = 0f;

        // user parameters
        private ulong userId = 0;
        private int userBodyIndex = 255;
        private Vector3 userBodyPos = Vector3.zero;

        // image parameters
        private int imageWidth = 0;
        private int imageHeight = 0;

        // mesh parameters
        private bool bMeshInited = false;
        private int meshParamsCache = 0;

        void Start()
        {
            // get sensor data
            kinectManager = KinectManager.Instance;
            sensorData = (kinectManager != null && kinectManager.IsInitialized()) ? kinectManager.GetSensorData(sensorIndex) : null;
        }


        void OnDestroy()
        {
            if (bMeshInited)
            {
                // release the mesh-related resources
                FinishMesh();
            }
        }


        void Update()
        {
            if (mesh == null && sensorData != null && sensorData.depthCamIntr != null)
            {
                // init mesh and its related data
                InitMesh();
            }

            if (bMeshInited)
            {
                // user params
                userId = kinectManager.GetUserIdByIndex(playerIndex);
                userBodyIndex = userId != 0 ? kinectManager.GetBodyIndexByUserId(userId) : 255;
                userBodyPos = userId != 0 ? kinectManager.GetUserKinectPosition(userId, true) : Vector3.zero;

                // update the mesh
                UpdateMesh();
            }
        }


        // inits the mesh and related data
        private void InitMesh()
        {
            // create mesh
            mesh = new Mesh
            {
                name = "User" + playerIndex + "Mesh-S" + sensorIndex,
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.mesh = mesh;
            }
            else
            {
                Debug.LogWarning("MeshFilter not found! You may not see the mesh on screen");
            }

            // create depth image buffer
            if (sensorData.depthImageBuffer == null)
            {
                int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                sensorData.depthImageBuffer = KinectInterop.CreateComputeBuffer(sensorData.depthImageBuffer, depthBufferLength, sizeof(uint));
                depthBufferCreated = true;
            }

            // create body index buffer
            if (sensorData.bodyIndexBuffer == null)
            {
                int bodyIndexBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 4;
                sensorData.bodyIndexBuffer = KinectInterop.CreateComputeBuffer(sensorData.bodyIndexBuffer, bodyIndexBufferLength, sizeof(uint));
                bodyIndexBufferCreated = true;
            }

            // create point cloud color texture
            if (sensorData != null && sensorData.sensorInterface != null)
            {
                sensorInt = (DepthSensorBase)sensorData.sensorInterface;

                Vector2Int imageRes = Vector2Int.zero;
                if (sensorInt.pointCloudColorTexture == null)
                {
                    sensorInt.pointCloudResolution = sourceImageResolution;
                    imageRes = sensorInt.GetPointCloudTexResolution(sensorData);

                    colorTexture = KinectInterop.CreateRenderTexture(colorTexture, imageRes.x, imageRes.y, RenderTextureFormat.ARGB32);
                    sensorInt.pointCloudColorTexture = colorTexture;
                    colorTextureCreated = true;
                }
                else
                {
                    imageRes = sensorInt.GetPointCloudTexResolution(sensorData);
                    colorTexture = sensorInt.pointCloudColorTexture;
                    colorTextureCreated = false;
                }

                // create space table
                spaceTable = sensorInt.pointCloudResolution == DepthSensorBase.PointCloudResolution.DepthCameraResolution ?
                    sensorInt.GetDepthCameraSpaceTable(sensorData) : sensorInt.GetColorCameraSpaceTable(sensorData);

                int spaceBufferLength = imageRes.x * imageRes.y * 3;
                spaceTableBuffer = new ComputeBuffer(spaceBufferLength, sizeof(float));
                spaceTableBuffer.SetData(spaceTable);
                spaceTable = null;

                // create copy texture
                colorTextureCopy = KinectInterop.CreateRenderTexture(colorTextureCopy, imageRes.x, imageRes.y, RenderTextureFormat.ARGB32);

                // set the color texture
                Renderer meshRenderer = GetComponent<Renderer>();
                if (meshRenderer && meshRenderer.material /**&& meshRenderer.material.mainTexture == null*/)
                {
                    meshShaderMat = meshRenderer.material;
                }

                // get reference to the transform
                trans = GetComponent<Transform>();

                // image width & height
                imageWidth = imageRes.x;
                imageHeight = imageRes.y;

                // create mesh vertices & indices
                CreateMeshVertInd();
                bMeshInited = true;
            }
        }


        // creates the mesh vertices and indices
        private void CreateMeshVertInd()
        {
            int xVerts = (imageWidth / coarseFactor); // + 1;
            int yVerts = (imageHeight / coarseFactor); // + 1;
            int vCount = xVerts * yVerts;

            // mesh vertices
            mesh.Clear();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            Vector3[] meshVertices = new Vector3[xVerts * yVerts];
            Vector3[] meshNormals = new Vector3[xVerts * yVerts];

            float vsx = (float)coarseFactor / (float)imageWidth;
            float vsy = (float)coarseFactor / (float)imageHeight;

            for (int y = 0, vi = 0; y < yVerts; y++)
            {
                for (int x = 0; x < xVerts; x++, vi++)
                {
                    meshVertices[vi] = new Vector3(x * vsx, y * vsy, 0f);
                    meshNormals[vi] = new Vector3(0f, 1f, 0f);  // 0f, 0f, -1f
                }
            }

            mesh.vertices = meshVertices;
            mesh.normals = meshNormals;

            // mesh indices
            if (showAsPointCloud)
            {
                int[] meshIndices = new int[vCount];
                for (int i = 0; i < vCount; i++)
                    meshIndices[i] = i;

                mesh.SetIndices(meshIndices, MeshTopology.Points, 0);
            }
            else
            {
                int[] meshIndices = new int[(xVerts - 1) * (yVerts - 1) * 6];
                for (int y = 0, ii = 0; y < (yVerts - 1); y++)
                {
                    for (int x = 0; x < (xVerts - 1); x++)
                    {
                        meshIndices[ii++] = (y + 1) * xVerts + x;
                        meshIndices[ii++] = y * xVerts + x + 1;
                        meshIndices[ii++] = y * xVerts + x;

                        meshIndices[ii++] = (y + 1) * xVerts + x + 1;
                        meshIndices[ii++] = y * xVerts + x + 1;
                        meshIndices[ii++] = (y + 1) * xVerts + x;
                    }
                }

                mesh.SetIndices(meshIndices, MeshTopology.Triangles, 0);
            }

            meshParamsCache = coarseFactor + (showAsPointCloud ? 10 : 0);
        }


        // releases mesh-related resources
        private void FinishMesh()
        {
            if (sensorInt)
            {
                sensorInt.pointCloudColorTexture = null;
            }

            if(sensorData.depthImageBuffer != null && depthBufferCreated)
            {
                sensorData.depthImageBuffer.Release();
                sensorData.depthImageBuffer.Dispose();
                sensorData.depthImageBuffer = null;
            }

            if (sensorData.bodyIndexBuffer != null && bodyIndexBufferCreated)
            {
                sensorData.bodyIndexBuffer.Release();
                sensorData.bodyIndexBuffer.Dispose();
                sensorData.bodyIndexBuffer = null;
            }

            if (colorTexture && colorTextureCreated)
            {
                colorTexture.Release();
                colorTexture = null;
            }

            if (colorTextureCopy)
            {
                colorTextureCopy.Release();
                colorTextureCopy = null;
            }

            if (spaceTableBuffer != null)
            {
                spaceTableBuffer.Dispose();
                spaceTableBuffer = null;
            }

            bMeshInited = false;
        }


        // updates the mesh according to current depth frame
        private void UpdateMesh()
        {
            if (bMeshInited && colorTexture != null && sensorData.depthImage != null && sensorData.depthCamIntr != null && meshShaderMat != null &&
                lastDepthFrameTime != sensorData.lastDepthFrameTime && (Time.time - lastMeshUpdateTime) >= updateMeshInterval)
            {
                lastDepthFrameTime = sensorData.lastDepthFrameTime;
                lastMeshUpdateTime = Time.time;

                int paramsCache = coarseFactor + (showAsPointCloud ? 10 : 0);
                if(meshParamsCache != paramsCache)
                {
                    //Debug.Log("Mesh params changed. Recreating...");
                    CreateMeshVertInd();
                }

                if (sensorData.depthImageBuffer != null && sensorData.depthImage != null && depthBufferCreated)
                {
                    int depthBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 2;
                    KinectInterop.SetComputeBufferData(sensorData.depthImageBuffer, sensorData.depthImage, depthBufferLength, sizeof(uint));
                }

                if (sensorData.bodyIndexBuffer != null && sensorData.bodyIndexImage != null && bodyIndexBufferCreated)
                {
                    int bodyIndexBufferLength = sensorData.depthImageWidth * sensorData.depthImageHeight / 4;
                    KinectInterop.SetComputeBufferData(sensorData.bodyIndexBuffer, sensorData.bodyIndexImage, bodyIndexBufferLength, sizeof(uint));
                }

                Graphics.CopyTexture(colorTexture, colorTextureCopy);

                meshShaderMat.SetBuffer("_DepthMap", sensorData.depthImageBuffer);
                meshShaderMat.SetBuffer("_BodyIndexMap", sensorData.bodyIndexBuffer);
                meshShaderMat.SetInt("_CoarseFactor", coarseFactor);

                meshShaderMat.SetInt("_UserBodyIndex", userBodyIndex);
                meshShaderMat.SetVector("_UserBodyPos", userBodyPos);
                meshShaderMat.SetMatrix("_Transform", sensorInt.GetSensorToWorldMatrix());

                meshShaderMat.SetVector("_TexRes", new Vector2(imageWidth, imageHeight));
                meshShaderMat.SetVector("_Cxy", new Vector2(sensorData.depthCamIntr.ppx, sensorData.depthCamIntr.ppy));
                meshShaderMat.SetVector("_Fxy", new Vector2(sensorData.depthCamIntr.fx, sensorData.depthCamIntr.fy));

                meshShaderMat.SetTexture("_ColorTex", colorTextureCopy);
                meshShaderMat.SetVector("_SpaceScale", kinectManager.GetSensorSpaceScale(sensorIndex));  // kinectManager.GetDepthImageScale(sensorIndex)
                meshShaderMat.SetBuffer("_SpaceTable", spaceTableBuffer);

                // mesh bounds
                if (kinectManager.GetUserBoundingBox(userId, null, sensorIndex, Rect.zero, out Vector3 posMin, out Vector3 posMax))
                {
                    Vector3 boundsCenter = new Vector3((posMax.x - posMin.x) / 2f, (posMax.y - posMin.y) / 2f, (posMax.z - posMin.z) / 2f);
                    Vector3 boundsSize = new Vector3((posMax.x - posMin.x), (posMax.y - posMin.y), (posMax.z - posMin.z));
                    mesh.bounds = new Bounds(boundsCenter, boundsSize);
                }

                if (updateColliderInterval > 0 && (Time.time - lastColliderUpdateTime) >= updateColliderInterval)
                {
                    lastColliderUpdateTime = Time.time;
                    MeshCollider meshCollider = GetComponent<MeshCollider>();

                    if (meshCollider)
                    {
                        meshCollider.sharedMesh = null;
                        meshCollider.sharedMesh = mesh;
                    }
                }

            }
        }

    }
}
