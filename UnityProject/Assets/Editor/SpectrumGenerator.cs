using System;
using UnityEngine;

namespace cylvester
{
    interface ISpectrumGenerator
    {
        Texture2D Spectrum { get; }
        int Update(IPdArray pdArray, Rect selectionRect);
    }
    
    public class SpectrumGenerator : ISpectrumGenerator
    {
        public Texture2D Spectrum { get; }

        public  SpectrumGenerator(int width, int height)
        {
            Spectrum = new Texture2D(width, height);
        }
        
        public int Update(IPdArray pdArray, Rect selectionRect)
        {
            if (pdArray != null)
            {
                return UpdatePlayMode(pdArray, selectionRect);
            }
            UpdateEditMode(selectionRect);
            return 0;
        }

        private int UpdatePlayMode(IPdArray pdArray, Rect selectionRect)
        {
            var numPixels = 0;
            var data = pdArray.Data;
            OnAllPixels((x, y) =>
            {
                var magnitude = data[x] * 20f;
                var validPixel = magnitude > y;
                var color = validPixel ? Color.green : Color.black;

                if (IsInSelection(x, y, ref selectionRect))
                {
                    color.a = 1f;
                    if (validPixel)
                        numPixels++;
                }
                else
                    color.a = 0.2f;
                    
                Spectrum.SetPixel(x, y, color);
            });
            Spectrum.Apply();
            return numPixels;
        }

        private void UpdateEditMode(Rect selectionRect)
        {
            OnAllPixels((x, y) =>
            {
                var color = Color.black;
                if (IsInSelection(x, y, ref selectionRect))
                    color.a = 1f;
                else
                    color.a = 0.2f;
                Spectrum.SetPixel(x, y, color);
            });
            Spectrum.Apply();
        }

        private void OnAllPixels(Action<int, int> action)
        {
            for (var x = 0; x < Spectrum.width; x++)
                for (var y = 0; y < Spectrum.height; y++)
                    action(x, y);
        }

        private bool IsInSelection(int x, int y, ref Rect selectionRect)
        {
            var inRectHorizontally = selectionRect.x < x && x < selectionRect.x + (selectionRect.width-1);
            var mY = Spectrum.height - selectionRect.y;
            var inRectVertically = mY - (selectionRect.height-1) < y && y < mY;
            return inRectHorizontally && inRectVertically;
        }
    }
}