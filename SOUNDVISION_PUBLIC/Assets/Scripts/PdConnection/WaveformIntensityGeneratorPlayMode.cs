using UnityEngine;

namespace cylvester
{
    public class WaveformIntensityGeneratorPlayMode : WaveformIntensityGenerator, IWaveformIntensityGenerator
    {
        private IPdArraySelector arraySelector_;

        private Color32[] resetColorArray;

        public WaveformIntensityGeneratorPlayMode(int textureWidth, int textureHeight, IPdArraySelector arraySelector)
            :base(textureWidth, textureHeight)
        {
            arraySelector_ = arraySelector;

            //generate empty texture
            Color32 resetColor = new Color32(0, 0, 0, 255); //black
            resetColorArray = Waveform.GetPixels32();
            for (int i = 0; i < resetColorArray.Length; i++)
            {
                resetColorArray[i] = resetColor;
            }
        }

        public int Update(float gain)
        {
            
            var data = arraySelector_.SelectedArray;
            Waveform.SetPixels32(resetColorArray); // Reset all pixels color
            //Draw Waveform
            for (int x = 0; x < Waveform.width; x++) //iterate over sprectrum length
            {
                for (int y=0; y<Waveform.height; y++)
                {
                    Waveform.SetPixel(x, y, new Color(data[x] * gain, 0, 0));
                }
            }

            Waveform.Apply();
            return 0;
        }
    }
}