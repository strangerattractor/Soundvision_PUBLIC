using UnityEngine;

namespace cylvester
{
    public class SpectrumGeneratorEditMode : SpectrumGenerator, ISpectrumGenerator
    {
        public SpectrumGeneratorEditMode(int textureWidth, int textureHeight) 
            : base(textureWidth,textureHeight) { }
        
        public int Update(Rect selectionRect)
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
            return 0;
        }
    }

}