using UnityEngine;

namespace cylvester
{
    interface ISpectrumGenerator
    {
        Texture2D Spectrum { get; }
        void Update(float[] fftData, ref Rect selectionRect);
    }
    
    public class SpectrumGenerator : ISpectrumGenerator
    {
        private Texture2D texture_;
        private readonly int height_;
        public Texture2D Spectrum => texture_;
        
        public  SpectrumGenerator(int width, int height)
        {
            texture_ = new Texture2D(width, height);
            height_ = height;
        }
        
        public void Update(float[] fftData, ref Rect selectionRect)
        {
            for (var x = 0; x < texture_.width; x++)
            {
                var magnitude = fftData[x];
                for (var y = 0; y < texture_.height; y++)
                {

                    var alpha = 0.4f;
                    if ((selectionRect.x < x && x < (selectionRect.x + selectionRect.width)) &&
                        (selectionRect.y < y && y < (selectionRect.y + selectionRect.height)))
                    {
                        alpha = 1f;
                    }

                    var color = y > magnitude ? Color.white : Color.gray;
                    color.a = alpha;
                    texture_.SetPixel(x, height_, color);
                }
            }
            texture_.Apply();
        }
    }
}