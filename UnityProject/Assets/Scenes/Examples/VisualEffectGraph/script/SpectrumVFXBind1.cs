using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class SpectrumVFXBind1 : MonoBehaviour
{
    [SerializeField] private VisualEffect targetVfx = null;
    private static readonly string spectrumValue = "Spectrum Value 1";

    public void OnEnergyChanged(float energy)
    {
        targetVfx.SetFloat(spectrumValue, energy);
    }
}
