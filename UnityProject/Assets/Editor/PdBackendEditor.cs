using UnityEditor;
using UnityEngine;

namespace cylvester
{
    [CustomEditor(typeof(PdBackend))]
    public class PdBackendEditor : Editor
    {
        private PdBackend pdBackend_;

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
            pdBackend_ = (PdBackend) target;

            if (Application.isPlaying)
            {
                RenderSamplePlayback();
                Repaint();
            }
        }

        private void RenderSamplePlayback()
        {
            GUILayout.Space(5);
            pdBackend_.samplePlayback = EditorGUILayout.Popup("Sample File to play", pdBackend_.samplePlayback, samples_);
        }

    }
}
