using UnityEngine;
using UnityEngine.VFX;

public class LookupTextureVFXBinder : MonoBehaviour
{
        [SerializeField]  LookupTexture lookupTexture;
        [SerializeField]  VisualEffect visualEffect;

        public void Update()
        {
            visualEffect.SetTexture("_LookupTexture", lookupTexture.Texture);
            visualEffect.SetInt("_LookupTextureIndex", lookupTexture.Index);
            visualEffect.SetInt("_LookupTextureLength", lookupTexture.Index);
        }
}
