using UnityEditor;
using UnityEngine;

namespace cylvester
{
    [CustomEditor(typeof(PdBackend))]
    public class PdBackendEditor : Editor
    {
        private PdBackend pdBackend_;
        private ILevelMeter[] levelMeters_;
        private readonly string[] channels_ = {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"
        };

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
            levelMeters_ = new ILevelMeter[16];
            for (var i = 0; i < 16; ++i)
                levelMeters_[i] = new LevelMeter(i);
        }

        public override void OnInspectorGUI ()
        {
            pdBackend_ = (PdBackend) target;

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Main Patch");
            pdBackend_.mainPatch = GUILayout.TextField(pdBackend_.mainPatch, 30);
            GUILayout.EndHorizontal();
            
            if (Application.isPlaying)
            {
                RenderSamplePlayback();
                RenderLevelMeters();
                Repaint();
            }
        }

        private void RenderSamplePlayback()
        {
            GUILayout.Space(5);
            pdBackend_.samplePlayback = EditorGUILayout.Popup("Sample File to play", pdBackend_.samplePlayback, samples_);
        }

        private void RenderLevelMeters()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            foreach (var levelMeter in levelMeters_)
                levelMeter.Render(pdBackend_.levelMeterArray);
            GUILayout.EndHorizontal();
        }
    }
}
