using System.Linq;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(PdLevelTracker))]
sealed class PdLevelTrackerEditor : UnityEditor.Editor
{
    #region Private members

    SerializedProperty _pdbackend;
    SerializedProperty _channel;
    SerializedProperty _filterType;
    SerializedProperty _dynamicRange;
    SerializedProperty _autoGain;
    SerializedProperty _gain;
    SerializedProperty _holdAndFallDown;
    SerializedProperty _fallDownSpeed;

    PropertyBinderEditor _propertyBinderEditor;

    #endregion

    #region Labels

    static class Styles
    {
        public static Label NoDevice = "No device available";
        public static Label DefaultDevice = "Default Device";
        public static Label Select = "Select";
        public static Label DynamicRange = "Dynamic Range (dB)";
        public static Label Gain = "Gain (dB)";
        public static Label Speed = "Speed";
    }

    #endregion

    #region Editor implementation

    void OnEnable()
    {
        var finder = new PropertyFinder(serializedObject);

        _pdbackend = finder["_pdbackend"];
        _channel = finder["_channel"];
        _filterType = finder["_filterType"];
        _dynamicRange = finder["_dynamicRange"];
        _autoGain = finder["_autoGain"];
        _gain = finder["_gain"];
        _holdAndFallDown = finder["_holdAndFallDown"];
        _fallDownSpeed = finder["_fallDownSpeed"];

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

        // Input settings
        EditorGUILayout.PropertyField(_pdbackend);
        EditorGUILayout.PropertyField(_channel);
        EditorGUILayout.PropertyField(_dynamicRange, Styles.DynamicRange);
        EditorGUILayout.PropertyField(_autoGain);

        // Show Gain when no peak tracking.
        if (_autoGain.hasMultipleDifferentValues ||
            !_autoGain.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_gain, Styles.Gain);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(_holdAndFallDown);

        // Show Fall Down Speed when "Hold And Fall Down" is on.
        if (_holdAndFallDown.hasMultipleDifferentValues ||
            _holdAndFallDown.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_fallDownSpeed, Styles.Speed);
            EditorGUI.indentLevel--;
        }

        // Draw the level meter during play mode.
        if (RequiresConstantRepaint())
        {
            EditorGUILayout.Space();
            LevelMeterDrawer.DrawMeter((PdLevelTracker)target);
        }

        // Show Reset Peak Level button during play mode.
        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Reset Auto Gain"))
                foreach (PdLevelTracker t in targets) t.ResetAutoGain();
        }

        serializedObject.ApplyModifiedProperties();

        // Property binders
        if (targets.Length == 1) _propertyBinderEditor.ShowGUI();
    }

    #endregion
}

