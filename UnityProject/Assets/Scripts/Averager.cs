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
    [SerializeField] [Range(1, 10000)] private int bufferSize = 1;
    [SerializeField] private bool Log;
    private List<float> valueList;
    private float average_;

    [SerializeField] private UnityAveragerEvent onAverageProcessed;

    private float input_;
    private int index_;

    private float total;

    private void Start()
    {
        valueList = new List<float>(Mathf.Abs(bufferSize));
        valueList.Clear();
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
        valueList.Insert(index_, input_);
        average_ = valueList.Average();

        index_++;
        index_ %= bufferSize;

        onAverageProcessed.Invoke(average_);
    }
}