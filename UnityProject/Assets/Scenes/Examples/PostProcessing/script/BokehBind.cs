using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BokehBind : MonoBehaviour
{
    [SerializeField] private PostProcessVolume volume;

    private DepthOfField depthOfField_;
    
    private void Start()
    {
        volume.profile.TryGetSettings(out depthOfField_);
    }

    public void OnEnergyChanged(float energy)
    {
        depthOfField_.focusDistance.value = 0.1f + energy / 100f;
    }
}
