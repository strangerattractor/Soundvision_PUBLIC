using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXEmitterBinder : MonoBehaviour
{
    [SerializeField] private VisualEffect particleEmissionFX = null;
    private static readonly string emissionForce = "Emission Force";
    [SerializeField] private float baseForce = 1;
    [SerializeField] private float multiplier = 1;

    public void OnEnergyChangedEmission(float energy)
    {
        particleEmissionFX.SetFloat(emissionForce, energy * multiplier + baseForce);
        Debug.Log("Emitter " + energy);
    }
}
