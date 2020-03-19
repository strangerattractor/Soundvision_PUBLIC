using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderGraphKeijiroBinder : MonoBehaviour
{
    [SerializeField] private Material material_;
    [SerializeField] private string valueName_ = "ValueName_";

    private float _val;

    public float val
    {
        get => _val;
        set => _val = value;
    }

    void Update()
    {
        material_.SetFloat(valueName_, _val);
    }
}
