using UnityEngine;
using UnityEngine.VFX;

public class FlareBind : MonoBehaviour
{
    [SerializeField] private VisualEffect flareEffect = null;
    private static readonly string Spread = "spread";
    
    public void OnEnergyChanged(float energy)
    {
        flareEffect.SetFloat(Spread, energy * 0.1f + 0.1f);
    }
}
