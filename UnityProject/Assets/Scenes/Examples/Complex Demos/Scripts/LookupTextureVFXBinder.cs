using UnityEngine;
using UnityEngine.Experimental.VFX;

public class LookupTextureVFXBinder : MonoBehaviour
{
        [SerializeField] private LookupTexture lookupTexture;
        [SerializeField] private VisualEffect visualEffect;

        public void Update()
        {
            visualEffect.SetTexture("_LookupTexture", lookupTexture.Texture);
            visualEffect.SetInt("_LookupTextureIndex", lookupTexture.Index);
            visualEffect.SetInt("_LookupTextureLength", lookupTexture.Index);
        }
}
