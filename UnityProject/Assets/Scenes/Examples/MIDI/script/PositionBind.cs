using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cylvester
{
    public class PositionBind : MonoBehaviour
    {
        [SerializeField,Range(1,16)] private int channel = 1;
        
        public void OnControlMessageReceived(ControlMessage mes)
        {
            if (mes.Channel == channel - 1)
            {
                var x = (mes.ControlNumber - 64) / 10f;
                var y = (mes.ControlValue - 64) / 10f;
                transform.position = new Vector3(x, y, 0f);
            }
        }
    }
}

