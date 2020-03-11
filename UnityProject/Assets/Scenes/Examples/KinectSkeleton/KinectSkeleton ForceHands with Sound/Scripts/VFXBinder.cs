using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXBinder : MonoBehaviour
{
    [SerializeField] private VisualEffect particleEmissionFX = null;
    private static readonly string attractiveForce = "Attractive Force";
    private static readonly string emissionForce = "Emission Force";

    public void OnEnergyChangedAttraction(float energy)
    {
        particleEmissionFX.SetFloat(attractiveForce, energy * 10f + 1f);
    }

    public void OnEnergyChangedEmission(float energy)
    {
        particleEmissionFX.SetFloat(emissionForce, energy + 0.1f + 1f);
    }
}
