using System;
using UnityEngine;

namespace cylvester
{
    public interface IPdBackend
    {
        bool State { set; }
    }
    
    [ExecuteInEditMode]
    public class PdBackend : MonoBehaviour, IPdBackend
    {
        [SerializeField] string mainPatch;
        [SerializeField] int inchannels = 2;

        private Action onToggled_;
        
        private void OnEnable()
        {
            PdProcess.Instance.Start(mainPatch, inchannels);
        }

        private void OnDisable()
        {
            PdProcess.Instance.Stop();
        }

        public bool State
        {
            set => enabled = value;
        }
    }
}