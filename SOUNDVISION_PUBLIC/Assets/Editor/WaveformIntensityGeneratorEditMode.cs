using UnityEngine;

namespace cylvester
{
    public class WaveformIntensityGeneratorEditMode : WaveformIntensityGenerator, IWaveformIntensityGenerator
    {

        private Color32[] resetColorArray;

        public WaveformIntensityGeneratorEditMode(int textureWidth, int textureHeight) 
            : base(textureWidth,textureHeight)
        {
            //generate empty texture
            Color32 resetColor = new Color32(0, 0, 0, 64); //black with alpha
            resetColorArray = Waveform.GetPixels32();
            // for (int i = 0; i < resetColorArray.Length; i++)
            // {
            //     resetColorArray[i] = resetColor;
            // }
        }
  
        public int Update(float gain)
        {
            return 0;
        }
    }

}