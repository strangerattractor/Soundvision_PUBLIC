using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public class TextureMapper : MonoBehaviour
    {
        [SerializeField] private Waveform waveform;
        [SerializeField] private Spectrogram spectrogram;

        [SerializeField] private GameObject waveformPanel;
        [SerializeField] private GameObject spectrogramPanel;
        
        private Renderer waveFormRenderer_;
        private Renderer spectroGramRenderer_;
        private static readonly int baseColorMap_ = Shader.PropertyToID("_BaseColorMap");
        
        void Start()
        {
            waveFormRenderer_ = waveformPanel.GetComponent<Renderer>();
            spectroGramRenderer_ = spectrogramPanel.GetComponent<Renderer>();
        }

        void Update()
        {
            waveFormRenderer_.material.SetTexture(baseColorMap_, waveform.Texture);
            spectroGramRenderer_.material.SetTexture(baseColorMap_, spectrogram.Texture);
        }
    }
}