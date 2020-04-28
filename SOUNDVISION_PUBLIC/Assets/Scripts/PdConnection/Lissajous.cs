using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public interface ILissajous
    {
        Texture2D Texture { get; }
    }

    public class Lissajous : PdBaseBindStereo, ILissajous
    {
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField, Range(1, 10)]  private int gain_ = 1;


        private IPdArraySelector waveformArraySelectorLeft_;
        private IPdArraySelector waveformArraySelectorRight_;
        private Texture2D texture_;
        private Vector2Int[] cache_;

        void Start()
        {
            base.Start();
            cache_ = new Vector2Int[PdConstant.BlockSize];
            waveformArraySelectorLeft_ = new PdArraySelector(pdbackend.WaveformArrayContainer);
            waveformArraySelectorRight_ = new PdArraySelector(pdbackend.WaveformArrayContainer);

            texture_ = new Texture2D(PdConstant.BlockSize, PdConstant.BlockSize, TextureFormat.R8, false);
            
            var pixels = texture_.GetPixels();
            for (var i = 0;i < pixels.Length; ++i)
                pixels[i] = Color.black;
            texture_.SetPixels(pixels);
            texture_.Apply();
        }

        void Update()
        {
            waveformArraySelectorLeft_.Selection = channelLeft - 1;
            waveformArraySelectorRight_.Selection = channelRight - 1;
            var arrayLeft = waveformArraySelectorLeft_.SelectedArray;
            var arrayRight = waveformArraySelectorRight_.SelectedArray;

            for (var i = 0; i < PdConstant.BlockSize; i++)
            {
                var x = (int)(256f * Mathf.Clamp(arrayLeft[i] * gain_, -1f, 1f)) + 256;
                var y = (int)(256f * Mathf.Clamp(arrayRight[i] * gain_, -1f, 1f)) + 256;
                texture_.SetPixel(cache_[i].x, cache_[i].y, Color.black);
                texture_.SetPixel(x , y, Color.white);
                cache_[i] = new Vector2Int(x, y);
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