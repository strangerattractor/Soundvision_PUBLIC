using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class BokehBind : MonoBehaviour
{
    private Bloom bloom_;
    [SerializeField] Volume volume = null;
    
    private void Start()
    {
        volume.profile.TryGet(out bloom_);
    }

    public void OnEnergyChanged(float energy)
    {
        bloom_.intensity.value = energy / 500f;
    }
}
