using UnityEngine;

namespace cylvester
{
    public class SpectrumGeneratorPlayMode : SpectrumGenerator, ISpectrumGenerator
    {
        private IPdArraySelector arraySelector_;
        
        public SpectrumGeneratorPlayMode(int textureWidth, int textureHeight, IPdArraySelector arraySelector)
            :base(textureWidth, textureHeight)
        {
            arraySelector_ = arraySelector;
        }

        public int Update(Rect selectionRect)
        {
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

            // statt über alle Pixel, nur über SpektrumArray und dann die Auswahl prüfen
            /*for (int x = (int) selectionRect.x; x < selectionRect.x+selectionRect.width-1; x++)
            {
                for (int y = (int) selectionRect.y; y < Spectrum.height - selectionRect.y - (selectionRect.height - 1); y++)
                {
                    var magnitude = data[x] * 20f;
                    var validPixel = magnitude > y;
                    var color = validPixel ? Color.green : Color.black;

                    //if (IsInSelection(x, y, ref selectionRect))
                    
                        color.a = 1f;
                        if (validPixel)
                            numPixels++;
                    
                    Spectrum.SetPixel(x, y, color);
                }
            }
            */

                Spectrum.Apply();
            return numPixels;
        }
    }
}