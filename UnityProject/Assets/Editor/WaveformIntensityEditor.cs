using UnityEngine;
using UnityEditor;

namespace cylvester
{
    [CustomEditor(typeof(WaveformIntensity))]
    class WaveformIntensityEditor : UnityEditor.Editor
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
        private IWaveformIntensityGenerator spectrumGeneratorEditMode_;

        public void OnEnable()
        {
            var behaviour = (WaveformIntensity) target;

            pdBackendProperty_ = serializedObject.FindProperty("pdBackend");
            selectionProperty_ = serializedObject.FindProperty("selection");
            energyChangedProperty_ = serializedObject.FindProperty("energyChanged");
            channelProperty_ = serializedObject.FindProperty("channel");
            spectrumGeneratorEditMode_ = new WaveformIntensityGeneratorEditMode(behaviour.TextureWidth, behaviour.TextureHeight);
        }

        public override void OnInspectorGUI()
        {
            var behaviour = (WaveformIntensity) target;
            EditorGUILayout.PropertyField(pdBackendProperty_);

            GUILayout.Space(5);
            GUILayout.Label("Waveform", EditorStyles.boldLabel);
            paintSpace_ = GUILayoutUtility.GetRect(behaviour.TextureWidth, behaviour.TextureWidth,
                behaviour.TextureHeight, behaviour.TextureHeight);
            
            if (Event.current.type == EventType.Repaint)
            {
                // update selection
                if (Application.isPlaying)
                {
                    GUI.DrawTexture(paintSpace_, behaviour.Waveform);
                }
                else
                {
                    spectrumGeneratorEditMode_.Update();
                    GUI.DrawTexture(paintSpace_, spectrumGeneratorEditMode_.Waveform);
                }
            }

            Repaint();

            serializedObject.ApplyModifiedProperties();
        }
    }
}