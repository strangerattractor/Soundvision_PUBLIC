using UnityEngine;
using UnityEditor;

namespace cylvester
{
    public class LevelMeter
    {
        private string label_;
        private float dB_;
        private Texture2D meterImageTexture_;
        
        public LevelMeter(int index)
        {
            label_ = (index + 1).ToString();
            meterImageTexture_ = new Texture2D(1, 32);
        }
        
        public void Render()
        {
            UpdateTexture();
            var rect = EditorGUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fixedHeight = 120
            };
            GUILayout.Label(label_, style);
            rect.y += 20;
            rect.height -= 20;
            EditorGUI.DrawPreviewTexture(rect, meterImageTexture_);
            EditorGUILayout.EndVertical();
        }
        
        private void UpdateTexture()
        {
            for (var i = 0; i < 32; ++i)
            {
                meterImageTexture_.SetPixel(0, i, i < 10 ? Color.green : Color.black);
            }
            meterImageTexture_.Apply();
        }

    }
}