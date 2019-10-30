using System;
using UnityEngine;

namespace cylvester
{
    public class Sequencer : MonoBehaviour
    {
        [SerializeField] private bool[] sequence = new bool[16];

        private void OnValidate()
        {
            if(sequence.Length != 16)
                Array.Resize(ref sequence, 16);
        }
    }
}
