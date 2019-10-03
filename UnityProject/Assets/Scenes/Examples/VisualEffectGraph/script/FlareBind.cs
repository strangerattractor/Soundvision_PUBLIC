using UnityEngine;
using UnityEngine.Experimental.VFX;

public class FlareBind : MonoBehaviour
{
    [SerializeField] private VisualEffect flareEffect;
    private static readonly string Spread = "spread";
    
    public void OnEnergyChanged(float energy)
    {
        flareEffect.SetFloat(Spread, energy * 0.01f );
    }
}
