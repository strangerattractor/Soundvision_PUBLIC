using UnityEngine;

namespace cylvester
{
    public class FrequencyBandGeneratorPlayMode : FrequencyBandGenerator, IFrequencyBandGenerator
    {
        private IPdArraySelector arraySelector_;

        private Color32[] resetColorArray;

        public FrequencyBandGeneratorPlayMode(int textureWidth, int textureHeight, IPdArraySelector arraySelector)
            :base(textureWidth, textureHeight)
        {
            arraySelector_ = arraySelector;

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
            var numPixels = 0;
            var numPixelsOrg = 0; // for testing purpose
            var data = arraySelector_.SelectedArray;
            Spectrum.SetPixels32(resetColorArray); // Reset all pixels color
            var rectcolor = Color.red;
            //Draw selection Rectangle border
            for (int i=(int)selectionRect.x;i< (selectionRect.x + (selectionRect.width - 1)); i++) //horizontal lines
            {
                Spectrum.SetPixel(i, (int) (Spectrum.height - 1- selectionRect.y - (selectionRect.height - 1)), rectcolor); //end line
                Spectrum.SetPixel(i, (int)(Spectrum.height - 1 - selectionRect.y), rectcolor); //start line
            }
            for (int i = (int)(Spectrum.height - selectionRect.y - (selectionRect.height - 1)); i < (int)(Spectrum.height - selectionRect.y); i++) //vertical lines
            {
                Spectrum.SetPixel((int)selectionRect.x, i, rectcolor); // line left
                Spectrum.SetPixel((int)(selectionRect.x + (selectionRect.width - 1)), i, rectcolor); // line right
            }

            //Draw Spectrum and calculate numPixels
            var spectrumcolor = Color.white;
            for (int x = 0; x < Spectrum.width; x++) //iterate over sprectrum length
            {
                float magnitude;
                if (logScale) {
                    magnitude = Mathf.Max(Mathf.Log10(data[x] * gain) * gain, 0);
                }
                else {
                    magnitude = data[x] * gain;
                }
                
                if (drawFullSpectrum) {
                    for (int y=0; y<magnitude; y++) //all pixels below spectrum value at x position
                    {
                        Spectrum.SetPixel(x, y, spectrumcolor);

                        if (IsInSelection(x, y, ref selectionRect)) //current spectrum pixel is inside rect
                        {
                            numPixelsOrg++;
                        }
                    }
                }

                Spectrum.SetPixel(x, (int)magnitude, spectrumcolor);

                if (selectionRect.x < x && x < selectionRect.x + (selectionRect.width-1)) {
                    float yTop = Spectrum.height - selectionRect.y;
                    float yBottom = yTop - selectionRect.height;
                    float magClamped = Mathf.Clamp(magnitude, yBottom + 1, yTop - 1);

                    if (magClamped > yBottom + 1) {
                        numPixels += (int)(magClamped - yBottom - 1);
                    }
                }
            }

            Spectrum.Apply();
            // Debug.Log(numPixels - numPixelsOrg); // must be zero
            return numPixels;
        }
    }
}
