using UnityEditor;
using UnityEngine;

namespace cylvester
{
    [CustomEditor(typeof(MidiSequencer))]
    public class MidiSequencerEditor : UnityEditor.Editor
    {
        private SerializedProperty timeProperty_;
        private SerializedProperty sequenceProperty_;
        private SerializedProperty callbackProperty_;
        private readonly string[] options_ = new string[] {"3", "4", "5", "6"};

        private void OnEnable()
        {
            timeProperty_ = serializedObject.FindProperty("time");
            sequenceProperty_ = serializedObject.FindProperty("sequence");
            callbackProperty_ = serializedObject.FindProperty("triggered");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update ();
            
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Time:", GUILayout.Width(120));
                timeProperty_.intValue  = EditorGUILayout.Popup(timeProperty_.intValue - 3, options_) + 3;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Trigger on:", GUILayout.Width(120));
                for (var i = 0; i < 4 * timeProperty_.intValue; ++i)
                {
                    sequenceProperty_.GetArrayElementAtIndex(i).boolValue 
                        = EditorGUILayout.Toggle(sequenceProperty_.GetArrayElementAtIndex(i).boolValue, GUILayout.Width(10));
                }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(callbackProperty_);
            serializedObject.ApplyModifiedProperties ();
        }
    }

}
