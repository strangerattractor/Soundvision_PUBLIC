using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace cylvester
{ 
    public class MidiTransitionController : MonoBehaviour
    {
        [SerializeField] private int oneBarLoopButton = 86;
        [SerializeField] private int fourBarLoopButton = 94;
        [SerializeField] private PlayableDirector playableDirector;

        private int oneBarLoop = 96;
        private int fourBarLoop = 384;
        private int instaTrig = 2;
        private int nextSelectedSceneOutput = 18;
        private int lastSelectedSceneOutput = 17;
        private bool instaChangeActive;

        private int currentTick;
        private float transitionLength = 16; //sets the duration in Seconds, how long a transition has to be in "TimeLine" to be played back correctly when CYLVESTER is hooked up correctly
        private float restTimeS = 1f; //init transTime is 1 Second

        [SerializeField] private int lastSelectedScene = 0;
        [SerializeField] private int currentSelectedScene = 0;
        [SerializeField] private int nextSelectedScene;

        [SerializeField, Range(1, 16)] private int channel = 1;

        [SerializeField] StateManager stateManager;

        public void OnSyncReceived(MidiSync midiSync, int counter)
        {
            currentTick = counter;
        }

        public void OnMidiMessageReceived(MidiMessage mes)
        {

            if (mes.Status - 176 == channel - 1) //Which Channel
            {

                if (mes.Data1 == nextSelectedSceneOutput)
                {
                    nextSelectedScene = mes.Data2; //Get next Schene Update
                }

                if (mes.Data1 == lastSelectedSceneOutput)
                    lastSelectedScene = mes.Data2; //Get current selected Scene

                if (mes.Data1 == oneBarLoopButton) //Button fourBarLoop
                {
                    RestTime(fourBarLoop - currentTick % fourBarLoop);
                    TimelinePlaybackSpeed();
                    stateManager.SelectedState = nextSelectedScene;
                }

                if (mes.Data1 == fourBarLoopButton) //Button oneBarLoop
                {
                    RestTime(oneBarLoop - currentTick % oneBarLoop);
                    TimelinePlaybackSpeed();
                    stateManager.SelectedState = nextSelectedScene;
                }

                if (mes.Data1 == instaTrig)
                {
                    instaChangeActive = true;

                    if (instaChangeActive && (currentSelectedScene != lastSelectedScene))
                    {
                        currentSelectedScene = lastSelectedScene;
                        stateManager.SelectedState = lastSelectedScene;
                        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(10);
                        Debug.Log("Instatrig " + currentSelectedScene);
                        Debug.Log("Last selected Scene new " + lastSelectedScene);
                        instaChangeActive = false;
                    }
                    
                }
            }
        }

        public void RestTime(int restTick)
        {
            restTimeS = restTick / 24.0f / stateManager.CurrentState.Bpm * 60;
        }

        public void TimelinePlaybackSpeed()
        {
            float timelinePlaybackSpeed = transitionLength / Mathf.Clamp(restTimeS, 0.001f, transitionLength);
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(timelinePlaybackSpeed); //set playbackspeed of Timeline
        }
    }
}