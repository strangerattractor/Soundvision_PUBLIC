using System;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable]
    class UnityDifferenceTextureEvent : UnityEvent<Texture2D> { }

    public class DifferenceTexture : MonoBehaviour
    {
        [SerializeField] private UnityDifferenceTextureEvent DifferenceTextureReceived;
        
        private Texture2D cache_;
        private Texture2D difference_;
        private readonly int size_ = 512 * 424 * 2;

        private void Start()
        {
            cache_ = new Texture2D(512, 424, TextureFormat.R16, false);
            difference_ = new Texture2D(512, 424, TextureFormat.R16, false);
        }
        
        public void OnInfraredFrameReceived(Texture2D texture)
        {
            var rawDifference = difference_.GetPixels();
            var rawNewTexture = texture.GetRawTextureData();
            var rawOldTexture = cache_.GetRawTextureData();

            // perhaps the fastest way to calculate on CPU
                        
            for (var i = 0; i < rawDifference.Length; ++i)
            
            difference_.Apply();
            Graphics.CopyTexture(texture, cache_);
            DifferenceTextureReceived.Invoke(difference_);
        }
    }

}


