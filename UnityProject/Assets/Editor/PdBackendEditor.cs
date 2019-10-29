using UnityEditor;
using UnityEngine;

namespace cylvester
{
    [CustomEditor(typeof(PdBackend))]
    public class PdBackendEditor : Editor
    {
        private PdBackend pdBackend_;
        private SerializedProperty midiMessageReceivedProperty_;
        private SerializedProperty midiClockReceivedProperty_;

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
        
        private void Awake()
        {
            pdBackend_ = (PdBackend) target;
        }

        public override void OnInspectorGUI ()
        {
            
            serializedObject.Update();
            pdBackend_ = (PdBackend) target;
            midiMessageReceivedProperty_ = serializedObject.FindProperty("midiMessageReceived");
            EditorGUILayout.PropertyField(midiMessageReceivedProperty_);
            
            midiClockReceivedProperty_ = serializedObject.FindProperty("midiClockReceived");
            EditorGUILayout.PropertyField(midiClockReceivedProperty_);
            
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
