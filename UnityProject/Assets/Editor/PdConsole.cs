using System;
using UnityEditor;

namespace cylvester
{
    public class PdConsole : EditorWindow
    {
        private const int NumChannels = 16;
        
        private IEditorToggle dspToggle_;
        private ITogglePresenter togglePresenter_;
        private Action onDspToggleStateChanged_;
        private IPdBackend pdBackend_;
        private LevelMeter[] levelMeters_;
        private PdArray levelMeterArray_;

        [MenuItem("SoundVision/Pd console %#p")]
        static void Init()
        {
            var window = (PdConsole)GetWindow(typeof(PdConsole));
            window.Show();
        }

        private void Awake()
        { 
            var foundObjects = FindObjectsOfType(typeof(PdBackend));
            if (foundObjects.Length != 1)
                return;

            pdBackend_ = (IPdBackend) foundObjects[0];
            dspToggle_ = new EditorToggle();
            
            onDspToggleStateChanged_ = () =>
            {
                if (pdBackend_.State)
                {
                    levelMeterArray_ = new PdArray("levelmeters", NumChannels);
                    for (var i = 0; i < NumChannels; ++i)
                        levelMeters_[i] = new LevelMeter(i, levelMeterArray_);
                }
                else
                    levelMeterArray_.Dispose();
            };

            pdBackend_.StateChanged += onDspToggleStateChanged_;
            togglePresenter_ = new TogglePresenter(dspToggle_, pdBackend_);
            levelMeters_ = new LevelMeter[NumChannels];
            

        }

        private void OnDestroy()
        {
            levelMeterArray_?.Dispose();
            dspToggle_.ToggleStateChanged -= onDspToggleStateChanged_;
        }

        private void OnGUI ()
        {
            if(!ValidatePdBackend())
                return;

            EditorGUILayout.Space();
            dspToggle_.State = EditorGUILayout.Toggle("Pure Data Process", dspToggle_.State);

            if(!CheckProcessingState())
                return;
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            levelMeterArray_.Update();
            foreach (var levelMeter in levelMeters_)
                levelMeter.Render();

            EditorGUILayout.EndHorizontal();
            
            Repaint();

        }
        private bool ValidatePdBackend()
        {
            if (pdBackend_ != null) 
                return true;
            EditorGUILayout.LabelField("No Pd backend found in the scene");
            return false;
        }

        private bool CheckProcessingState()
        {
            if (pdBackend_.State) 
                return true;
            EditorGUILayout.LabelField("Pd process is currently inactive");
            return false;
        }
        
    }

}
