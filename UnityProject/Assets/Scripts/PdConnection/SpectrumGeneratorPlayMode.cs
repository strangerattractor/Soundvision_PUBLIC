using UnityEngine;

namespace cylvester
{
    public class SpectrumGeneratorPlayMode : SpectrumGenerator, ISpectrumGenerator
    {
        private IPdArray pdArray_;
        
        public SpectrumGeneratorPlayMode(int textureWidth, int textureHeight, IPdArray pdArray)
            :base(textureWidth, textureHeight)
        {
            pdArray_ = pdArray;
        }

        public int Update( Rect selectionRect)
        {
            var numPixels = 0;
            var data = pdArray_.Data;
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
    }
}