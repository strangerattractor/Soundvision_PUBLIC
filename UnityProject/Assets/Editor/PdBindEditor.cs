using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

namespace cylvester
{

    [CustomEditor(typeof(PdBind))]
    class PdBindEditor : UnityEditor.Editor
    {

        private int selectedSpectrum_;
        private Texture2D texture_;
        private readonly string[] channels = {
         "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"
         };

        private Rect paintSpace_;
        private Rect selectedArea_;
        private Rect scaledRect_;

        public void OnEnable()
        {
            texture_ = new Texture2D(256, 256);
        }

        public override void OnInspectorGUI()
        {
            var backend = (PdBackend)target;
            if (Event.current.isMouse && Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                    {
                        var mousePos = Event.current.mousePosition;
                        selectedArea_.x = mousePos.x;
                        selectedArea_.y = mousePos.y;
                        break;
                    }
                    case EventType.MouseDrag:
                    {
                        var mousePos = Event.current.mousePosition;
                        selectedArea_.width = mousePos.x - selectedArea_.x;
                        selectedArea_.height = mousePos.y - selectedArea_.y;
                        UpdateScaledRect();
                        break;
                    }
                }
            }

            GUILayout.Label("PureData Inputs", EditorStyles.boldLabel);

            selectedSpectrum_ = EditorGUILayout.Popup("Input Channel", selectedSpectrum_, channels);
            GUILayout.Space(30);
            GUILayout.Label("Spectrum Extractor", EditorStyles.boldLabel);
            
            var paintSpace = GUILayoutUtility.GetRect(256, 512, 256, 256);
            if (Event.current.type == EventType.Repaint)
            {
                paintSpace_ = paintSpace;
                RenderTexture();
                GUI.DrawTexture(paintSpace_, texture_);
            }

            Repaint();
        }

        private void RenderTexture()
        {
            for (var y = 0; y < texture_.height; y++)
            {
                for (var x = 0; x < texture_.width; x++)
                {
                    var alpha = 0.4f;
                    if ((scaledRect_.x < x && x < (scaledRect_.x + scaledRect_.width)) &&
                        (scaledRect_.y < y && y < (scaledRect_.y + scaledRect_.height)))
                    {
                        alpha = 1f;
                    }
                    var color = new Color(Random.value, Random.value, Random.value, alpha);
                    texture_.SetPixel(x, 256-y, color);
                }
            }
            texture_.Apply();
        }


        private void UpdateScaledRect()
        {
            var xPos = (selectedArea_.x - paintSpace_.x) / paintSpace_.width;
            var yPos = (selectedArea_.y - paintSpace_.y) / paintSpace_.height;
            var width = selectedArea_.width / paintSpace_.width;
            var height = selectedArea_.height / paintSpace_.height;

            scaledRect_.x = xPos * texture_.width;
            scaledRect_.y = yPos * texture_.height;
            scaledRect_.width = width * texture_.width;
            scaledRect_.height = height * texture_.height;
            
            Debug.Log(scaledRect_);
        }
    }
}
