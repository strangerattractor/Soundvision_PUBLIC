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

        private SerializedProperty pdBackendProperty_;
        private SerializedProperty channelProperty_;
        private SerializedProperty renderTextureProperty_;
        private SerializedProperty showDebugViewProperty_;
        private Rect paintSpace_;
        private ISpectrogramGenerator spectrumGeneratorEditMode_;

        public void OnEnable()
        {
            var behaviour = (Spectrogram) target;

            pdBackendProperty_ = serializedObject.FindProperty("pdBackend");
            channelProperty_ = serializedObject.FindProperty("channel");
            renderTextureProperty_ = serializedObject.FindProperty("renderTexture");
            showDebugViewProperty_ = serializedObject.FindProperty("showDebugView");
            spectrumGeneratorEditMode_ = new SpectrogramGeneratorEditMode(behaviour.TextureWidth, behaviour.TextureHeight);
        }

        public override void OnInspectorGUI()
        {
            var behaviour = (Spectrogram) target;
            EditorGUILayout.PropertyField(pdBackendProperty_);

            GUILayout.Label("PureData Inputs", EditorStyles.boldLabel);
            channelProperty_.intValue = EditorGUILayout.Popup("Input Channel", channelProperty_.intValue, channels_);

            GUILayout.Space(5);
            GUILayout.Label("Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(renderTextureProperty_);

            GUILayout.Space(5);
            GUILayout.Label("Spectrum", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showDebugViewProperty_);
            paintSpace_ = GUILayoutUtility.GetRect(behaviour.TextureWidth, behaviour.TextureWidth,
                behaviour.TextureHeight, behaviour.TextureHeight);
            
            if (Event.current.type == EventType.Repaint && showDebugViewProperty_.boolValue)
            {
                // update selection
                if (Application.isPlaying)
                {
                    GUI.DrawTexture(paintSpace_, behaviour.Spectrum);
                }
                else
                {
                    spectrumGeneratorEditMode_.Update();
                    GUI.DrawTexture(paintSpace_, spectrumGeneratorEditMode_.Spectrum);
                }
            }

            Repaint();

            serializedObject.ApplyModifiedProperties();
        }
    }
}