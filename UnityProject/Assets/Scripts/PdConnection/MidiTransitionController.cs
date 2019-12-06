using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace cylvester
{

    public enum CYL_Command
    {
        OneBarLoopButton = 86,
        FourBarLoopButton = 94,
        NextScelectedScene = 18,
        CurrentSelectedScene = 17,
        instaTrig = 2
    }

    public class MidiTransitionController : MonoBehaviour
    {

        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField, Range(1, 16)] private int channel = 1;
        [SerializeField] StateManager stateManager;

        private const int oneBarLoopLength = 96;
        private const int fourBarLoopLength = 384;

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

                        if (instaChangeActive)
                        {
                            stateManager.SelectedState = currentSelectedScene;
                            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(10);
                            instaChangeActive = false;
                        }
                    break;

                    case (byte) CYL_Command.FourBarLoopButton:
                        RestTime(fourBarLoopLength - currentTick % fourBarLoopLength);
                        TimelinePlaybackSpeed();
                        stateManager.SelectedState = nextSelectedScene;
                    break;

                    case (byte) CYL_Command.OneBarLoopButton:
                        RestTime(oneBarLoopLength - currentTick % oneBarLoopLength);
                        TimelinePlaybackSpeed();
                        stateManager.SelectedState = nextSelectedScene;
                    break;
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