using UnityEngine;
using UnityEditor;

namespace cylvester
{
    [CustomEditor(typeof(Spectrogram))]
    class SpectrogramEditor : UnityEditor.Editor
    {
        private readonly string[] channels_ =
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"
        };

        private SerializedProperty selectionProperty_;
        private SerializedProperty pdBackendProperty_;
        private SerializedProperty energyChangedProperty_;
        private SerializedProperty channelProperty_;
        private Rect paintSpace_;
        private ISpectrogramGenerator spectrumGeneratorEditMode_;

        public void OnEnable()
        {
            var behaviour = (Spectrogram) target;

            pdBackendProperty_ = serializedObject.FindProperty("pdBackend");
            selectionProperty_ = serializedObject.FindProperty("selection");
            energyChangedProperty_ = serializedObject.FindProperty("energyChanged");
            channelProperty_ = serializedObject.FindProperty("channel");
            spectrumGeneratorEditMode_ = new SpectrogramGeneratorEditMode(behaviour.TextureWidth, behaviour.TextureHeight);
        }

        public override void OnInspectorGUI()
        {
            var behaviour = (Spectrogram) target;
            EditorGUILayout.PropertyField(pdBackendProperty_);

            GUILayout.Space(5);
            GUILayout.Label("Spectrum", EditorStyles.boldLabel);
            paintSpace_ = GUILayoutUtility.GetRect(behaviour.TextureWidth, behaviour.TextureWidth,
                behaviour.TextureHeight, behaviour.TextureHeight);
            
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}