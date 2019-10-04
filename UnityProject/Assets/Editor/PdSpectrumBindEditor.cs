using UnityEngine;
using UnityEditor;

namespace cylvester
{
    [CustomEditor(typeof(PdSpectrumBind))]
    class PdSpectrumBindEditor : Editor
    {
        private readonly string[] channels_ =
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"
        };

        private IRectangularSelection rectangularSelection_;

        private SerializedProperty selectionProperty_;
        private SerializedProperty pdBackendProperty_;
        private SerializedProperty energyChangedProperty_;
        private SerializedProperty channelProperty_;
        private Rect paintSpace_;
        private ISpectrumGenerator spectrumGeneratorEditMode_;

        public void OnEnable()
        {
            var behaviour = (PdSpectrumBind) target;

            pdBackendProperty_ = serializedObject.FindProperty("pdBackend");
            selectionProperty_ = serializedObject.FindProperty("selection");
            energyChangedProperty_ = serializedObject.FindProperty("energyChanged");
            channelProperty_ = serializedObject.FindProperty("channel");
            rectangularSelection_ = new RectangularSelection(behaviour.TextureWidth, behaviour.TextureHeight);
            spectrumGeneratorEditMode_ = new SpectrumGeneratorEditMode(behaviour.TextureWidth, behaviour.TextureHeight);
        }

        public override void OnInspectorGUI()
        {
            var behaviour = (PdSpectrumBind) target;
            EditorGUILayout.PropertyField(pdBackendProperty_);

            GUILayout.Label("PureData Inputs", EditorStyles.boldLabel);
            channelProperty_.intValue = EditorGUILayout.Popup("Input Channel", channelProperty_.intValue, channels_);

            GUILayout.Label("Callback", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(energyChangedProperty_);

            GUILayout.Space(5);
            GUILayout.Label("Spectrum Extractor", EditorStyles.boldLabel);
            paintSpace_ = GUILayoutUtility.GetRect(behaviour.TextureWidth, behaviour.TextureWidth,
                behaviour.TextureHeight, behaviour.TextureHeight);
            
            UpdateSelection();

            if (Event.current.type == EventType.Repaint)
            {
                // update selection
                if (Application.isPlaying)
                {
                    GUI.DrawTexture(paintSpace_, behaviour.Spectrum);
                }
                else
                {
                    spectrumGeneratorEditMode_.Update(selectionProperty_.rectValue);
                    GUI.DrawTexture(paintSpace_, spectrumGeneratorEditMode_.Spectrum);
                }
            }

            Repaint();

            RenderExtractedEnergy(behaviour.Energy);

            serializedObject.ApplyModifiedProperties();
        }


        private void UpdateSelection()
        {
            if (!Event.current.isMouse || Event.current.button != 0) return;
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                {
                    rectangularSelection_.Start(Event.current.mousePosition);
                    break;
                }

                case EventType.MouseDrag:
                {
                    selectionProperty_.rectValue =
                        rectangularSelection_.Update(Event.current.mousePosition, ref paintSpace_);
                    break;
                }
            }
        }

        private void RenderExtractedEnergy(int energy)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Extracted Energy", EditorStyles.boldLabel);
            GUILayout.Label(energy.ToString());
            GUILayout.EndHorizontal();
        }
    }
}