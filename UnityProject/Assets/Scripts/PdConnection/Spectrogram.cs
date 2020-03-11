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
        [SerializeField] private PdBackend pdBackend;
        [SerializeField, Range(1, 16)] private int channel = 1;
        [SerializeField] int arrayLength_ = PdConstant.BlockSize; 
        
        private IPdArraySelector spectrumArraySelector_;
        private Texture2D texture_;
        private int index_;
        
        void Start()
        {
            spectrumArraySelector_ = new PdArraySelector(pdBackend.SpectrumArrayContainer);
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
            var array = spectrumArraySelector_.SelectedArray;
            for (var i = 0; i < PdConstant.BlockSize; i++)
            {
                texture_.SetPixel(i, index_, new Color(array[i], 0f, 0f));
            }

            texture_.Apply();
            
            index_++;
            index_ %= arrayLength_;
        }

        public Texture2D Texture => texture_;
        public int Index => index_;
    }


}
