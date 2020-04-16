using System;
using UnityEngine;

namespace cylvester

{
    public interface IWaveformIntensityGenerator
    {
        Texture2D Waveform { get; }
        int Update(float gain);
    }
    
    public abstract class WaveformIntensityGenerator
    {
        public Texture2D Waveform { get; }

        protected WaveformIntensityGenerator(int textureWidth, int textureHeight)
        {
            Waveform = new Texture2D(textureWidth, textureHeight);
        }
    }
}