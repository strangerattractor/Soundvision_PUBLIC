using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookupTextureSGBinder : MonoBehaviour
{
    [SerializeField] private LookupTexture lookupTexture;
    [SerializeField] private GameObject TextureOutput;

    private Renderer lookupTextureRenderer_;

    private static readonly int lookupTexture_ = Shader.PropertyToID("_LookupTexture");
    private static readonly int lookupTextureIndex_ = Shader.PropertyToID("_LookupTextureIndex");
    private static readonly int lookupTextureLength_ = Shader.PropertyToID("_LookupTextureLength");

    private void Start()
    {
        lookupTextureRenderer_ = TextureOutput.GetComponent<Renderer>();
    }


    private void Update()
    {
        var materials = lookupTextureRenderer_.materials;
        foreach(var material in materials)
        {
            material.SetTexture(lookupTexture_, lookupTexture.Texture);
            material.SetInt(lookupTextureIndex_, lookupTexture.Index);
            material.SetInt(lookupTextureLength_, lookupTexture.Length);
        }
    }

}

