using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[Serializable]
public class UnitySmoothEvent : UnityEvent<float>
{ }


public class Smoother : MonoBehaviour
{
    [SerializeField, Range(1f, 50f)] private float attackSmooth = 1f;
    [SerializeField, Range(1f, 50f)] private float releaseSmooth = 1f;
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
            input_ = offset + value * scale; //Set new input Value
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