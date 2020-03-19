using UnityEngine;
using UnityEngine.VFX;

namespace cylvester
{ 
    public class VFXKeijiroBinder : MonoBehaviour
    {
        [SerializeField] private VisualEffect targetVFX_ = null;
        [SerializeField] private string valueName_ = "Value_1";
        private float _val;

        public float val
        {
            get => _val;
            set => _val = value;
        }

        void Update()
        {
            targetVFX_.SetFloat(valueName_, _val);
        }
    }
}
