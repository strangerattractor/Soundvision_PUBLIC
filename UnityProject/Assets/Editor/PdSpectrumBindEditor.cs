using UnityEngine;
using UnityEditor;

namespace cylvester
{
    [CustomEditor(typeof(PdSpectrumBind))]
    class PdSpectrumBindEditor : Editor
    {
        private const int TextureWidth = 512;
        private const int TextureHeight = 256;
        private readonly string[] channels = {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"
        };
        private int selectedSpectrum_;
        private ISpectrumGenerator spectrumGenerator_;
        private IRectangularSelection rectangularSelection_;
        private Rect paintSpace_;
        
        public void OnEnable()
        {
            spectrumGenerator_ = new SpectrumGenerator(TextureWidth, TextureHeight);
            rectangularSelection_ = new RectangularSelection(ref paintSpace_, TextureWidth, TextureHeight);
        }

        public override void OnInspectorGUI()
        {
            var behaviour = (PdSpectrumBind)target;
            

            if (Event.current.isMouse && Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                    {
                        rectangularSelection_.Start(Event.current.mousePosition);
                        break;
                    }
                    case EventType.MouseDrag:
                    {
                        rectangularSelection_.Update(Event.current.mousePosition);
                        break;
                    }
                }
            }

            GUILayout.Label("PureData Inputs", EditorStyles.boldLabel);

            selectedSpectrum_ = EditorGUILayout.Popup("Input Channel", selectedSpectrum_, channels);
            GUILayout.Space(5);
            GUILayout.Label("Spectrum Extractor", EditorStyles.boldLabel);
            
            var paintSpace = GUILayoutUtility.GetRect(TextureHeight, TextureWidth, TextureHeight, TextureHeight);
            if (Event.current.type == EventType.Repaint)
            {
                paintSpace_ = paintSpace;
                spectrumGenerator_.Update(ref rectangularSelection_.Selection);
                GUI.DrawTexture(paintSpace_, spectrumGenerator_.Spectrum);
            }

            Repaint();
        }
    }
}
