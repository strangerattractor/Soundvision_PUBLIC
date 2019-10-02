using System;
using UnityEngine;

namespace cylvester
{
    public interface ISpectrumGenerator
    {
        Texture2D Spectrum { get; }
        int Update(Rect selectionRect);
    }
    
    public abstract class SpectrumGenerator
    {
        public Texture2D Spectrum { get; }

        protected SpectrumGenerator(int textureWidth, int textureHeight)
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