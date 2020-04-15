using UnityEngine;

namespace cylvester
{
    public class SpectrogramGeneratorPlayMode : SpectrogramGenerator, ISpectrogramGenerator
    {
        private IPdArraySelector arraySelector_;

        private Color32[] resetColorArray;

        public SpectrogramGeneratorPlayMode(int textureWidth, int textureHeight, IPdArraySelector arraySelector)
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

        public int Update(Rect selectionRect)
        {
            
            var data = arraySelector_.SelectedArray;
            Spectrum.SetPixels32(resetColorArray); // Reset all pixels color
            //Draw Spectrum
            for (int x = 0; x < Spectrum.width; x++) //iterate over sprectrum length
            {
                var magnitude = data[x] * 20f; //TODO: implement logarithmic scale for y values
                
                for (int y=0; y<Spectrum.height; y++) //all pixels below spectrum value at x position
                {
                    Spectrum.SetPixel(x, y, new Color(0, 255, 0, data[x]));
                }
            }

            Spectrum.Apply();
            return 0;
        }
    }
}