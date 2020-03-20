using UnityEngine;
using UnityEditor;

namespace cylvester
{
    [CustomEditor(typeof(PdFrequencyBandBind))]
    class PdFrequencyBandBindEditor : UnityEditor.Editor
    {
        private readonly string[] channels_ =
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"
        };

        private IRectangularSelection rectangularSelection_;

        private SerializedProperty selectionProperty_;
        private SerializedProperty pdBackendProperty_;
        private SerializedProperty energyChangedProperty_;
        private SerializedProperty channelProperty_;
        private SerializedProperty renderSpectrumProperty_;
        private SerializedProperty numBinsProperty_;
        private SerializedProperty targetBinProperty_;
        private SerializedProperty inputMinProperty_;
        private SerializedProperty inputMaxProperty_;
        private Rect paintSpace_;
        private ISpectrumGenerator spectrumGeneratorEditMode_;

        public void OnEnable()
        {
            var behaviour = (PdFrequencyBandBind) target;

            pdBackendProperty_ = serializedObject.FindProperty("pdBackend");
            selectionProperty_ = serializedObject.FindProperty("selection");
            energyChangedProperty_ = serializedObject.FindProperty("frequencyBandChanged");
            channelProperty_ = serializedObject.FindProperty("channel");
            renderSpectrumProperty_ = serializedObject.FindProperty("renderSpectrum");
            numBinsProperty_ = serializedObject.FindProperty("numBins");
            targetBinProperty_ = serializedObject.FindProperty("targetBin");
            inputMinProperty_ = serializedObject.FindProperty("inputMin");
            inputMaxProperty_ = serializedObject.FindProperty("inputMax");
            rectangularSelection_ = new RectangularSelection(behaviour.TextureWidth, behaviour.TextureHeight);
            spectrumGeneratorEditMode_ = new SpectrumGeneratorEditMode(behaviour.TextureWidth, behaviour.TextureHeight);
        }

        public override void OnInspectorGUI()
        {
            var behaviour = (PdFrequencyBandBind) target;
            EditorGUILayout.PropertyField(pdBackendProperty_);

            GUILayout.Label("PureData Inputs", EditorStyles.boldLabel);
            channelProperty_.intValue = EditorGUILayout.Popup("Input Channel", channelProperty_.intValue, channels_);

            GUILayout.Label("Callback", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(energyChangedProperty_);

            GUILayout.Space(5);
            GUILayout.Label("Spectrum Extractor", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(numBinsProperty_);
            EditorGUILayout.PropertyField(targetBinProperty_);
            EditorGUILayout.PropertyField(inputMaxProperty_);
            EditorGUILayout.PropertyField(inputMinProperty_);
            EditorGUILayout.PropertyField(renderSpectrumProperty_);
            paintSpace_ = GUILayoutUtility.GetRect(behaviour.TextureWidth, behaviour.TextureWidth,
                behaviour.TextureHeight, behaviour.TextureHeight);
            
            UpdateSelection();

            if (Event.current.type == EventType.Repaint)
            {
                // update selection
                if (Application.isPlaying)
                {
                    GUI.DrawTexture(paintSpace_, behaviour.Spectrum);
                }
                else
                {
                    GUI.DrawTexture(paintSpace_, spectrumGeneratorEditMode_.Spectrum);
                }
            }

            Repaint();

            RenderExtractedEnergy(behaviour.Energy);

            serializedObject.ApplyModifiedProperties();
        }


        private void UpdateSelection()
        {
        }

        private void RenderExtractedEnergy(float energy)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Extracted Energy", EditorStyles.boldLabel);
            GUILayout.Label(energy.ToString());
            GUILayout.EndHorizontal();
        }
    }
}