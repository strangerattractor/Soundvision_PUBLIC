using System;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnitySmoothEvent : UnityEvent<float>
{ }


public class Smoother : MonoBehaviour
{
    [SerializeField, Range(1f, 10f)] private float attackSmooth = 1f;
    [SerializeField, Range(1f, 10f)] private float releaseSmooth = 1f;
    [SerializeField] private bool ignore0;

    [SerializeField] private UnitySmoothEvent onSmoothProcessed;

    private float input_;
    private float previous_;

    public void OnValueChanged(float value)
    {
        if (!(ignore0 && value == 0.0f))
        { 
         input_ = value; //Set new input Value
        }
    }

    private void Update()
    {  
        var distance = input_ - previous_;
        previous_ += distance > 0f? (1f/attackSmooth) * distance : (1f/releaseSmooth) * distance;
        onSmoothProcessed.Invoke(previous_);
    }
}