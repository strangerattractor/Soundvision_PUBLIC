using UnityEditor;

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
            var foundObjects = FindObjectsOfType(typeof(PdBackend));
            if (foundObjects.Length != 1)
                return;

            pdBackend_ = (IPdBackend) foundObjects[0];
            dspToggle_ = new EditorToggle();
            
            togglePresenter_ = new TogglePresenter(dspToggle_, pdBackend_);
        }

        private void OnGUI ()
        {
            if(!ValidatePdBackend(pdBackend_))
                return;
            
            dspToggle_.State = EditorGUILayout.Toggle("Pure Data Process", dspToggle_.State);

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
