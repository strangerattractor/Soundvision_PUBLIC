using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public interface ISpectrogram
    {
        Texture2D Texture { get; }
        int Index { get;  }
    }
    
    public class Spectrogram : MonoBehaviour, ISpectrogram
    {
        [SerializeField] protected PdBackend pdBackend;
        [SerializeField, Range(1, 16)] protected int channel = 1;

        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] int arrayLength_ = PdConstant.BlockSize; 
        [SerializeField] private Rect selection = Rect.zero;
        
        private ISpectrumGenerator spectrumGenerator_;
        private IPdArraySelector spectrumArraySelector_;
        private Texture2D texture_;
        private int index_;
        
        public int TextureWidth { get; } = 512;
        public int TextureHeight { get; } = 256;
        public Texture2D Spectrum => spectrumGenerator_.Spectrum;

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
            spectrumArraySelector_ = new PdArraySelector(pdBackend.SpectrumArrayContainer);
            spectrumGenerator_ = new SpectrumGeneratorPlayMode(TextureWidth, TextureHeight, spectrumArraySelector_);
            texture_ = new Texture2D(PdConstant.BlockSize, arrayLength_, TextureFormat.RFloat, false);
            
            var pixels = texture_.GetPixels();
            for (var i = 0;i < pixels.Length; ++i)
                pixels[i] = Color.black;
            texture_.SetPixels(pixels);
            texture_.Apply();
        }

        void Update()
        {
            spectrumArraySelector_.Selection = channel - 1;
            var energy = spectrumGenerator_.Update(selection);
            var array = spectrumArraySelector_.SelectedArray;
            for (var i = 0; i < PdConstant.BlockSize; i++)
            {
                texture_.SetPixel(i+1, index_, new Color(array[i], array[i], array[i]));
                //ToDo: Understand WHY i has to be +1
            }

            texture_.Apply();
            if (renderTexture != null)
            {
                Graphics.Blit(texture_, renderTexture);
            }

            index_++;
            index_ %= arrayLength_;
        }

        public Texture2D Texture => texture_;
        public int Index => index_;
    }


}
