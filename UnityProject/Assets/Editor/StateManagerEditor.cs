using UnityEditor;
using UnityEngine;

namespace cylvester
{
    [CustomEditor(typeof(StateManager))]
    public class StateManagerEditor : Editor
    {
        private string[] titles_;
        
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
                var states = ((IStateManager) target).States;
                var newValue = EditorGUILayout.Popup(sceneSelection.intValue, GetTitles(states));
                if (newValue != sceneSelection.intValue)
                    ((IStateManager) target).SelectedState = newValue;
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        private string[] GetTitles(State[] states)
        {
            if (titles_ != null)
                return titles_;
            
            titles_ = new string[states.Length];
            
            for (var i = 0; i < states.Length; ++i)
                titles_[i] = states[i].Title;

            return titles_;
        }
    }
}