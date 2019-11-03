using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace cylvester
{
    public class KinectPointCloud : MonoBehaviour
    {
        [SerializeField] private VisualEffect visualEffect;

        private Texture2D cachedTexture_;
        private Texture2D data_;
        public void Start()
        {
            cachedTexture_  = new Texture2D(512, 424, TextureFormat.R16, false);
            data_ = new Texture2D(512, 424, TextureFormat.R16, false);
        }

        public void Update()
        {
            //Graphics.CopyTexture(data_, cachedTexture_);
        }
        
        public void OnInfraredFrameReceived( Texture2D data)
        {
            data_ = data;

            visualEffect.SetTexture("DepthImage", data_);
            visualEffect.SetTexture("CachedTexture", cachedTexture_);
        }
    }

}

