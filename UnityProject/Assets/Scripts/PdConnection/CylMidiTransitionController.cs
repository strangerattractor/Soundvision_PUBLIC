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
            NextSelectedTransition = 18,
            CurrentSelectedAnimation = 17,
            InstantTrigger = 2,
        }

        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private TimelineController timelineController;
        [SerializeField, Range(1, 16)] private int channel = 1;
        [SerializeField] private StateManager stateManager;
        [SerializeField] private float instaTransitionSpeed = 10;

        private const int OneBarTrigger = 96;
        private const int FourBarTrigger = OneBarTrigger * 4;
        private const float TransitionLength = 16;

        private ScheduledAction scheduledAction_;
        private int currentTick_;
        private int currentSelectedAnimation_;
        private int nextSelectedMarker_;

        private void Start()
        {
            scheduledAction_ = new ScheduledAction(() =>
            {
                stateManager.SelectedState = currentSelectedAnimation_;
                //playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(instaTransitionSpeed);
                timelineController.instantTrigger(instaTransitionSpeed);
            });

            currentSelectedAnimation_ = 0;
            nextSelectedMarker_ = 1;
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
                case CylCommand.NextSelectedTransition:

                    nextSelectedMarker_ = mes.Data2; //next marker only used for direction
                    Debug.Log("command nextSelectedMarker_=" + nextSelectedMarker_);
                    break;

                case CylCommand.InstantTrigger:

                    scheduledAction_.Ready();
                    break;

                case CylCommand.CurrentSelectedAnimation:

                    currentSelectedAnimation_ = mes.Data2;
                    Debug.Log("command currentSelectedAnimation_=" + currentSelectedAnimation_);
                    scheduledAction_.Go();
                    break;

                case CylCommand.FourBarLoopButton:
                case CylCommand.OneBarLoopButton:
                    float restTime=0;
                    switch(command)
                    {
                        case CylCommand.FourBarLoopButton:
                            restTime = CalculateRestTime(FourBarTrigger - currentTick_ % FourBarTrigger);
                            break;
                        case CylCommand.OneBarLoopButton:
                            restTime = CalculateRestTime(OneBarTrigger - currentTick_ % OneBarTrigger);
                            break;
                    }
                    Debug.Log("currentSelectedAnimation_=" + currentSelectedAnimation_);
                    Debug.Log("nextSelectedScene_=" + nextSelectedMarker_);
                    bool _reverse = nextSelectedMarker_ <= currentSelectedAnimation_; //nextSelectedMarker_ used to calculate direction

                    if (timelineController.animationPaused())
                    {
                        timelineController.UpdateTransitionTargetRealTime(restTime, _reverse);

                        stateManager.SelectedState = currentSelectedAnimation_ + 1; //marker at which transition starts
                    }
                    else //trigger pressed while animation running
                    {
                        timelineController.abortAnimation();
                        
                    }
                    break;

                default:
                    Debug.Log("Unexpected Command: " + mes.Data1);
                    throw new Exception("Unexpected CYL command");
            }
        }

        private float CalculateRestTime(int restTicks)
        {
            return restTicks / 24f / stateManager.CurrentState.Bpm * 60f;
        }

    }
}