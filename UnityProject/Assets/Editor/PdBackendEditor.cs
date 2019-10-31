using UnityEditor;
using UnityEngine;

namespace cylvester
{
    [CustomEditor(typeof(PdBackend))]
    public class PdBackendEditor : UnityEditor.Editor
    {
        private PdBackend pdBackend_;
        private SerializedProperty midiMessageReceivedProperty_;
        private SerializedProperty midiSyncReceivedProperty_;

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
            "Roses_Front",
            "TimbreID_Test"
        };
        
        private void OnEnable()
        {
            pdBackend_ = (PdBackend) target;
            midiMessageReceivedProperty_ = serializedObject.FindProperty("midiMessageReceived");
            midiSyncReceivedProperty_ = serializedObject.FindProperty("midiSyncReceived");
        }

        public override void OnInspectorGUI ()
        {
            
            serializedObject.Update();
            pdBackend_ = (PdBackend) target;
            EditorGUILayout.PropertyField(midiMessageReceivedProperty_);
            EditorGUILayout.PropertyField(midiSyncReceivedProperty_);
            
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
