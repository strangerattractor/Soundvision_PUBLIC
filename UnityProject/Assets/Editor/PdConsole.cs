using UnityEditor;
using UnityEngine;

namespace cylvester
{
    public class PdConsole : EditorWindow
    {
        private IEditorToggle dspToggle_;
        private ITogglePresenter togglePresenter_;
        private IPdBackend pdBackend_;
        private LevelMeter[] levelMeters_;

        [MenuItem("SoundVision/Pd console %#p")]
        static void Init()
        {
            var window = (PdConsole)GetWindow(typeof(PdConsole));
            window.Show();

            
        }

        private void OnEnable()
        { 
            var foundObjects = FindObjectsOfType(typeof(PdBackend));
            if (foundObjects.Length != 1)
                return;

            pdBackend_ = (IPdBackend) foundObjects[0];
            dspToggle_ = new EditorToggle();
            
            togglePresenter_ = new TogglePresenter(dspToggle_, pdBackend_);
            levelMeters_ = new LevelMeter[16];
            for (var i = 0; i < 16; ++i)
                levelMeters_[i] = new LevelMeter(i);
        }

        private void OnGUI ()
        {
            if(!ValidatePdBackend(pdBackend_))
                return;
            
            EditorGUILayout.Space();
            dspToggle_.State = EditorGUILayout.Toggle("Pure Data Process", dspToggle_.State);
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            foreach (var levelMeter in levelMeters_)
                levelMeter.Render();

            EditorGUILayout.EndHorizontal();

        }
        


        private bool ValidatePdBackend(IPdBackend pdBackend)
        {
            var exist = pdBackend_ != null;
            if (!exist)
            {
                EditorGUILayout.LabelField("No Pd backend found in the scene");
            }
            return exist;
        }
    }

}
