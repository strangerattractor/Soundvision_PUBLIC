using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using com.rfilkov.components;


namespace com.rfilkov.kinect
{
    /// <summary>
    /// Background removal manager is the component that filters and renders user body silhouettes.
    /// </summary>
    public class BackgroundRemovalManager : MonoBehaviour
    {
        [Tooltip("Depth sensor index - 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Tooltip("Index of the player, tracked by this component. -1 means all players, 0 - the 1st player only, 1 - the 2nd player only, etc.")]
        public int playerIndex = -1;

        [Tooltip("RawImage used for displaying the foreground image.")]
        public UnityEngine.UI.RawImage foregroundImage;

        [Tooltip("Camera used for alignment of bodies to color camera image.")]
        public Camera foregroundCamera;

        [Tooltip("Resolution of the generated foreground textures.")]
        private DepthSensorBase.PointCloudResolution foregroundImageResolution = DepthSensorBase.PointCloudResolution.ColorCameraResolution;

        [Tooltip("Offset from the lowest body joint to the floor.")]
        [Range(0f, 0.1f)]
        public float offsetToFloor = 0.05f;

        [Tooltip("Whether only the alpha texture is needed.")]
        public bool computeAlphaMaskOnly = false;

        [Tooltip("Whether the alpha texture will be inverted or not..")]
        public bool invertAlphaMask = false;

        [Tooltip("(Advanced) Whether to apply the median filter before the other filters.")]
        public bool applyMedianFilter = false;

        [Tooltip("(Advanced) Number of iterations used by the alpha texture's erode filter 0.")]
        [Range(0, 9)]
        public int erodeIterations0 = 0;  // 1

        [Tooltip("(Advanced) Number of iterations used by the alpha texture's dilate filter 1.")]
        [Range(0, 9)]
        public int dilateIterations = 0;  // 3;

        [Tooltip("(Advanced) Whether to apply the gradient filter.")]
        private bool applyGradientFilter = true;

        [Tooltip("(Advanced) Number of iterations used by the alpha texture's erode filter 2.")]
        [Range(0, 9)]
        public int erodeIterations = 0;  // 4;

        [Tooltip("(Advanced) Whether to apply the blur filter after at the end.")]
        public bool applyBlurFilter = true;

        [Tooltip("(Advanced) Color applied to the body contour after the filters.")]
        public Color bodyContourColor = Color.black;

        [Tooltip("UI-Text to display the BR-Manager debug messages.")]
        public UnityEngine.UI.Text debugText;


        // max number of bodies to track
        private const int MAX_BODY_COUNT = 10;

        // primary sensor data structure
        private KinectInterop.SensorData sensorData = null;
        private KinectManager kinectManager = null;

        // sensor interface
        private DepthSensorBase sensorInt = null;

        // render texture resolution
        private Vector2Int textureRes;

        // Bool to keep track whether Kinect and BR library have been initialized
        private bool bBackgroundRemovalInited = false;

        // The single instance of BackgroundRemovalManager
        //private static BackgroundRemovalManager instance;

        // last point cloud frame time
        private ulong lastDepth2SpaceFrameTime = 0;

        // render textures used by the shaders
        private RenderTexture colorTexture = null;
        private RenderTexture vertexTexture = null;
        private RenderTexture alphaTexture = null;
        private RenderTexture foregroundTexture = null;

        // Materials used to apply the shaders
        private Material medianFilterMat = null;
        private Material erodeFilterMat = null;
        private Material dilateFilterMat = null;
        private Material gradientFilterMat = null;
        private Material blurFilterMat = null;
        private Material invertAlphaMat = null;
        private Material foregroundMat = null;

        // foreground filter shader
        private ComputeShader foregroundFilterShader = null;
        private int foregroundFilterKernel = -1;

        //private Vector4[] foregroundFilterPos = null;
        private Vector4[] bodyPosMin = null;
        private Vector4[] bodyPosMaxX = null;
        private Vector4[] bodyPosMaxY = null;
        private Vector4[] bodyPosMaxZ = null;
        private Vector4[] bodyPosDot = null;

        // reference to filter-by-distance component
        private BackgroundRemovalByDist filterByDist = null;


        ///// <summary>
        ///// Gets the single BackgroundRemovalManager instance.
        ///// </summary>
        ///// <value>The BackgroundRemovalManager instance.</value>
        //public static BackgroundRemovalManager Instance
        //{
        //    get
        //    {
        //        return instance;
        //    }
        //}

        /// <summary>
        /// Determines whether the BackgroundRemovalManager was successfully initialized.
        /// </summary>
        /// <returns><c>true</c> if the BackgroundRemovalManager was successfully initialized; otherwise, <c>false</c>.</returns>
        public bool IsBackgroundRemovalInited()
        {
            return bBackgroundRemovalInited;
        }

        /// <summary>
        /// Gets the foreground image texture.
        /// </summary>
        /// <returns>The foreground image texture.</returns>
        public Texture GetForegroundTex()
        {
            return foregroundTexture;
        }

        /// <summary>
        /// Gets the alpha texture.
        /// </summary>
        /// <returns>The alpha texture.</returns>
        public Texture GetAlphaTex()
        {
            return alphaTexture;
        }

        /// <summary>
        /// Gets the color texture.
        /// </summary>
        /// <returns>The color texture.</returns>
        public Texture GetColorTex()
        {
            return colorTexture;
        }

        /// <summary>
        /// Gets the last background removal frame time.
        /// </summary>
        /// <returns>The last background removal time.</returns>
        public ulong GetLastBackgroundRemovalTime()
        {
            return lastDepth2SpaceFrameTime;
        }

        //----------------------------------- end of public functions --------------------------------------//

        //void Awake()
        //{
        //    instance = this;
        //}

        public void Start()
        {
            try
            {
                // get sensor data
                kinectManager = KinectManager.Instance;
                if (kinectManager && kinectManager.IsInitialized())
                {
                    sensorData = kinectManager.GetSensorData(sensorIndex);
                }

                if (sensorData == null || sensorData.sensorInterface == null)
                {
                    throw new Exception("Background removal cannot be started, because KinectManager is missing or not initialized.");
                }

                if(foregroundImage == null)
                {
                    // look for a foreground image
                    foregroundImage = GetComponent<UnityEngine.UI.RawImage>();
                }

                if (!foregroundCamera)
                {
                    // by default - the main camera
                    foregroundCamera = Camera.main;
                }

                // try to get reference to filter-by-dist component
                filterByDist = GetComponent<BackgroundRemovalByDist>();

                // Initialize the background removal
                bool bSuccess = InitBackgroundRemoval(sensorData);

                if (bSuccess)
                {
                    if (debugText != null)
                        debugText.text = string.Empty;
                }
                else
                {
                    throw new Exception("Background removal could not be initialized.");
                }

                bBackgroundRemovalInited = bSuccess;
            }
            catch (DllNotFoundException ex)
            {
                Debug.LogError(ex.ToString());
                if (debugText != null)
                    debugText.text = "Please check the SDK installations.";
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (debugText != null)
                    debugText.text = ex.Message;
            }
        }

        void OnDestroy()
        {
            if (bBackgroundRemovalInited)
            {
                // finish background removal
                FinishBackgroundRemoval(sensorData);
            }

            bBackgroundRemovalInited = false;
            //instance = null;
        }

        void Update()
        {
            if (bBackgroundRemovalInited)
            {
                // update the background removal
                UpdateBackgroundRemoval(sensorData);

                // check for valid foreground image texture
                if(foregroundImage != null && foregroundImage.texture == null)
                {
                    foregroundImage.texture = foregroundTexture;
                    foregroundImage.rectTransform.localScale = kinectManager.GetColorImageScale(sensorIndex);
                    foregroundImage.color = Color.white;
                }
            }
        }


        // initializes background removal with shaders
        private bool InitBackgroundRemoval(KinectInterop.SensorData sensorData)
        {
            if (sensorData != null && sensorData.sensorInterface != null && KinectInterop.IsDirectX11Available())
            {
                sensorInt = (DepthSensorBase)sensorData.sensorInterface;
                if (sensorInt.pointCloudColorTexture != null || sensorInt.pointCloudVertexTexture != null)
                {
                    Debug.LogError("The sensor-interface's point cloud textures are already in use!");
                    return false;
                }

                // set the texture resolution
                sensorInt.pointCloudResolution = foregroundImageResolution;
                textureRes = sensorInt.GetPointCloudTexResolution(sensorData);

                colorTexture = KinectInterop.CreateRenderTexture(colorTexture, textureRes.x, textureRes.y, RenderTextureFormat.ARGB32);
                vertexTexture = KinectInterop.CreateRenderTexture(vertexTexture, textureRes.x, textureRes.y, RenderTextureFormat.ARGBHalf);
                alphaTexture = KinectInterop.CreateRenderTexture(alphaTexture, textureRes.x, textureRes.y, RenderTextureFormat.ARGB32);
                foregroundTexture = KinectInterop.CreateRenderTexture(foregroundTexture, textureRes.x, textureRes.y, RenderTextureFormat.ARGB32);

                sensorInt.pointCloudColorTexture = colorTexture;
                sensorInt.pointCloudVertexTexture = vertexTexture;

                Shader erodeShader = Shader.Find("Kinect/ErodeShader");
                erodeFilterMat = new Material(erodeShader);
                erodeFilterMat.SetFloat("_TexResX", (float)textureRes.x);
                erodeFilterMat.SetFloat("_TexResY", (float)textureRes.y);
                //sensorData.erodeBodyMaterial.SetTexture("_MainTex", sensorData.bodyIndexTexture);

                Shader dilateShader = Shader.Find("Kinect/DilateShader");
                dilateFilterMat = new Material(dilateShader);
                dilateFilterMat.SetFloat("_TexResX", (float)textureRes.x);
                dilateFilterMat.SetFloat("_TexResY", (float)textureRes.y);
                //sensorData.dilateBodyMaterial.SetTexture("_MainTex", sensorData.bodyIndexTexture);

                Shader gradientShader = Shader.Find("Kinect/GradientShader");
                gradientFilterMat = new Material(gradientShader);

                Shader medianShader = Shader.Find("Kinect/MedianShader");
                medianFilterMat = new Material(medianShader);
                //sensorData.medianBodyMaterial.SetFloat("_Amount", 1.0f);

                Shader blurShader = Shader.Find("Kinect/BlurShader");
                blurFilterMat = new Material(blurShader);

                Shader invertShader = Shader.Find("Kinect/InvertShader");
                invertAlphaMat = new Material(invertShader);

                Shader foregroundShader = Shader.Find("Kinect/ForegroundShader");
                foregroundMat = new Material(foregroundShader);

                if(filterByDist == null)
                {
                    foregroundFilterShader = Resources.Load("ForegroundFiltBodyShader") as ComputeShader;
                    foregroundFilterKernel = foregroundFilterShader != null ? foregroundFilterShader.FindKernel("FgFiltBody") : -1;

                    //foregroundFilterPos = new Vector4[KinectInterop.Constants.MaxBodyCount];
                    bodyPosMin = new Vector4[MAX_BODY_COUNT];
                    bodyPosMaxX = new Vector4[MAX_BODY_COUNT];
                    bodyPosMaxY = new Vector4[MAX_BODY_COUNT];
                    bodyPosMaxZ = new Vector4[MAX_BODY_COUNT];
                    bodyPosDot = new Vector4[MAX_BODY_COUNT];
                }

                return true;
            }

            return false;
        }


        // releases background removal shader resources
        private void FinishBackgroundRemoval(KinectInterop.SensorData sensorData)
        {
            if(sensorInt)
            {
                sensorInt.pointCloudColorTexture = null;
                sensorInt.pointCloudVertexTexture = null;
            }

            if (colorTexture)
            {
                colorTexture.Release();
                colorTexture = null;
            }

            if (vertexTexture)
            {
                vertexTexture.Release();
                vertexTexture = null;
            }

            if (alphaTexture)
            {
                alphaTexture.Release();
                alphaTexture = null;
            }

            if(foregroundTexture)
            {
                foregroundTexture.Release();
                foregroundTexture = null;
            }

            erodeFilterMat = null;
            dilateFilterMat = null;
            medianFilterMat = null;
            blurFilterMat = null;
            invertAlphaMat = null;
            foregroundMat = null;
            
            if(foregroundFilterShader != null)
            {
                foregroundFilterShader = null;
            }

            //foregroundFilterPos = null;
            bodyPosMin = null;
            bodyPosMaxX = null;
            bodyPosMaxY = null;
            bodyPosMaxZ = null;
            bodyPosDot = null;
        }


        // computes current background removal texture
        private bool UpdateBackgroundRemoval(KinectInterop.SensorData sensorData)
        {
            if (bBackgroundRemovalInited && lastDepth2SpaceFrameTime != sensorData.lastDepth2SpaceFrameTime)
            {
                lastDepth2SpaceFrameTime = sensorData.lastDepth2SpaceFrameTime;

                RenderTexture[] tempTextures = new RenderTexture[2];
                tempTextures[0] = RenderTexture.GetTemporary(textureRes.x, textureRes.y, 0);
                tempTextures[1] = RenderTexture.GetTemporary(textureRes.x, textureRes.y, 0);

                RenderTexture[] tempGradTextures = null;
                if (applyGradientFilter)
                {
                    tempGradTextures = new RenderTexture[2];
                    tempGradTextures[0] = RenderTexture.GetTemporary(textureRes.x, textureRes.y, 0);
                    tempGradTextures[1] = RenderTexture.GetTemporary(textureRes.x, textureRes.y, 0);
                }

                // filter
                if(filterByDist != null && sensorInt != null)
                {
                    // filter by distance
                    filterByDist.ApplyVertexFilter(vertexTexture, sensorInt.GetSensorToWorldMatrix());
                }
                else if (foregroundFilterShader != null && sensorInt != null)
                {
                    // filter by bodies
                    ApplyForegroundFilterByBody();
                }

                Graphics.Blit(vertexTexture, alphaTexture);

                // median
                if (applyMedianFilter)
                {
                    ApplySimpleFilter(vertexTexture, alphaTexture, medianFilterMat, tempTextures);
                }
                else
                {
                    Graphics.Blit(vertexTexture, alphaTexture);
                }

                // erode0
                ApplyIterableFilter(alphaTexture, alphaTexture, erodeFilterMat, erodeIterations0, tempTextures);
                if(applyGradientFilter)
                {
                    Graphics.CopyTexture(alphaTexture, tempGradTextures[0]);
                }

                // dilate
                ApplyIterableFilter(alphaTexture, alphaTexture, dilateFilterMat, dilateIterations, tempTextures);
                if (applyGradientFilter)
                {
                    //Graphics.Blit(alphaTexture, tempGradTextures[1]);
                    gradientFilterMat.SetTexture("_ErodeTex", tempGradTextures[0]);
                    ApplySimpleFilter(alphaTexture, tempGradTextures[1], gradientFilterMat, tempTextures);
                }

                // erode
                ApplyIterableFilter(alphaTexture, alphaTexture, erodeFilterMat, erodeIterations, tempTextures);
                if (tempGradTextures != null)
                {
                    Graphics.Blit(alphaTexture, tempGradTextures[0]);
                }

                // blur
                if(applyBlurFilter)
                {
                    ApplySimpleFilter(alphaTexture, alphaTexture, blurFilterMat, tempTextures);
                }

                if(invertAlphaMask)
                {
                    ApplySimpleFilter(alphaTexture, alphaTexture, invertAlphaMat, tempTextures);
                }

                if(!computeAlphaMaskOnly)
                {
                    foregroundMat.SetTexture("_ColorTex", colorTexture);
                    foregroundMat.SetTexture("_GradientTex", tempGradTextures[1]);

                    Color gradientColor = (erodeIterations0 != 0 || dilateIterations != 0 || erodeIterations != 0) ? bodyContourColor : Color.clear;
                    foregroundMat.SetColor("_GradientColor", gradientColor);

                    ApplySimpleFilter(alphaTexture, foregroundTexture, foregroundMat, tempTextures);
                }
                else
                {
                    Graphics.CopyTexture(alphaTexture, foregroundTexture);
                }

                // cleanup
                if (tempGradTextures != null)
                {
                    RenderTexture.ReleaseTemporary(tempGradTextures[0]);
                    RenderTexture.ReleaseTemporary(tempGradTextures[1]);
                }

                RenderTexture.ReleaseTemporary(tempTextures[0]);
                RenderTexture.ReleaseTemporary(tempTextures[1]);
            }

            return true;
        }

        // applies foreground filter by body
        private void ApplyForegroundFilterByBody()
        {
            Matrix4x4 matKinectWorld = sensorInt.GetSensorToWorldMatrix();
            //foregroundFilterShader.SetMatrix("_Transform", matKinectWorld);
            //foregroundFilterShader.SetFloat("Distance", 1f);

            Matrix4x4 matWorldKinect = matKinectWorld.inverse;
            if (kinectManager != null && kinectManager.userManager != null)
            {
                List<ulong> alUserIds = null;

                if (playerIndex < 0)
                {
                    alUserIds = kinectManager.userManager.alUserIds;
                }
                else
                {
                    alUserIds = new List<ulong>();

                    ulong userId = kinectManager.GetUserIdByIndex(playerIndex);
                    if (userId != 0)
                        alUserIds.Add(userId);
                }

                int uCount = Mathf.Min(alUserIds.Count, MAX_BODY_COUNT);
                foregroundFilterShader.SetInt("_NumBodies", uCount);

                // get the background rectangle (use the portrait background, if available)
                Rect backgroundRect = foregroundCamera.pixelRect;
                PortraitBackground portraitBack = PortraitBackground.Instance;

                if (portraitBack && portraitBack.enabled)
                {
                    backgroundRect = portraitBack.GetBackgroundRect();
                }

                int jCount = kinectManager.GetJointCount();
                for (int i = 0; i < uCount; i++)
                {
                    ulong userId = alUserIds[i];
                    //foregroundFilterPos[i] = kinectManager.GetUserPosition(userId);

                    //float xMin = float.MaxValue, xMax = float.MinValue;
                    //float yMin = float.MaxValue, yMax = float.MinValue;
                    //float zMin = float.MaxValue, zMax = float.MinValue;

                    //for (int j = 0; j < jCount; j++)
                    //{
                    //    if(kinectManager.IsJointTracked(userId, j))
                    //    {
                    //        Vector3 jPos = kinectManager.GetJointPosColorOverlay(userId, j, sensorIndex, foregroundCamera, backgroundRect);

                    //        if (jPos.x < xMin) xMin = jPos.x;
                    //        if (jPos.y < yMin) yMin = jPos.y;
                    //        if (jPos.z < zMin) zMin = jPos.z;

                    //        if (jPos.x > xMax) xMax = jPos.x;
                    //        if (jPos.y > yMax) yMax = jPos.y;
                    //        if (jPos.z > zMax) zMax = jPos.z;
                    //    }
                    //}

                    bool bSuccess = kinectManager.GetUserBoundingBox(userId, foregroundCamera, sensorIndex, backgroundRect, 
                        out Vector3 pMin, out Vector3 pMax);

                    if (bSuccess)
                    {
                        Vector3 posMin = new Vector3(pMin.x - 0.1f, pMin.y - offsetToFloor, pMin.z - 0.1f);
                        Vector3 posMaxX = new Vector3(pMax.x + 0.1f, posMin.y, posMin.z);
                        Vector3 posMaxY = new Vector3(posMin.x, pMax.y + 0.2f, posMin.z);
                        Vector3 posMaxZ = new Vector3(posMin.x, posMin.y, pMax.z + 0.1f);

                        //foregroundFilterDistXY[i] = new Vector4(xMin - 0.1f, xMax + 0.1f, yMin - offsetToFloor, yMax + 0.1f);
                        //foregroundFilterDistZ[i] = new Vector4(zMin - 0.2f, zMax + 0.0f, 0f, 0f);
                        bodyPosMin[i] = matWorldKinect.MultiplyPoint3x4(posMin);
                        bodyPosMaxX[i] = matWorldKinect.MultiplyPoint3x4(posMaxX) - (Vector3)bodyPosMin[i];
                        bodyPosMaxY[i] = matWorldKinect.MultiplyPoint3x4(posMaxY) - (Vector3)bodyPosMin[i];
                        bodyPosMaxZ[i] = matWorldKinect.MultiplyPoint3x4(posMaxZ) - (Vector3)bodyPosMin[i];
                        bodyPosDot[i] = new Vector3(Vector3.Dot(bodyPosMaxX[i], bodyPosMaxX[i]), Vector3.Dot(bodyPosMaxY[i], bodyPosMaxY[i]), Vector3.Dot(bodyPosMaxZ[i], bodyPosMaxZ[i]));
                    }

                    //string sMessage2 = string.Format("Xmin: {0:F1}; Xmax: {1:F1}", bodyPosMin[i].x, bodyPosMaxX[i].x);
                    //Debug.Log(sMessage2);
                }
            }

            //foregroundFilterShader.SetVectorArray("BodyPos", foregroundFilterPos);
            foregroundFilterShader.SetVectorArray("_BodyPosMin", bodyPosMin);
            foregroundFilterShader.SetVectorArray("_BodyPosMaxX", bodyPosMaxX);
            foregroundFilterShader.SetVectorArray("_BodyPosMaxY", bodyPosMaxY);
            foregroundFilterShader.SetVectorArray("_BodyPosMaxZ", bodyPosMaxZ);
            foregroundFilterShader.SetVectorArray("_BodyPosDot", bodyPosDot);

            foregroundFilterShader.SetTexture(foregroundFilterKernel, "_VertexTex", vertexTexture);
            foregroundFilterShader.Dispatch(foregroundFilterKernel, textureRes.x / 8, textureRes.y / 8, 1);
        }

        // applies iterable filter to the source texture
        private void ApplyIterableFilter(RenderTexture source, RenderTexture destination, Material filterMat, int numIterations, RenderTexture[] tempTextures)
        {
            if (!source || !destination || !filterMat || numIterations == 0)
                return;

            Graphics.Blit(source, tempTextures[0]);

            for (int i = 0; i < numIterations; i++)
            {
                Graphics.Blit(tempTextures[i % 2], tempTextures[(i + 1) % 2], filterMat);
            }

            if ((numIterations % 2) == 0)
            {
                Graphics.Blit(tempTextures[0], destination);
            }
            else
            {
                Graphics.Blit(tempTextures[1], destination);
            }
        }

        // applies simple filter to the source texture
        private void ApplySimpleFilter(RenderTexture source, RenderTexture destination, Material filterMat, RenderTexture[] tempTextures)
        {
            if (!source || !destination || !filterMat)
                return;

            Graphics.Blit(source, tempTextures[0], filterMat);
            Graphics.Blit(tempTextures[0], destination);
        }

    }
}
