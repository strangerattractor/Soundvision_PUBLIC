using UnityEditor;
using UnityEngine;

namespace cylvester
{
    public class PdConsole : EditorWindow
    {
        private IEditorToggle dspToggle_;
        private ITogglePresenter togglePresenter_;
        private IPdBackend pdBackend_;

        [MenuItem("SoundVision/Pd console %#p")]
        static void Init()
        {
            var window = (PdConsole)GetWindow(typeof(PdConsole));
            window.Show();
        }

        private void OnEnable()
        {
            object[] foundObjects  = FindObjectsOfType(typeof(PdBackend));
            
            
            dspToggle_ = new EditorToggle();
           togglePresenter_ = new TogglePresenter(dspToggle_, pdBackend_);
        }

        void OnGUI ()
        {
            foundObjects.
            dspToggle_.State = EditorGUILayout.Toggle("Pure Data Process", dspToggle_.State);

        }
    }

}
