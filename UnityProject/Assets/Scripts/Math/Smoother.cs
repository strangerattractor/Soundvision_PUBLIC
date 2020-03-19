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
    [SerializeField] private float scale = 1;
    [SerializeField] private float offset = 0;
    [SerializeField] private bool Log;

    [SerializeField] private UnitySmoothEvent onSmoothProcessed;

    private float input_;
    private float previous_;

    public void OnValueChanged(float value)
    {
        if (!(ignore0 && value == 0.0f))
        { 
         input_ = value * scale + offset; //Set new input Value
        }

        if(Log)
        {
         Debug.Log("Smoothed Vaule: " + input_);
        }

    }

    private void Update()
    {  
        var distance = input_ - previous_;
        previous_ += distance > 0f? (1f/attackSmooth) * distance : (1f/releaseSmooth) * distance;
        onSmoothProcessed.Invoke(previous_);
    }
}