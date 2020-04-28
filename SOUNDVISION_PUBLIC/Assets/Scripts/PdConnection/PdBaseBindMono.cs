using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    public abstract class PdBaseBindMono : MonoBehaviour
    {
        [SerializeField] protected PdBackend pdbackend;
        [SerializeField, Range(1, 16)] protected int channel = 1;

        protected void Start()
        {
            if (pdbackend == null)
            {
                var pdBackendObjects = FindObjectsOfType<PdBackend>();
                if (pdBackendObjects.Length > 0)
                {
                    var g = pdBackendObjects[0].gameObject;
                    pdbackend = g.GetComponent<PdBackend>();
                }
            }
        }
    }
}