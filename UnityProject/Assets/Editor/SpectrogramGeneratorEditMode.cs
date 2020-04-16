using UnityEngine;

namespace cylvester
{
    public class SpectrogramGeneratorEditMode : SpectrogramGenerator, ISpectrogramGenerator
    {

        private Color32[] resetColorArray;

        public SpectrogramGeneratorEditMode(int textureWidth, int textureHeight) 
            : base(textureWidth,textureHeight)
        {
            //generate empty texture
            Color32 resetColor = new Color32(0, 0, 0, 64); //black with alpha
            resetColorArray = Spectrum.GetPixels32();
            // for (int i = 0; i < resetColorArray.Length; i++)
            // {
            //     resetColorArray[i] = resetColor;
            // }
        }
  
        public int Update()
        {
            return 0;
        }
    }

}