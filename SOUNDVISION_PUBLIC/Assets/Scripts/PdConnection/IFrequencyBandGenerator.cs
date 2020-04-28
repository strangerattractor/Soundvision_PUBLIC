using System;
using UnityEngine;

namespace cylvester

{
    public interface IFrequencyBandGenerator
    {
        Texture2D Spectrum { get; }
        int Update(Rect selectionRect, bool drawFullSpectrum, float gain, bool logScale);
    }
    
    public abstract class FrequencyBandGenerator
    {
        public Texture2D Spectrum { get; }

        protected FrequencyBandGenerator(int textureWidth, int textureHeight)
        {
            Spectrum = new Texture2D(textureWidth, textureHeight);
        }
        
        protected void OnAllPixels(Action<int, int> action)
        {
            for (var x = 0; x < Spectrum.width; x++)
            for (var y = 0; y < Spectrum.height; y++)
                action(x, y);
        }

        protected bool IsInSelection(int x, int y, ref Rect selectionRect)
        {
            var inRectHorizontally = selectionRect.x < x && x < selectionRect.x + (selectionRect.width-1);
            var mY = Spectrum.height - selectionRect.y;
            var inRectVertically = mY - (selectionRect.height-1) < y && y < mY;
            return inRectHorizontally && inRectVertically;
        }
    }
}
