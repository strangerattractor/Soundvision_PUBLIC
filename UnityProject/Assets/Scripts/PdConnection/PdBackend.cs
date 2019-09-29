using System;
using System.Threading;
using UnityEngine;

namespace cylvester
{
    public interface IPdBackend
    {
        bool State { set; get; }
        event Action StateChanged;
    }
    
    [ExecuteInEditMode]
    public class PdBackend : MonoBehaviour, IPdBackend
    {
        [SerializeField] string mainPatch = "";
        [SerializeField] int inchannels = 2;

        private Action onToggled_;
        
        private void OnEnable()
        {
            PdProcess.Instance.Start(mainPatch, inchannels);
            Thread.Sleep(500);
        }

        private void OnDisable()
        {
            PdProcess.Instance.Stop();
        }

        public bool State
        {
            set
            {
                enabled = value;
                if(StateChanged != null)
                    StateChanged.Invoke();
            }
            get => enabled;
        }

        public event Action StateChanged;
    }
}