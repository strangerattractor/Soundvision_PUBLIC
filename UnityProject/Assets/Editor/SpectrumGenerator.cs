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
        private Texture2D texture_;
        public Texture2D Spectrum => texture_;
        
        public  SpectrumGenerator(int width, int height)
        {
            texture_ = new Texture2D(width, height);
        }
        
        public int Update(IPdArray pdArray, Rect selectionRect)
        {
            var numPixels = 0;
            for (var x = 0; x < texture_.width; x++)
            {
                for (var y = 0; y < texture_.height; y++)
                {
                    var color = Color.black;
                    var validPixel = false;
                    
                    if (pdArray != null)
                    {
                        var magnitude = pdArray.Data[x] * 20f;
                        validPixel = magnitude > y;
                        color = validPixel ? Color.green : Color.black;
                    }

                    var mY = texture_.height - selectionRect.y;
                    if ((selectionRect.x < x && x < (selectionRect.x + selectionRect.width)) &&
                        (mY - selectionRect.height < y && y < mY))
                    {
                        color.a = 1f;
                        if (validPixel)
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