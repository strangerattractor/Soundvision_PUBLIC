using UnityEngine;

namespace cylvester
{
    public class FrequencyBandGeneratorEditMode : FrequencyBandGenerator, IFrequencyBandGenerator
    {

        private Color32[] resetColorArray;

        public FrequencyBandGeneratorEditMode(int textureWidth, int textureHeight) 
            : base(textureWidth,textureHeight)
        {
            //generate empty texture
            Color32 resetColor = new Color32(0, 0, 0, 255); //black with alpha
            resetColorArray = Spectrum.GetPixels32();
            for (int i = 0; i < resetColorArray.Length; i++)
            {
                resetColorArray[i] = resetColor;
            }
        }

        public int Update(Rect selectionRect, bool drawFullSpectrum, float gain, bool logScale)
        {
            Spectrum.SetPixels32(resetColorArray); // Reset all pixels color
            var rectcolor = Color.red;
            //Draw selection Rectangle border
            for (int i = (int)selectionRect.x; i < (selectionRect.x + (selectionRect.width - 1)); i++) //horizontal lines
            {
                Spectrum.SetPixel(i, (int)(Spectrum.height - 1 - selectionRect.y - (selectionRect.height - 1)), rectcolor); //end line
                Spectrum.SetPixel(i, (int)(Spectrum.height - 1 - selectionRect.y), rectcolor); //start line
            }
            for (int i = (int)(Spectrum.height - selectionRect.y - (selectionRect.height - 1)); i < (int)(Spectrum.height - selectionRect.y); i++) //vertical lines
            {
                Spectrum.SetPixel((int)selectionRect.x, i, rectcolor); // line left
                Spectrum.SetPixel((int)(selectionRect.x + (selectionRect.width - 1)), i, rectcolor); // line right
            }

            Spectrum.Apply();

            return 0;
        }
    }

}
