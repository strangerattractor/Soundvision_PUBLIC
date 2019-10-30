using UnityEditor;
using UnityEngine;

namespace cylvester
{
    [CustomEditor(typeof(Sequencer))]
    public class SequencerEditor : Editor
    {
        private SerializedProperty sequenceProperty_;
        private void OnEnable()
        { 
            sequenceProperty_ = serializedObject.FindProperty("sequence");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update ();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Trigger on:", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(160));

            for (var i = 0; i < 16; ++i)
            {
                sequenceProperty_.GetArrayElementAtIndex(i).boolValue 
                    = EditorGUILayout.Toggle(sequenceProperty_.GetArrayElementAtIndex(i).boolValue);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties ();
        }
    }

}
