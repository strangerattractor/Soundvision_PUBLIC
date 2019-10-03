using UnityEngine;
using UnityEngine.Experimental.VFX;

public class SmokeBind : MonoBehaviour
{
    [SerializeField] private VisualEffect smokeEffect;
    private static readonly string SmokeAnimSpeed = "smokeAnimSpeed";
    
    public void OnEnergyChanged(float energy)
    {
        smokeEffect.SetFloat(SmokeAnimSpeed, energy * 0.1f + 8f);
    }
}
