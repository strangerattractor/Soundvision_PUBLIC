using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionBind : MonoBehaviour
{
    [SerializeField] private Material emissionBoost;
    private static readonly string emissionIntensity = "_BoostEmission";

    public void OnEnergyChanged (float energy)
    {
        emissionBoost.SetFloat(emissionIntensity, energy * .1f);
    }
}
