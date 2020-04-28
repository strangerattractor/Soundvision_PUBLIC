using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderGraphBinder : MonoBehaviour
{
    [SerializeField] private Material material_;
    [SerializeField] private string valueName_ = "ValueName_";
    [SerializeField] private float factor_ = .1f;
    [SerializeField] private float offset_ = 0;
    [SerializeField] private bool logValue = false;

    private float input_ = 1;

    public void OnEnergyChanged(float value)
    {
        input_ = offset_ + value * factor_;
        material_.SetFloat(valueName_, input_);
        if (logValue)
        {
            Debug.Log("Value:" + input_);
        }
    }
}
