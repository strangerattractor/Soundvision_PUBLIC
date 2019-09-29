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

        private ISpectrumGenerator spectrumGenerator_;
        private IRectangularSelection rectangularSelection_;
        private Rect paintSpace_;
        
        private SerializedProperty pdBackendProperty_;

        
        public void OnEnable()
        {
            spectrumGenerator_ = new SpectrumGenerator(TextureWidth, TextureHeight);
            rectangularSelection_ = new RectangularSelection(TextureWidth, TextureHeight);

            pdBackendProperty_ = serializedObject.FindProperty("pdBackend");
            
        }

        public override void OnInspectorGUI()
        {

            var behaviour = (IPdSpectrumBind)target;
            EditorGUILayout.PropertyField(pdBackendProperty_);
            serializedObject.ApplyModifiedProperties();

            GUILayout.Label("PureData Inputs", EditorStyles.boldLabel);
            behaviour.Channel = EditorGUILayout.Popup("Input Channel", behaviour.Channel, channels);

            if (Application.isPlaying && pdBackendProperty_ != null)
                RenderSpectrumExtractor(behaviour);
        }

        private void RenderSpectrumExtractor(IPdSpectrumBind behaviour)
        {
            UpdateSelection(behaviour);
            
            GUILayout.Space(5);
            GUILayout.Label("Spectrum Extractor", EditorStyles.boldLabel);
            
            var paintSpace = GUILayoutUtility.GetRect(TextureHeight, TextureWidth, TextureHeight, TextureHeight);
            if (Event.current.type == EventType.Repaint)
            {
                paintSpace_ = paintSpace;
                var spectrumArray = behaviour.GetPdArray(behaviour.Channel);
                
                behaviour.Energy = spectrumGenerator_.Update(spectrumArray.Data, ref behaviour.Selection);
                GUI.DrawTexture(paintSpace_, spectrumGenerator_.Spectrum);
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Extracted Energy", EditorStyles.boldLabel);
            GUILayout.Label(behaviour.Energy.ToString());
            GUILayout.EndHorizontal();

            Repaint();
        }

        private void UpdateSelection(IPdSpectrumBind behaviour) 
        {
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
                        rectangularSelection_.Update(Event.current.mousePosition, 
                            ref paintSpace_, ref behaviour.Selection);
                        break;
                    }
                }
            }
        }
    }
}
