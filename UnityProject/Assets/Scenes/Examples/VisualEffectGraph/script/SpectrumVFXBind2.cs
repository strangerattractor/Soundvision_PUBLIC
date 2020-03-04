using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpectrumVFXBind2 : MonoBehaviour
{
    [SerializeField] private VisualEffect targetVfx = null;
    private static readonly string spectrumValue = "Spectrum Value 2";

    public void OnEnergyChanged(float energy)
    {
        targetVfx.SetFloat(spectrumValue, energy);
    }
}
