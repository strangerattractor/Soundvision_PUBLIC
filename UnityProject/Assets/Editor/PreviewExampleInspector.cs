using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(PreviewExample))]
public class PreviewExampleInspector : Editor {

    public override bool HasPreviewGUI() { return true; }

    public override GUIContent GetPreviewTitle() { return new GUIContent("name"); }
    public override void OnPreviewGUI(Rect r, GUIStyle background) {
        GUI.Box(new Rect(10, 10, 100, 100), "Preview");
    }

}
