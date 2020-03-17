using UnityEngine;

//
// Unity component used to track audio input level and drive other
// components via UnityEvent
//
[AddComponentMenu("PD Level Tracker")]
public sealed class PdLevelTracker : MonoBehaviour
{
    #region Editor attributes and public properties

    [SerializeField] cylvester.PdBackend _pdbackend;
    public cylvester.PdBackend pdbackend
    {
        get => _pdbackend;
        set => _pdbackend = value;
    }

    // Channel Selection
    [SerializeField, Range(0, 15)] int _channel = 0;
    public int channel
    {
        get => _channel;
        set => _channel = value;
    }

    // Auto gain control switch
    [SerializeField] bool _autoGain = true;
    public bool autoGain
    {
        get => _autoGain;
        set => _autoGain = value;
    }

    // Manual input gain (only used when auto gain is off)
    [SerializeField, Range(-10, 40)] float _gain = 6;
    public float gain
    {
        get => _gain;
        set => _gain = value;
    }

    // Dynamic range in dB
    [SerializeField, Range(1, 40)] float _dynamicRange = 12;
    public float dynamicRange
    {
        get => _dynamicRange;
        set => _dynamicRange = value;
    }

    // "Hold and fall down" animation switch
    [SerializeField] bool _holdAndFallDown = true;
    public bool holdAndFallDown
    {
        get => _holdAndFallDown;
        set => _holdAndFallDown = value;
    }

    // Fall down animation speed
    [SerializeField, Range(0, 1)] float _fallDownSpeed = 0.3f;
    public float fallDownSpeed
    {
        get => _fallDownSpeed;
        set => _fallDownSpeed = value;
    }

    // Property binders
    [SerializeReference] PropertyBinder[] _propertyBinders = null;
    public PropertyBinder[] propertyBinders
    {
        get => (PropertyBinder[])_propertyBinders.Clone();
        set => _propertyBinders = value;
    }

    #endregion

    #region Runtime public properties and methods

    // Current input gain (dB)
    public float currentGain => _autoGain ? -_head : _gain;

    // Unprocessed input level (dBFS)
    public float inputLevel
        //=> Random.Range(-70.0f, -30.0f);//
        => pdbackend.LevelArray.Data[_channel] - 100;
    // Stream?.GetChannelLevel(_channel, _filterType) ?? kSilence;

    // Curent level in the normalized scale
    public float normalizedLevel => _normalizedLevel;

    private cylvester.IPdArraySelector waveformArraySelector_;

    // Raw wave audio data as NativeSlice
    public Unity.Collections.NativeSlice<float> AudioDataSlice()
    {
        // probably not right
        waveformArraySelector_.Selection = _channel;
        var array = waveformArraySelector_.SelectedArray;

        var s = new Unity.Collections.NativeSlice<float>();
        s.CopyFrom(array);
        return s;
    }

    // Reset the auto gain state.
    public void ResetAutoGain() => _head = kSilence;

    #endregion

    #region Private members

    // Silence: Locally defined noise floor level (dBFS)
    const float kSilence = -60;

    // Current normalized level value
    float _normalizedLevel = 0;

    // Nominal level of auto gain (recent maximum level)
    float _head = kSilence;

    // Hold and fall down animation parameter
    float _fall = 0;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        waveformArraySelector_ = new cylvester.PdArraySelector(pdbackend.WaveformArrayContainer);
    }

    void Update()
    {
        var input = inputLevel;
        var dt = Time.deltaTime;

        // Auto gain control
        if (_autoGain)
        {
            // Slowly return to the noise floor.
            const float kDecaySpeed = 0.6f;
            _head = Mathf.Max(_head - kDecaySpeed * dt, kSilence);

            // Pull up by input with a small headroom.
            var room = _dynamicRange * 0.05f;
            _head = Mathf.Clamp(input - room, _head, 0);
        }

        // Normalize the input value.
        var normalizedInput
            = Mathf.Clamp01((input + currentGain) / _dynamicRange + 1);

        if (_holdAndFallDown)
        {
            // Hold and fall down animation
            _fall += Mathf.Pow(10, 1 + _fallDownSpeed * 2) * dt;
            _normalizedLevel -= _fall * dt;

            // Pull up by input.
            if (_normalizedLevel < normalizedInput)
            {
                _normalizedLevel = normalizedInput;
                _fall = 0;
            }
        }
        else
        {
            _normalizedLevel = normalizedInput;
        }

        // Output
        if (_propertyBinders != null)
            foreach (var b in _propertyBinders) b.Level = _normalizedLevel;
    }

    #endregion
}
