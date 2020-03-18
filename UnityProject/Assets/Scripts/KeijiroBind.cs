using UnityEngine;
using Lasp;

//
// Unity component used to track audio input level and drive other
// components via UnityEvent
//
[AddComponentMenu("Keijiro Bind")]
public sealed class KeijiroBind : MonoBehaviour
{
    #region Editor attributes and public properties

    // Property binders
    [SerializeReference] float _inputMin = 0;
    [SerializeReference] float _inputMax = 100;
    [SerializeReference] PropertyBinder[] _propertyBinders = null;
    public PropertyBinder[] propertyBinders
    {
        get => (PropertyBinder[])_propertyBinders.Clone();
        set => _propertyBinders = value;
    }

    #endregion

    #region Runtime public properties and methods

    // Curent level in the normalized scale
    public float normalizedLevel => _normalizedLevel;

    #endregion

    #region Private members

    float _normalizedLevel;

    #endregion

    #region Bind event implementation

    public void OnValueReceived(float value)
    {
        _normalizedLevel = Mathf.Clamp01((value - _inputMin) / (_inputMax - _inputMin));
    }

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        var dt = Time.deltaTime;

        // Output
        if (_propertyBinders != null)
            foreach (var b in _propertyBinders) b.Level = _normalizedLevel;
    }

    #endregion
}
