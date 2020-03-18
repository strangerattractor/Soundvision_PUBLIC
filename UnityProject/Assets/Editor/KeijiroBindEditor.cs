using System.Linq;
using UnityEngine;
using UnityEditor;
using Lasp.Editor;

[CanEditMultipleObjects]
[CustomEditor(typeof(KeijiroBind))]
sealed class KeijiroBindEditor : UnityEditor.Editor
{
    #region Private members

    SerializedProperty _inputMin;
    SerializedProperty _inputMax;

    PropertyBinderEditor _propertyBinderEditor;

    #endregion

    #region Editor implementation

    void OnEnable()
    {
        var finder = new PropertyFinder(serializedObject);

        _inputMin = finder["_inputMin"];
        _inputMax = finder["_inputMax"];

        _propertyBinderEditor
            = new PropertyBinderEditor(finder["_propertyBinders"]);
    }

    public override bool RequiresConstantRepaint()
    {
        // Keep updated while playing.
        return Application.isPlaying && targets.Length == 1;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_inputMin);
        EditorGUILayout.PropertyField(_inputMax);

        serializedObject.ApplyModifiedProperties();

        // Property binders
        if (targets.Length == 1) _propertyBinderEditor.ShowGUI();
    }

    #endregion
}

