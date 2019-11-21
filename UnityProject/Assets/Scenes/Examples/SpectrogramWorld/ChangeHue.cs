using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    interface IChangeHue
    {
        float Hue { set; }
    }

    public class ChangeHue : MonoBehaviour, IChangeHue
    {
        [SerializeField] Renderer thisRend_;

        private static readonly int spectrogramHue_ = Shader.PropertyToID("_Pitch");

        public float Hue
        {
            set
            {
                var x = value * 1f;
                thisRend_.material.SetFloat(spectrogramHue_, x);
            }
        }
    }
}