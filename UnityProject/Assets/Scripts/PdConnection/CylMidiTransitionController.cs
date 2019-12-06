using System;
using UnityEngine;
using UnityEngine.Playables;

namespace cylvester
{
    public enum CYLCommand
    { 
        OneBarLoopButton = 94,
        FourBarLoopButton = 86,
        NextSelectedScene = 18,
        CurrentSelectedScene = 17,
        InstantTrigger = 2,
    }

    public class CylMidiTransitionController : MonoBehaviour
    {

        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField, Range(1, 16)] private int channel = 1;
        [SerializeField] private StateManager stateManager;
        [SerializeField] private float instaTransitionSpeed = 10;

        private const int OneBarTrigger = 96;
        private const int FourBarTrigger = OneBarTrigger * 4;
        private const float TransitionLength = 16; 
        
        private bool instantChangeActive_;
        private int currentTick_;
        private float restTime_ = 1f;
        private int currentSelectedScene_ ;
        private int nextSelectedScene_;

        public void OnSyncReceived(MidiSync midiSync, int counter)
        {
            currentTick_ = counter;
        }

        public void OnMidiMessageReceived(MidiMessage mes)
        {
            if (mes.Status - 176 != channel - 1) return;

            var command = (CYLCommand)mes.Data1;
            switch (command)
            {
                case CYLCommand.NextSelectedScene:
                {
                    nextSelectedScene_ = mes.Data2;
                    break;
                }
                case CYLCommand.InstantTrigger:
                {
                    instantChangeActive_ = true;
                    break;
                }
                case CYLCommand.CurrentSelectedScene:
                {
                    currentSelectedScene_ = mes.Data2;

                    if (instantChangeActive_)
                    {
                        stateManager.SelectedState = currentSelectedScene_;
                        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(instaTransitionSpeed);
                        instantChangeActive_ = false;
                    }

                    break;
                }
                case CYLCommand.FourBarLoopButton:
                {
                    if (nextSelectedScene_ > currentSelectedScene_)
                    {
                        UpdateRestTime(FourBarTrigger - currentTick_ % FourBarTrigger);
                        UpdateTimelinePlaybackSpeed(1f);
                        stateManager.SelectedState = nextSelectedScene_;
                    }
                    else
                    {
                        UpdateRestTime(FourBarTrigger - currentTick_ % FourBarTrigger);
                        UpdateTimelinePlaybackSpeed(-1f);
                        stateManager.SelectedState = nextSelectedScene_ + 2;
                    }

                    break;
                }
                case CYLCommand.OneBarLoopButton:
                {
                    UpdateRestTime(OneBarTrigger - currentTick_ % OneBarTrigger);
                    UpdateTimelinePlaybackSpeed(1f);
                    stateManager.SelectedState = nextSelectedScene_;
                    break;
                }
                default:
                    throw new Exception("Unexpected CYL command");
                    
            }
        }

        private void UpdateRestTime(int restTicks)
        {
            restTime_ = restTicks / 24.0f / stateManager.CurrentState.Bpm * 60f;
        }

        private void UpdateTimelinePlaybackSpeed(float speed)
        {
            var timelinePlaybackSpeed = TransitionLength / Mathf.Clamp(restTime_, 0.001f, TransitionLength);
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(timelinePlaybackSpeed * speed); //set playbackspeed of Timeline
        }
    }
}