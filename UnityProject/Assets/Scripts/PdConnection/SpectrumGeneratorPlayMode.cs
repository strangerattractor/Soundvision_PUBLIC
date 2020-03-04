using UnityEngine;

namespace cylvester
{
    public class SpectrumGeneratorPlayMode : SpectrumGenerator, ISpectrumGenerator
    {
        private IPdArraySelector arraySelector_;

        private Color32[] resetColorArray;

        public SpectrumGeneratorPlayMode(int textureWidth, int textureHeight, IPdArraySelector arraySelector)
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
            /* ## Old implementation
            var numPixels = 0;
            var data = arraySelector_.SelectedArray;
            
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
            */

            //New Implementation
            var numPixels = 0;
            var data = arraySelector_.SelectedArray;
            Spectrum.SetPixels32(resetColorArray); // Reset all pixels color
            var rectcolor = Color.red;
            //Draw selection Rectangle border
            for (int i=(int)selectionRect.x;i< (selectionRect.x + (selectionRect.width - 1)); i++) //horizontal lines
            {
                Spectrum.SetPixel(i, (int) (Spectrum.height - selectionRect.y - (selectionRect.height - 1)), rectcolor); //end line
                Spectrum.SetPixel(i, (int)(Spectrum.height - selectionRect.y), rectcolor); //start line
            }
            for (int i = (int)(Spectrum.height - selectionRect.y - (selectionRect.height - 1)); i < (int)(Spectrum.height - selectionRect.y); i++) //vertical lines
            {
                Spectrum.SetPixel((int)selectionRect.x, i, rectcolor); // line left
                Spectrum.SetPixel((int)(selectionRect.x + (selectionRect.width - 1)), i, rectcolor); // line right
            }

            //Draw Spectrum and calculate numPixels
            var spectrumcolor = Color.green;
            for (int x = 0; x < Spectrum.width; x++) //iterate over sprectrum length
            {
                var magnitude = data[x] * 20f; //TODO: implement logarithmic scale for y values
                
                for (int y=0; y<magnitude; y++) //all pixels below spectrum value at x position
                {
                    Spectrum.SetPixel(x, y, spectrumcolor);

                    if (IsInSelection(x, y, ref selectionRect)) //current spectrum pixel is inside rect
                    {
                        numPixels++;
                    }
                }
            }

            Spectrum.Apply();
            return numPixels;
        }
    }
}