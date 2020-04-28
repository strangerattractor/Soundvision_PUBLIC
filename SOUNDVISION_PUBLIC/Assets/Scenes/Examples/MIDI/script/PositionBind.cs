using UnityEngine;

namespace cylvester
{
    public class PositionBind : MonoBehaviour
    {
        [SerializeField,Range(1,16)] private int channel = 1;
        
        public void OnMidiMessageReceived(MidiMessage mes)
        {
            if (mes.Status - 176 == channel - 1)
            {
                var x = (mes.Data1 - 64) / 10f;
                var y = (mes.Data2 - 64) / 10f;
                transform.position = new Vector3(x, y, 0f);
            }
        }
    }
}

