using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class VFXAttractorBinder : MonoBehaviour
{

    [SerializeField] private VisualEffect particleEmissionFX = null;
    [SerializeField] private float baseForce = 10;
    [SerializeField] private float multiplier = 1;
    private static readonly string attractiveForce = "Attractive_Force";

    public void OnEnergyChangedAttraction(float energy)
    {
        particleEmissionFX.SetFloat(attractiveForce, (energy * multiplier) + baseForce);
        Debug.Log("Attraction " + energy);
    }
}
