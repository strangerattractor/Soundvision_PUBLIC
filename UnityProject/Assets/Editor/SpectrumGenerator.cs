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
            var numPixels = 0;
            for (var x = 0; x < Spectrum.width; x++)
            {
                for (var y = 0; y < Spectrum.height; y++)
                {
                    var color = Color.black;
                    var validPixel = false;
                    
                    if (pdArray != null)
                    {
                        var magnitude = pdArray.Data[x] * 20f;
                        validPixel = magnitude > y;
                        color = validPixel ? Color.green : Color.black;
                    }

                    if (IsInSelection(x, y, ref selectionRect))
                    {
                        color.a = 1f;
                        if (validPixel)
                            numPixels++;
                    }
                    else
                        color.a = 0.2f;
                    
                    Spectrum.SetPixel(x, y, color);
                }
            }
            Spectrum.Apply();
            return numPixels;
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