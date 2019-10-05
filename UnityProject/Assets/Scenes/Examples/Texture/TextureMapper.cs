using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public class TextureMapper : MonoBehaviour
    {
        [SerializeField] private Spectrogram spectrogram;
        private Renderer renderer_;
        private static readonly int baseColorMap_ = Shader.PropertyToID("_BaseColorMap");
        
        void Start()
        {
            renderer_ = GetComponent<Renderer>();
        }

        void Update()
        {
            renderer_.material.SetTexture(baseColorMap_, spectrogram.Texture);
        }
    }
}