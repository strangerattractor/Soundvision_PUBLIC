using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// UserBodyBlender allows visibility of the user's body parts that stand in front of the virtual clothing. 
    /// </summary>
    public class UserBodyBlender : MonoBehaviour
    {
        [Tooltip("Allowed depth distance between the user and the clothing model, in meters.")]
        [Range(-0.5f, 0.5f)]
        public float depthThreshold = 0.1f;

        [Tooltip("RawImage used to display the scene background.")]
        public UnityEngine.UI.RawImage backgroundImage;

        [Tooltip("Index of the depth sensor that generates the color camera background. 0 is the 1st one, 1 - the 2nd one, etc.")]
        private int sensorIndex = 0;

        private Material userBlendMat = null;
        private KinectManager kinectManager = null;
        //private BackgroundRemovalManager backManager = null;
        private KinectInterop.SensorData sensorData = null;
        //private ulong lastColorDepthBufferTime = 0;

        private Rect shaderUvRect = new Rect(0, 0, 1, 1);
        private bool shaderRectInited = false;

        //private float depthFactor = 1f;

        private RenderTexture copyToTex;


        // sets texture to copy to
        /// <summary>
        /// Sets the texture to copy the camera image to.
        /// </summary>
        /// <param name="tex">The target texture</param>
        public void SetCopyToTexture(RenderTexture tex)
        {
            copyToTex = tex;
        }


        void OnEnable()
        {
            // set camera to clear the background
            Camera thisCamera = gameObject.GetComponent<Camera>();
            if (thisCamera)
            {
                thisCamera.depthTextureMode = DepthTextureMode.Depth;
            }
        }


        void OnDisable()
        {
            // set camera to clear the depth buffser only
            Camera thisCamera = gameObject.GetComponent<Camera>();
            if (thisCamera)
            {
                thisCamera.depthTextureMode = DepthTextureMode.None;
            }
        }


        void Start()
        {
            kinectManager = KinectManager.Instance;
            //backManager = BackgroundRemovalManager.Instance;

            if (kinectManager && kinectManager.IsInitialized())
            {
                Shader userBlendShader = Shader.Find("Kinect/UserBlendShader");
                sensorData = kinectManager.GetSensorData(sensorIndex);

                if (userBlendShader != null && sensorData != null)
                {
                    userBlendMat = new Material(userBlendShader);

                    if (sensorData.colorDepthBuffer == null && sensorData.colorImageWidth > 0 && sensorData.colorImageHeight > 0)
                    {
                        int bufferLength = sensorData.colorImageWidth * sensorData.colorImageHeight / 2;
                        sensorData.colorDepthBuffer = new ComputeBuffer(bufferLength, sizeof(uint));
                    }
                }
            }
            else
            {
                // disable the component
                gameObject.GetComponent<UserBodyBlender>().enabled = false;
            }
        }

        void OnDestroy()
        {
            if (sensorData != null && sensorData.colorDepthBuffer != null)
            {
                sensorData.colorDepthBuffer.Dispose();
                sensorData.colorDepthBuffer = null;
            }
        }

        void Update()
        {
            if (!shaderRectInited)
            {
                PortraitBackground portraitBack = PortraitBackground.Instance;
                if (portraitBack && portraitBack.IsInitialized())
                {
                    shaderUvRect = portraitBack.GetShaderUvRect();
                }

                shaderRectInited = true;
            }

            if (kinectManager && kinectManager.IsInitialized() && userBlendMat != null)
            {
                if (sensorData != null && sensorData.colorDepthBuffer != null && sensorData.colorImageTexture &&
                    sensorData.usedColorDepthBufferTime != sensorData.lastColorDepthBufferTime)
                {
                    sensorData.usedColorDepthBufferTime = sensorData.lastColorDepthBufferTime;

                    userBlendMat.SetFloat("_ColorResX", (float)sensorData.colorImageWidth);
                    userBlendMat.SetFloat("_ColorResY", (float)sensorData.colorImageHeight);
                    userBlendMat.SetFloat("_ColorScaleX", (float)sensorData.colorImageScale.x);

                    userBlendMat.SetFloat("_ColorOfsX", shaderUvRect.x);
                    userBlendMat.SetFloat("_ColorMulX", shaderUvRect.width);
                    userBlendMat.SetFloat("_ColorOfsY", shaderUvRect.y);
                    userBlendMat.SetFloat("_ColorMulY", shaderUvRect.height);

                    if (backgroundImage)
                    {
                        userBlendMat.SetTexture("_BackTex", backgroundImage.texture);
                    }

                    // color camera texture
                    //Texture colorTex = backManager && sensorData.color2DepthTexture ? (Texture)sensorData.color2DepthTexture : sensorData.colorImageTexture;
                    //userBlendMat.SetTexture("_ColorTex", colorTex);
                    userBlendMat.SetTexture("_ColorTex", sensorData.colorImageTexture);  // sensorData.colorDepthTexture

                    userBlendMat.SetBuffer("_DepthMap", sensorData.colorDepthBuffer);
                }
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (userBlendMat != null)
            {
                userBlendMat.SetFloat("_Threshold", depthThreshold);
                Graphics.Blit(source, destination, userBlendMat);

                if (copyToTex != null)
                {
                    Graphics.Blit(destination, copyToTex);
                }
            }
        }
    }
}
