using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class PitchVFXBind : MonoBehaviour
{
    [SerializeField] private VisualEffect targetVfx = null;
    private static readonly string pitchValue = "Pitch Value";

    public void OnEnergyChanged(float energy)
    {
        targetVfx.SetFloat(pitchValue, energy);
    }
}
