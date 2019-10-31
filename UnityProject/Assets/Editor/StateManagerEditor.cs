using UnityEditor;
using UnityEngine;

namespace cylvester
{
    
    [CustomEditor(typeof(StateManager))]
    public class StateManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI ()
        {
            var csvFileName = serializedObject.FindProperty("csvFileName");
            var onStateChanged = serializedObject.FindProperty("onStateChanged");
            var sceneSelection = serializedObject.FindProperty("sceneSelection");
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(csvFileName);
            EditorGUILayout.PropertyField(onStateChanged);
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Current State");
                var newValue = EditorGUILayout.Popup(sceneSelection.intValue, ((IStateManager) target).StateTitles);
                if (newValue != sceneSelection.intValue)
                    ((IStateManager) target).State = newValue;
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
        
    }
    
}