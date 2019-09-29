using UnityEditor;
using UnityEngine;

namespace cylvester
{
    [CustomEditor(typeof(PdBackend))]
    public class PdBackendEditor : Editor
    {
        private IPdBackend pdBackend_;
        private ILevelMeter[] levelMeters_;
        private readonly string[] channels = {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"
        };
        
        private void Awake()
        {
            pdBackend_ = (IPdBackend) target;
            levelMeters_ = new ILevelMeter[16];
            for (var i = 0; i < 16; ++i)
                levelMeters_[i] = new LevelMeter(i);
        }

        public override void OnInspectorGUI ()
        {
            pdBackend_ = (IPdBackend) target;

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Main Patch");
            pdBackend_.MainPatch = GUILayout.TextField(pdBackend_.MainPatch, 30);
            GUILayout.EndHorizontal();

            pdBackend_.NumInputChannels = EditorGUILayout.Popup("Number of input channels", pdBackend_.NumInputChannels, channels);

            if (Application.isPlaying)
            {

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                foreach (var levelMeter in levelMeters_)
                    levelMeter.Render(pdBackend_.LevelMeterArray);
                GUILayout.EndHorizontal();

                Repaint();
            }
        }
    }
}
