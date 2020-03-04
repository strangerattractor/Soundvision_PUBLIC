using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace cylvester
{ 
    public class VFXValueBinder : MonoBehaviour
    {
        [SerializeField] private VisualEffect targetVFX_ = null;
        [SerializeField] private string valueName_ = "Value_1";

        public void OnEnergyChanged(float value)
        {
            targetVFX_.SetFloat(valueName_, value * .1f);
        }
    }
}
