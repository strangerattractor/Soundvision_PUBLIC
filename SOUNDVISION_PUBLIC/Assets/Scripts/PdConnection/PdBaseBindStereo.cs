using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    public abstract class PdBaseBindStereo : MonoBehaviour
    {
        [SerializeField] protected PdBackend pdbackend;
        [SerializeField, Range(1, 16)] protected int channelLeft = 1;
        [SerializeField, Range(1, 16)] protected int channelRight = 2;

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