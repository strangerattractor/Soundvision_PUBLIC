using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookupTextureMapper : MonoBehaviour
{
    [SerializeField] private LookupTexture lookupTexture;
    [SerializeField] private GameObject TextureOutput;

    private Renderer lookupTextureRenderer_;

    private static readonly int lookupTexture_ = Shader.PropertyToID("_BaseColorMap");
    private static readonly int lookupTextureIndex_ = Shader.PropertyToID("_LookupTextureIndex");
    private static readonly int lookupTextureLength_ = Shader.PropertyToID("_LookupTextureLength");

    private void Start()
    {
        lookupTextureRenderer_ = TextureOutput.GetComponent<Renderer>();
    }


    private void Update()
    {
        lookupTextureRenderer_.material.SetTexture(lookupTexture_, lookupTexture.Texture);
        lookupTextureRenderer_.material.SetInt(lookupTextureIndex_, lookupTexture.Index);
        lookupTextureRenderer_.material.SetInt(lookupTextureLength_, lookupTexture.Length);
    }

}

