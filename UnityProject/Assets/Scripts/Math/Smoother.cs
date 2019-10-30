using System;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnitySmoothEvent : UnityEvent<float>
{}


public class Smoother : MonoBehaviour
{
    [SerializeField, Range(1f, 10f)] private float attackSmooth = 1f;
    [SerializeField, Range(1f, 10f)] private float releaseSmooth = 1f;

    [SerializeField] private UnitySmoothEvent onSmoothProcessed;

    private float input_;
    private float previous_;

    public void OnValueChanged(float value)
    {
        input_ = value;
    }

    private void Update()
    {
        var distance = input_ - previous_;
        previous_ += distance > 0f? (1f/attackSmooth) * distance : (1f/releaseSmooth) * distance;
        onSmoothProcessed.Invoke(previous_);
    }
}