using UnityEngine;

namespace cylvester
{
    public class FrequencyBandGeneratorPlayMode : FrequencyBandGenerator, IFrequencyBandGenerator
    {
        private IPdArraySelector arraySelector_;

        private Color32[] resetColorArray;
        private float[] bins;

        public FrequencyBandGeneratorPlayMode(int textureWidth, int textureHeight, IPdArraySelector arraySelector)
            :base(textureWidth, textureHeight)
        {
            arraySelector_ = arraySelector;

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
            bins = b;
        }
        public int Update(int rectx, int recty, int rectw, int recth)
        {
            var numPixels = 0;
            var data = arraySelector_.SelectedArray;
            Spectrum.SetPixels32(resetColorArray); // Reset all pixels color
            var rectcolor = Color.red;
            //Draw selection Rectangle border
            for (int i=(int)rectx;i< (rectx + (rectw- 1)); i++) //horizontal lines
            {
                Spectrum.SetPixel(i, (int) (Spectrum.height - recty - (recth - 1)), rectcolor); //end line
                Spectrum.SetPixel(i, (int)(Spectrum.height - recty), rectcolor); //start line
            }
            for (int i = (int)(Spectrum.height - recty - (recth - 1)); i < (int)(Spectrum.height - recty); i++) //vertical lines
            {
                Spectrum.SetPixel((int)rectx, i, rectcolor); // line left
                Spectrum.SetPixel((int)(rectx + (rectw - 1)), i, rectcolor); // line right
            }

            //Draw Spectrum and calculate numPixels
            var spectrumcolor = Color.green;
            for (int x = 0; x < Spectrum.width; x++) //iterate over sprectrum length
            {
                var magnitude = bins[x * bins.Length / Spectrum.width] * Spectrum.height; //TODO: implement logarithmic scale for y values
                
                for (int y=0; y<magnitude; y++) //all pixels below spectrum value at x position
                {
                    Spectrum.SetPixel(x, y, spectrumcolor);
                }
            }

            Spectrum.Apply();
            return 0;
        }
    }
}