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
            Color32 resetColor = new Color32(0, 0, 0, 64); //black with alpha
            resetColorArray = Spectrum.GetPixels32();
            for (int i = 0; i < resetColorArray.Length; i++)
            {
                resetColorArray[i] = resetColor;
            }
        }

        public void SetBins(float[] b)
        {
        }

        public int Update(int rectx, int recty, int rectw, int recth)
        {
            Spectrum.SetPixels32(resetColorArray); // Reset all pixels color
            var rectcolor = Color.red;
            //Draw selection Rectangle border
            for (int i = rectx; i < (rectx + (rectw - 1)); i++) //horizontal lines
            {
                Spectrum.SetPixel(i, (int)(Spectrum.height - recty - (recth - 1)), rectcolor); //end line
                Spectrum.SetPixel(i, (int)(Spectrum.height - recty), rectcolor); //start line
            }
            for (int i = (Spectrum.height - recty - (recth - 1)); i < (Spectrum.height - recty); i++) //vertical lines
            {
                Spectrum.SetPixel(rectx, i, rectcolor); // line left
                Spectrum.SetPixel((rectx + (rectw - 1)), i, rectcolor); // line right
            }

            Spectrum.Apply();

            return 0;
        }
    }

}