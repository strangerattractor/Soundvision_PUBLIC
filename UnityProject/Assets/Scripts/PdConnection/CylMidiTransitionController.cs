using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace cylvester
{
    
    public enum CYL_Command
    { 
        //these are the midi CCs coming from the CYL_Axoloti box
        OneBarLoopButton = 94,
        FourBarLoopButton = 86,
        NextScelectedScene = 18,
        CurrentSelectedScene = 17,
        instaTrig = 2,
    }

    public enum Timeline_Command
    {
        Forwards = 1,
        Backwards = -1
    }

    public class CylMidiTransitionController : MonoBehaviour
    {

        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField, Range(1, 16)] private int channel = 1;
        [SerializeField] StateManager stateManager;
        [SerializeField] float instaTransitionSpeed = 10;

        private const int oneBarTrigger = 96;
        private const int fourBarTrigger = 384;

        private bool instaChangeActive;

        private int currentTick;
        private float transitionLength = 16; //sets the duration in Seconds, how long a transition has to be in "TimeLine" to be played back correctly when CYLVESTER is hooked up correctly
        private float restTimeS = 1f; //init transTime is 1 Second

        int currentSelectedScene = 0;
        int nextSelectedScene = 0;

        public void OnSyncReceived(MidiSync midiSync, int counter)
        {
            currentTick = counter;
        }

        public void OnMidiMessageReceived(MidiMessage mes)
        {

            if (mes.Status - 176 == channel - 1) //Choose Midi-Channel
            {

                switch (mes.Data1)
                {
                    case (byte) CYL_Command.NextScelectedScene:
                        nextSelectedScene = mes.Data2; //Get next selected Scene
                    break;

                    case (byte) CYL_Command.instaTrig:
                        instaChangeActive = true;
                    break;

                    case (byte) CYL_Command.CurrentSelectedScene:
                    currentSelectedScene = mes.Data2; //Get current selected Scene

                        if (instaChangeActive) //This triggers instant Switch between states
                        {
                            stateManager.SelectedState = currentSelectedScene;
                            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(instaTransitionSpeed);
                            instaChangeActive = false;
                        }
                    break;

                    case (byte) CYL_Command.FourBarLoopButton:
                        if (nextSelectedScene > currentSelectedScene)
                        { 
                        RestTime(fourBarTrigger - currentTick % fourBarTrigger);
                        TimelinePlaybackSpeed((int) Timeline_Command.Forwards);
                        stateManager.SelectedState = nextSelectedScene;
                        }
                        else
                        {
                            RestTime(fourBarTrigger - currentTick % fourBarTrigger);
                            TimelinePlaybackSpeed((int)Timeline_Command.Backwards);
                            stateManager.SelectedState = nextSelectedScene + 2;
                        }
                        break;

                    case (byte) CYL_Command.OneBarLoopButton:
                        RestTime(oneBarTrigger - currentTick % oneBarTrigger);
                        TimelinePlaybackSpeed((int) Timeline_Command.Forwards);
                        stateManager.SelectedState = nextSelectedScene;
                    break;
            }
        }
        }

        public void RestTime(int restTick)
        {
            restTimeS = restTick / 24.0f / stateManager.CurrentState.Bpm * 60;
        }

        public void TimelinePlaybackSpeed(int direction)
        {
            float timelinePlaybackSpeed = transitionLength / Mathf.Clamp(restTimeS, 0.001f, transitionLength);
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(timelinePlaybackSpeed * direction); //set playbackspeed of Timeline
        }
    }
}