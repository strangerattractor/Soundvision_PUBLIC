using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderGraphBinder : MonoBehaviour
{
    [SerializeField] private Material material_;
    [SerializeField] private string valueName_ = "ValueName_";

    public void OnEnergyChanged(float value)
    {
        material_.SetFloat(valueName_, value * .1f);
    }
}
