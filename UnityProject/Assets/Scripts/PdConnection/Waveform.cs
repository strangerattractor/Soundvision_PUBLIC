using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public interface IWaveform
    {
        Texture2D Texture { get; }
    }
    
    public class Waveform : MonoBehaviour, ISpectrogram
    {
        [SerializeField] private PdBackend pdBackend;
        [SerializeField, Range(1, 16)] private int channel = 1;
        
        private IPdArraySelector waveformArraySelector_;
        private Texture2D texture_;
        private int[] cache_;
        
        void Start()
        {
            cache_ = new int[PdConstant.BlockSize];
            waveformArraySelector_ = new PdArraySelector(pdBackend.WaveformArrayContainer);
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
        }

        public Texture2D Texture => texture_;
    }
}