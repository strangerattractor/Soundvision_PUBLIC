using System;
using UnityEngine;

namespace cylvester

{
    public interface ISpectrogramGenerator
    {
        Texture2D Spectrum { get; }
        int Update();
    }
    
    public abstract class SpectrogramGenerator
    {
        public Texture2D Spectrum { get; }

        protected SpectrogramGenerator(int textureWidth, int textureHeight)
        {
            Spectrum = new Texture2D(textureWidth, textureHeight);
        }
    }
}