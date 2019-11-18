using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public class SpectrogramMapper : MonoBehaviour
    {
        [SerializeField] private Spectrogram spectrogram;

        [SerializeField] private GameObject spectrogramPanel;

        
        private Renderer spectroGramRenderer_;

        private static readonly int baseColorMap_ = Shader.PropertyToID("_BaseColorMap");
        private static readonly int spectrogramIndex_ = Shader.PropertyToID("_Index");

        
        void Start()
        {
            spectroGramRenderer_ = spectrogramPanel.GetComponent<Renderer>();
        }

        void Update()
        {
            spectroGramRenderer_.material.SetTexture(baseColorMap_, spectrogram.Texture);
            spectroGramRenderer_.material.SetInt(spectrogramIndex_, spectrogram.Index);
        }
    }
}