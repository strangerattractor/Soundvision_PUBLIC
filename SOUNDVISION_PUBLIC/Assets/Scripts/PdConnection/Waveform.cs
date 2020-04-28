using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public interface IWaveform
    {
        Texture2D Texture { get; }
    }
    
    public class Waveform : PdBaseBindMono, IWaveform
    {
        [SerializeField] private RenderTexture renderTexture;
        
        private IPdArraySelector waveformArraySelector_;
        private Texture2D texture_;
        private int[] cache_;
        
        void Start()
        {
            base.Start();
            cache_ = new int[PdConstant.BlockSize];
            waveformArraySelector_ = new PdArraySelector(pdbackend.WaveformArrayContainer);
            texture_ = new Texture2D(PdConstant.BlockSize, PdConstant.BlockSize, TextureFormat.R8, false);
            
            var pixels = texture_.GetPixels();
            for (var i = 0;i < pixels.Length; ++i)
                pixels[i] = Color.black;
            texture_.SetPixels(pixels);
            texture_.Apply();
        }

        void Update()
        {
            waveformArraySelector_.Selection = channel - 1;
            var array = waveformArraySelector_.SelectedArray;
            for (var i = 0; i < PdConstant.BlockSize; i++)
            {
                var y = (int)(256f * Mathf.Clamp(array[i], -1f, 1f)) + 256;
                texture_.SetPixel(i , cache_[i], Color.black);
                texture_.SetPixel(i , y, Color.white);
                cache_[i] = y;
            }

            texture_.Apply();
            if(renderTexture != null)
            {
                Graphics.Blit(texture_, renderTexture);
            }
        }

        public Texture2D Texture => texture_;
    }
}