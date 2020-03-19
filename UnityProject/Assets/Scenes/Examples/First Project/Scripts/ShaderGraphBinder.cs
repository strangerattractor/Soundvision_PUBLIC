using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderGraphBinder : MonoBehaviour
{
    [SerializeField] private Material material_;
    [SerializeField] private string valueName_ = "ValueName_";
    [SerializeField] private float factor_ = .1f;
    [SerializeField] private bool logLevel = false;

    public void OnEnergyChanged(float value)
    {
        material_.SetFloat(valueName_, value * factor_);
        if (logLevel)
        {
            Debug.Log("Level:" + value);
        }
    }
}
