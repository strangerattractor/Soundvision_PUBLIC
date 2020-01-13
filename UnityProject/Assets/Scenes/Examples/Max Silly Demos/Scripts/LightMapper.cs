using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMapper : MonoBehaviour
{
    [SerializeField] private Light light;
    [SerializeField] private float minimum = 100;
    [SerializeField] private float multiplier = 100;

    public void OnEnergyChanged(float energy)
    {
        light.intensity = energy * multiplier + minimum;
    }
}

