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

        private SerializedProperty pdBackendProperty_;
        private SerializedProperty channelProperty_;
        private SerializedProperty renderTextureProperty_;
        private SerializedProperty showDebugViewProperty_;
        private SerializedProperty debugViewGainProperty_;
        private Rect paintSpace_;
        private IWaveformIntensityGenerator spectrumGeneratorEditMode_;

        public void OnEnable()
        {
            var behaviour = (WaveformIntensity) target;

            pdBackendProperty_ = serializedObject.FindProperty("pdBackend");
            channelProperty_ = serializedObject.FindProperty("channel");
            renderTextureProperty_ = serializedObject.FindProperty("renderTexture");
            showDebugViewProperty_ = serializedObject.FindProperty("showDebugView");
            debugViewGainProperty_ = serializedObject.FindProperty("debugViewGain");
            spectrumGeneratorEditMode_ = new WaveformIntensityGeneratorEditMode(behaviour.TextureWidth, behaviour.TextureHeight);
        }

        public override void OnInspectorGUI()
        {
            var behaviour = (WaveformIntensity) target;
            EditorGUILayout.PropertyField(pdBackendProperty_);

            GUILayout.Label("PureData Inputs", EditorStyles.boldLabel);
            channelProperty_.intValue = EditorGUILayout.Popup("Input Channel", channelProperty_.intValue, channels_);

            GUILayout.Space(5);
            GUILayout.Label("Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(renderTextureProperty_);

            GUILayout.Space(5);
            GUILayout.Label("Waveform", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showDebugViewProperty_);
            EditorGUILayout.PropertyField(debugViewGainProperty_);
            paintSpace_ = GUILayoutUtility.GetRect(behaviour.TextureWidth, behaviour.TextureWidth,
                behaviour.TextureHeight, behaviour.TextureHeight);
            
            if (Event.current.type == EventType.Repaint && showDebugViewProperty_.boolValue)
            {
                // update selection
                if (Application.isPlaying)
                {
                    GUI.DrawTexture(paintSpace_, behaviour.Waveform);
                }
                else
                {
                    spectrumGeneratorEditMode_.Update(debugViewGainProperty_.floatValue);
                    GUI.DrawTexture(paintSpace_, spectrumGeneratorEditMode_.Waveform);
                }
            }

            Repaint();

            serializedObject.ApplyModifiedProperties();
        }
    }
}