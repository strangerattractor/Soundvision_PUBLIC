using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class UnityAveragerEvent : UnityEvent<float>
{ }


public class Averager : MonoBehaviour
{
    [SerializeField] [Range(1, 1000)] private int bufferSize = 1;
    [SerializeField] private bool Log;
    private List<float> valueList_;
    private float average_;

    [SerializeField] private UnityAveragerEvent onAverageProcessed;

    private float input_;
    private int index_;

    private bool bufferSizeChanged = false;
    private int oldBufferSize_;

    private void Start()
    {
        oldBufferSize_ = bufferSize;
        ConstructBufferList(oldBufferSize_);
    }

    private void ConstructBufferList(int length_)
    {
        index_ = 0;
        valueList_ = new List<float>(length_);
        for (int i = 0; i < length_; i++)
        { 
            valueList_.Add(0f);
        }

    }

    public void OnValueChanged(float value)
    {
        input_ = value;

        if (Log)
        {
            Debug.Log("Averaged Vaule: " + average_);
        }
    }

    private void Update()
    {
        valueList_[index_] = input_;
        average_ = valueList_.Average();
        index_++;
        index_ %= bufferSize;
        onAverageProcessed.Invoke(average_);
        
        if (oldBufferSize_ != bufferSize)
        {
            index_ = 0;
            ConstructBufferList(bufferSize);
        }
        oldBufferSize_ = bufferSize;
    }
}