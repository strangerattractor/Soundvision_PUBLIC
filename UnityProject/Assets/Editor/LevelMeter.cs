using UnityEngine;
using UnityEditor;

namespace cylvester
{
    interface ILevelMeter
    {
        void Render();
    }
    
    public class LevelMeter : ILevelMeter
    {
        private const int TextureWidth = 1;
        private const int TextureHeight = 100;

        private readonly Texture2D meterImageTexture_;
        private readonly int index_;
        private readonly string label_;
        private readonly IPdArray pdArray_;
        
        public LevelMeter(int index, IPdArray pdArray)
        {
            index_ = index;
            label_ = (index_ + 1).ToString();
            pdArray_ = pdArray;
            meterImageTexture_ = new Texture2D(TextureWidth, TextureHeight);
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
            var level = pdArray_.Data[index_];
            for (var i = 0; i < 100; ++i)
            {
                meterImageTexture_.SetPixel(0, i, i < level ? Color.green : Color.black);
            }
            meterImageTexture_.Apply();
        }

    }
}