using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthMaterialsController : MonoBehaviour
{
    [SerializeField] private GameObject materials;

    private Renderer depthTextureRenderer_;

    private static readonly int StartDistanceFromCamera_ = Shader.PropertyToID("_StartDistanceFromCamera");
    [SerializeField] float startDistanceFromCamera = 2;

    private static readonly int ScanColor_ = Shader.PropertyToID("_ScanColor");
    [SerializeField] Color scanColor = Color.green;

    private static readonly int EmissionMultiplier_ = Shader.PropertyToID("_EmissionMultiplier");
    [SerializeField] float emissionMultiplier = 0;

    private static readonly int YTiling_ = Shader.PropertyToID("_YTiling");
    [SerializeField] float yTiling = -0.05f;

    private static readonly int DisplacementFactor_ = Shader.PropertyToID("_DisplacementFactor");
    [SerializeField] float displacementFactor = 10f;

    private void Start()
    {
        depthTextureRenderer_ = materials.GetComponent<Renderer>();
    }

    private void Update()
    {
        var materials = depthTextureRenderer_.materials;
        foreach (var material in materials)
        {
            material.SetFloat(StartDistanceFromCamera_, startDistanceFromCamera);
            material.SetColor(ScanColor_, scanColor);
            material.SetFloat(EmissionMultiplier_, emissionMultiplier);
            material.SetFloat(YTiling_, yTiling);
            material.SetFloat(DisplacementFactor_, displacementFactor);
        }
    }
}
