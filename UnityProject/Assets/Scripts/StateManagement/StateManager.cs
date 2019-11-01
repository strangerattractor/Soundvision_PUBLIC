using System;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable]
    public class UnityStateEvent : UnityEvent<IStateManager>
    {}
    
    public interface IStateManager
    {
        int SelectedState { set; }
        
        State[] States { get; }
        string CurrentState { get; }
        string PreviousState { get; }
        string NextState { get; }

        void OnMidiReceived(MidiMessage message);
    }

    public struct State
    {
        public string Title;
        public string Note;
        public int BPM;

        private float Speed => BPM / 60f; // 60 BPM as speed 1f
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
            var lines  = content.Split('\n');
            
            var numberOfEntries = lines.Length - 1;
            States = new State[numberOfEntries];
            for (var i = 0; i < numberOfEntries; ++i)
            {
                var columns = (lines[i+1].Trim()).Split(',');
                
                States[i].Title = columns[0];

                try
                {
                    States[i].BPM = int.Parse(columns[1]);
                }
                catch (FormatException)
                {
                    States[i].BPM = 60; // gracefully fail
                }

                States[i].Note = columns[2];
            }
        }
        
        public State[] States { get; private set; }

        public string CurrentState => States[sceneSelection].Title;

        public string PreviousState => sceneSelection == 0 ? "---" : States[sceneSelection-1].Title;

        public string NextState => sceneSelection == States.Length - 1 ? "---" : States[sceneSelection + 1].Title;

        public int SelectedState
        {
            set
            {
                sceneSelection = value;
                onStateChanged.Invoke(this);
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
                    if (sceneSelection >= States.Length - 1) return;
                    sceneSelection++;
                    break;
                default:
                    return;
            }
            onStateChanged.Invoke(this);
        }
    }

}


