using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{ 
    public class MidiTransitionController : MonoBehaviour
    {
        [SerializeField] private int oneBarLoopButton = 86;
        [SerializeField] private int fourBarLoopButton = 94;

        private int oneBarLoop = 96;
        private int fourBarLoop = 384;
        private int currentTick;
        private float transitionLength = 16; //sets the duration in Seconds, how long a transition has to be in "TimeLine" to be played back correctly when CYLVESTER is hooked up correctly
        private float restTimeS = 1f; //init transTime is 1 Second

        [SerializeField, Range(1, 16)] private int channel = 1;

        [SerializeField] StateManager stateManager;

        public void Start()
        {
            
        }

        public void OnSyncReceived(MidiSync midiSync, int counter)
        {
            currentTick = counter;
        }

        public void OnMidiMessageReceived(MidiMessage mes)
        {
            if (mes.Status - 176 == channel - 1) //Which Channel
            {
                if (mes.Data1 == oneBarLoopButton) //Button fourBarLoop
                {
                    RestTime(fourBarLoop - currentTick % fourBarLoop);
                }

                if (mes.Data1 == fourBarLoopButton) //Button oneBarLoop
                {
                    RestTime(oneBarLoop - currentTick % oneBarLoop);
                }
            }
        }

        public void RestTime(int restTick)
        {
            restTimeS = restTick / 24.0f / stateManager.CurrentState.Bpm * 60;
        }

        public float TimelinePlaybackSpeed ()
        {
            float timelinePlaybackSpeed;
            /*           if (restTimeS == 0f)
                       {
                           restTimeS = initTransitionTime; //Initial Transition Time
                       }
           */
            timelinePlaybackSpeed = transitionLength / Mathf.Clamp(restTimeS, 0.001f, transitionLength);
            return timelinePlaybackSpeed;
        }
    }
}