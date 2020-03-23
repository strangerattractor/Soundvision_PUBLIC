using System;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{

    public interface IStateReader
    {
        State? PreviousState { get; }
        State CurrentState { get; }
        State? NextState { get; }
    }
    
    public interface IStateManager : IStateReader
    {
        int SelectedState { set; }
        State[] States { get; }
    }

    public struct State
    {
        public string Title;
        public string Note;
        public int Bpm;

        public float Speed => Bpm / 60f; // 60 BPM as speed 1f
    }
    
    [Serializable]
    public class UnityStateEvent : UnityEvent<IStateReader>
    {}
    
    public class StateManager : MonoBehaviour, IStateManager
    {
        string[] titles_;
        public enum Operation
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
            Debug.Log(Application.streamingAssetsPath + "/" + csvFileName + ".csv");
            Debug.Log(content);
            var lines  = content.Split('\n');
            
            var numberOfEntries = lines.Length - 1;
            States = new State[numberOfEntries];
            for (var i = 0; i < numberOfEntries; ++i)
            {
                var columns = (lines[i+1].Trim()).Split(',');
                
                States[i].Title = columns[0];

                try
                {
                    States[i].Bpm = int.Parse(columns[1]);
                }
                catch (FormatException)
                {
                    States[i].Bpm = 60; // gracefully fail
                }

                States[i].Note = columns[2];
            }
        }
        
        public State[] States { get; private set; }

        public State CurrentState => States[sceneSelection];

        public State? PreviousState => sceneSelection == 0 ? (State?) null : States[sceneSelection-1];

        public State? NextState => sceneSelection == States.Length - 1 ? (State?) null : States[sceneSelection + 1];

        public int SelectedState
        {
            set
            {
                sceneSelection = value;
                if (value > States.Length - 1)
                { //if index out of bounds
                    sceneSelection = States.Length - 1; //set to last index
                }
                else if (value < 0)
                {
                    sceneSelection = 0;
                }
                onStateChanged.Invoke(this);
            }
            get
            { return sceneSelection; }
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
                    sceneSelection = Math.Max(sceneSelection, 0); //constrain minimum
                    break;
                case Operation.Next:
                    if (sceneSelection >= States.Length - 1) return;
                    sceneSelection++;
                    sceneSelection = Math.Min(sceneSelection, States.Length - 1); //constrain maximum
                    break;
                default:
                    return;
            }
            onStateChanged.Invoke(this);
        }

        public void OnStateChanged(Operation operation)
        {

            switch (operation)
            {
                case Operation.Rewind:
                    if (sceneSelection == 0) return;
                    sceneSelection = 0;
                    break;
                case Operation.Previous:
                    if (sceneSelection == 0) return;
                    sceneSelection--;
                    sceneSelection = Math.Max(sceneSelection, 0); //constrain minimum
                    break;
                case Operation.Next:
                    if (sceneSelection >= States.Length - 1) return;
                    sceneSelection++;
                    sceneSelection = Math.Min(sceneSelection, States.Length - 1); //constrain maximum
                    break;
                default:
                    return;
            }
            onStateChanged.Invoke(this);
        }

        public string[] GetStateInfos()
            //Gets all infos from the selected row (Title, Note, BPM)
        {
            if (titles_ != null && titles_.Length > 0 )
                return titles_;

            titles_ = new string[States.Length];

            for (var i = 0; i < States.Length; ++i)
                if (States[i].Title != "")
                    titles_[i] = States[i].Title + " || " + States[i].Note + " || BPM: " + States[i].Bpm;
                else
                    titles_[i] = "";

            return titles_;
        }
    }

}