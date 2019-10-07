using UnityEditor;
using UnityEngine;

namespace cylvester
{
    [CustomEditor(typeof(PdBackend))]
    public class PdBackendEditor : Editor
    {
        private PdBackend pdBackend_;
        private SerializedProperty controlMessageReceivedProperty_;

        private readonly string[] samples_ =
        {
            "No Playback",
            "Back_Back",
            "Brutal_Synth",
            "Dialog",
            "Drums",
            "Fox_Melo",
            "Kick",
            "Pads+Strings",
            "Rose_Sax",
            "Roses_Front"
        };
        
        private void Awake()
        {
            pdBackend_ = (PdBackend) target;
        }

        public override void OnInspectorGUI ()
        {
            
            serializedObject.Update();
            pdBackend_ = (PdBackend) target;
            controlMessageReceivedProperty_ = serializedObject.FindProperty("controlMessageReceived");
            EditorGUILayout.PropertyField(controlMessageReceivedProperty_);
            
            if (Application.isPlaying)
            {
                RenderSamplePlayback();
                Repaint();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void RenderSamplePlayback()
        {
            GUILayout.Space(5);
            pdBackend_.samplePlayback = EditorGUILayout.Popup("Sample File to play", pdBackend_.samplePlayback, samples_);
        }

    }
}
