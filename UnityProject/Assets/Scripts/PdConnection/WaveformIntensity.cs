using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public interface IWaveformIntensity
    {
        Texture2D Texture { get; }
    }

    public class WaveformIntensity : PdBaseBindMono, IWaveformIntensity
    {
        [SerializeField] private RenderTexture renderTexture;

        private IPdArraySelector waveformArraySelector_;
        private Texture2D texture_;

        void Start()
        {
            base.Start();
            waveformArraySelector_ = new PdArraySelector(pdbackend.WaveformArrayContainer);
            texture_ = new Texture2D(PdConstant.BlockSize, 1, TextureFormat.RFloat, false);

            var pixels = texture_.GetPixels();
            for (var i = 0; i < pixels.Length; ++i)
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
                texture_.SetPixel(i, 0, new Color(array[i], array[i], array[i]));
            }

            texture_.Apply();
            if (renderTexture != null)
            {
                Graphics.Blit(texture_, renderTexture);
            }
        }

        public Texture2D Texture => texture_;
    }
}