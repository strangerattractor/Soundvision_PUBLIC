using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public interface IWaveformIntensity
    {
        Texture2D Texture { get; }
    }

    public class WaveformIntensity : MonoBehaviour, IWaveformIntensity
    {
        [SerializeField] protected PdBackend pdBackend;
        [SerializeField, Range(1, 16)] protected int channel = 1;

        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] int arrayLength_ = PdConstant.BlockSize; 

        private IWaveformIntensityGenerator waveformGenerator_;
        private IPdArraySelector waveformArraySelector_;
        private Texture2D texture_;
        private int index_;
        
        public int TextureWidth { get; } = 512;
        public int TextureHeight { get; } = 8;
        public Texture2D Waveform => waveformGenerator_.Waveform;

        void Start()
        {
            if (pdBackend == null)
            {
                var pdBackendObjects = FindObjectsOfType<PdBackend>();
                if (pdBackendObjects.Length > 0)
                {
                    var g = pdBackendObjects[0].gameObject;
                    pdBackend = g.GetComponent<PdBackend>();
                }
            }
            waveformArraySelector_ = new PdArraySelector(pdBackend.WaveformArrayContainer);
            waveformGenerator_ = new WaveformIntensityGeneratorPlayMode(TextureWidth, TextureHeight, waveformArraySelector_);
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
            waveformGenerator_.Update();
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