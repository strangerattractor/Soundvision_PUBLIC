using UnityEngine;
using UnityEngine.VFX;

public class SmokeBind : MonoBehaviour
{
    [SerializeField] private VisualEffect smokeEffect = null;
    private static readonly string SmokeAnimSpeed = "smokeAnimSpeed";
    
    public void OnEnergyChanged(float energy)
    {
        smokeEffect.SetFloat(SmokeAnimSpeed, energy * 0.1f + 8f);
    }
}
