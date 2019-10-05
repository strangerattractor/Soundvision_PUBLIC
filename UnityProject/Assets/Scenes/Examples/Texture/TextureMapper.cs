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
        [SerializeField] private GameObject chikashiPanel;

        
        private Renderer waveFormRenderer_;
        private Renderer spectroGramRenderer_;
        private Renderer chikashiRenderer_;
        private static readonly int baseColorMap_ = Shader.PropertyToID("_BaseColorMap");
        private static readonly int myTexture_ = Shader.PropertyToID("_MyTexture");
        
        void Start()
        {
            waveFormRenderer_ = waveformPanel.GetComponent<Renderer>();
            spectroGramRenderer_ = spectrogramPanel.GetComponent<Renderer>();
            chikashiRenderer_ = chikashiPanel.GetComponent<Renderer>();
        }

        void Update()
        {
            waveFormRenderer_.material.SetTexture(baseColorMap_, waveform.Texture);
            spectroGramRenderer_.material.SetTexture(baseColorMap_, spectrogram.Texture);
            chikashiRenderer_.material.SetTexture(myTexture_, waveform.Texture);
        }
    }
}