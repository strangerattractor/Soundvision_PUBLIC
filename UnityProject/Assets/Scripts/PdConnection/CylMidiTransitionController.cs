using System;
using UnityEngine;
using UnityEngine.Playables;

namespace cylvester
{
    public class CylMidiTransitionController : MonoBehaviour
    {
        private enum CylCommand
        { 
            OneBarLoopButton = 94,
            FourBarLoopButton = 86,
            NextSelectedScene = 18,
            CurrentSelectedScene = 17,
            InstantTrigger = 2,
        }
        
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField, Range(1, 16)] private int channel = 1;
        [SerializeField] private StateManager stateManager;
        [SerializeField] private float instaTransitionSpeed = 10;

        private const int OneBarTrigger = 96;
        private const int FourBarTrigger = OneBarTrigger * 4;
        private const float TransitionLength = 16; 
        
        private ScheduledAction scheduledAction_;
        private int currentTick_;
        private int currentSelectedScene_ ;
        private int nextSelectedScene_;

        private void Start()
        {
            scheduledAction_ = new ScheduledAction(() =>
            {
                stateManager.SelectedState = currentSelectedScene_;
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(instaTransitionSpeed);
            });
        }
        
        public void OnSyncReceived(MidiSync midiSync, int counter)
        {
            currentTick_ = counter;
        }

        public void OnMidiMessageReceived(MidiMessage mes)
        {
            if (mes.Status - 176 != channel - 1) return;

            var command = (CylCommand)mes.Data1;
            switch (command)
            {
                case CylCommand.NextSelectedScene:
                {
                    nextSelectedScene_ = mes.Data2;
                    break;
                }
                case CylCommand.InstantTrigger:
                {
                    scheduledAction_.Ready();
                    break;
                }
                case CylCommand.CurrentSelectedScene:
                {
                    currentSelectedScene_ = mes.Data2;
                    scheduledAction_.Go();
                    break;
                }
                case CylCommand.FourBarLoopButton:
                {
                    var restTime = UpdateRestTime(FourBarTrigger - currentTick_ % FourBarTrigger);
                    if (nextSelectedScene_ > currentSelectedScene_)
                    {
                        UpdateTimelinePlaybackSpeed(1f, restTime);
                        stateManager.SelectedState = nextSelectedScene_;
                    }
                    else
                    {
                        UpdateTimelinePlaybackSpeed(-1f, restTime);
                        stateManager.SelectedState = nextSelectedScene_ + 2;
                    }

                    break;
                }
                case CylCommand.OneBarLoopButton:
                {
                    var restTime = UpdateRestTime(OneBarTrigger - currentTick_ % OneBarTrigger);
                    UpdateTimelinePlaybackSpeed(1f, restTime);
                    stateManager.SelectedState = nextSelectedScene_;
                    break;
                }
                default:
                    throw new Exception("Unexpected CYL command");
                    
            }
        }

        private float UpdateRestTime(int restTicks)
        {
            return restTicks / 24f / stateManager.CurrentState.Bpm * 60f;
        }

        private void UpdateTimelinePlaybackSpeed(float speed, float restTime)
        {
            var timelinePlaybackSpeed = TransitionLength / Mathf.Clamp(restTime, 0.001f, TransitionLength);
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(timelinePlaybackSpeed * speed);
        }
    }
}