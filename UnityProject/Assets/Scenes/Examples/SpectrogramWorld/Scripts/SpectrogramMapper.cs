using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public class SpectrogramMapper : MonoBehaviour
    {
        [SerializeField] private Spectrogram spectrogram;

        [SerializeField] private GameObject spectrogramPanel;

        
        private Renderer spectrogramRenderer_;

        private static readonly int baseColorMap_ = Shader.PropertyToID("_BaseColorMap");
        private static readonly int spectrogramIndex_ = Shader.PropertyToID("_Index");
        
        void Start()
        {
            spectrogramRenderer_ = spectrogramPanel.GetComponent<Renderer>();
        }

        void Update()
        {
            spectrogramRenderer_.material.SetTexture(baseColorMap_, spectrogram.Texture);
            spectrogramRenderer_.material.SetInt(spectrogramIndex_, spectrogram.Index);
        }
    }
}