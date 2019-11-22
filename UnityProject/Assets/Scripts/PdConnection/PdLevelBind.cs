using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [System.Serializable]
    public class LevelEvent : UnityEvent<float>
    {
    }
    
    public class PdLevelBind : MonoBehaviour
    {
        [SerializeField] private PdBackend pdbackend;
        [SerializeField, Range(1, 16)] private int channel = 1;
        [SerializeField] private LevelEvent levelChanged;
        [SerializeField] bool logLevel;
        private float level_;
        
        void Update()
        {
            var level = pdbackend.LevelArray.Data[channel - 1];

            if (level_ != level)
            {
                level_ = level;
                levelChanged.Invoke(level_);

                if (logLevel)
                { 
                Debug.Log("Level:" + level_);
                }
            }
        }
    }
}