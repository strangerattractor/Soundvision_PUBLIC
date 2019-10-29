using System;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable]
    public class UnityStateEvent : UnityEvent<string>
    {}
    
    public interface IStateManager
    {
        int State { set; }
        string[] StateTitles { get; }
        void OnMidiReceived(MidiMessage message);
    }
    
    public class StateManager : MonoBehaviour, IStateManager
    {
        private enum Operation
        {
            Rewind = 0,
            Previous = 1,
            Next = 2
        }
        
        [SerializeField] private string csvFileName = "qlist";
        [SerializeField] private UnityStateEvent onStateChanged;
        [SerializeField] private int sceneSelection;

        void Start()
        { 
            var content = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/" + csvFileName + ".csv");
            var lines  = content.Split("\n"[0]);
            StateTitles = new string[lines.Length - 1];
            for (var i = 1; i < lines.Length; ++i)
            {
                var columns = (lines[i].Trim()).Split(","[0]);
                StateTitles[i-1] = columns[0];
            }
        }
        public string[] StateTitles { get; private set; }

        public int State
        {
            set
            {
                sceneSelection = value;
                onStateChanged.Invoke(StateTitles[sceneSelection]);
            }
        }

        public void OnMidiReceived(MidiMessage message)
        {
            if (message.Status != 176 || message.Data1 != 127) return;
            
            switch ((Operation)message.Data2)
            {
                case Operation.Rewind:
                    if (sceneSelection == 0) return;
                    sceneSelection = 0;
                    break;
                case Operation.Previous:
                    if (sceneSelection == 0) return;
                    sceneSelection--;                        
                    break;
                case Operation.Next:
                    if (sceneSelection >= StateTitles.Length - 1) return;
                    sceneSelection++;
                    break;
                default:
                    return;
            }
            onStateChanged.Invoke(StateTitles[sceneSelection]);
        }
    }

}


