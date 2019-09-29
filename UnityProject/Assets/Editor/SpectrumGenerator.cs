using UnityEngine;

namespace cylvester
{
    interface ISpectrumGenerator
    {
        Texture2D Spectrum { get; }
        int Update(float[] fftData, ref Rect selectionRect);
    }
    
    public class SpectrumGenerator : ISpectrumGenerator
    {
        private Texture2D texture_;
        public Texture2D Spectrum => texture_;
        
        public  SpectrumGenerator(int width, int height)
        {
            texture_ = new Texture2D(width, height);
        }
        
        public int Update(float[] fftData, ref Rect selectionRect)
        {
            var numPixels = 0;
            for (var x = 0; x < texture_.width; x++)
            {
                var magnitude = fftData[x] * 20f;
                for (var y = 0; y < texture_.height; y++)
                {
                    var mY = texture_.height - selectionRect.y;

                    var fillPixel = magnitude > y;
                    var color =  fillPixel ? Color.green : Color.black;
                    if ((selectionRect.x < x && x < (selectionRect.x + selectionRect.width)) &&
                        (mY - selectionRect.height < y && y < mY))
                    {
                        color.a = 1f;
                        if (fillPixel)
                            numPixels++;
                    }
                    else
                    {
                        color.a = 0.2f;
                    }
                    texture_.SetPixel(x, y, color);
                }
            }
            texture_.Apply();
            return numPixels;
        }
    }
}