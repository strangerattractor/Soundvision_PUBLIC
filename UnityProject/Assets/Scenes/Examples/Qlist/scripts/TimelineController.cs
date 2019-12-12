using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace cylvester
{
    public class TimelineController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private StateManager stateManager;

        [SerializeField] private float initTransitionFactor = 1f;
        
        private IList<QlistMarker> qlistMarkers_;
        private Boundary boundary_;
        private float speed_;
        private float transitionTargetRealtime_;
        private float previousMarkerTime_;
        private float nextMarkerTime_;
        private bool transitionDirectionReverse_;

        public void Start()
        { 
            var timeline = (TimelineAsset)playableDirector.playableAsset;
            var markers = timeline.markerTrack.GetMarkers();
            qlistMarkers_ = new List<QlistMarker>();
            foreach (var marker in markers)
                qlistMarkers_.Add((QlistMarker)marker);

            playableDirector.time = 0;
            boundary_ = new Boundary(null, null);
            ResetSpeed();
        }
        
        public void OnStateChanged(IStateReader stateManager)
        {
            var stateName = stateManager.CurrentState.Title;
            var numMarkers = qlistMarkers_.Count;
            for (var i = 0; i < numMarkers; ++i)
            {
                if (qlistMarkers_[i].id != stateName) 
                    continue;
                
                playableDirector.time = qlistMarkers_[i].time;
                var previousMarkerTime = i > 0 ? (double?) qlistMarkers_[i - 1].time : 0;
                var nextMarkerTime = i < numMarkers - 1 ? (double?) qlistMarkers_[i + 1].time : qlistMarkers_[numMarkers - 1].time;
                previousMarkerTime_ = (float) previousMarkerTime;
                nextMarkerTime_ = (float) nextMarkerTime;
                boundary_ = new Boundary(previousMarkerTime, nextMarkerTime);
                playableDirector.Play();
                break;
            }
        }

        private void Update()
        {
            if (playableDirector.state == PlayState.Paused)
                return;

            if (!(Time.fixedUnscaledTime >= transitionTargetRealtime_)) // check if Transition has not finished yet
            {
                if (!transitionDirectionReverse_) // if transition direction forward, speed becomes positive
                {
                    speed_ = CalculateTransitionSpeed(nextMarkerTime_);
                }

                else // if transition direction backward, speed becomes negative
                {
                    speed_ = CalculateTransitionSpeed(previousMarkerTime_);
                }
            }

            var deltaTime = Time.deltaTime;
            var expectedTimeIncrement = speed_ * deltaTime;
            var expectedTimeInTimeline = playableDirector.time + expectedTimeIncrement;

            if (boundary_.IsInside(expectedTimeInTimeline)) // check if we are in between next and previous marker
            {
                playableDirector.time = expectedTimeInTimeline;
                playableDirector.Evaluate();
            }
            else
            {
                //sets playhead precisly to marker position at the end of transition. 
                if (!transitionDirectionReverse_) 
                {
                    playableDirector.time = nextMarkerTime_;
                }

                else
                {
                    playableDirector.time = previousMarkerTime_;
                }

                playableDirector.Pause();
                ResetSpeed();
            }
        }

        private void ResetSpeed()
        {
            speed_ = initTransitionFactor;
        }


        public void UpdateTransitionTargetRealTime(float restTime, bool reverse)
        {
            transitionDirectionReverse_ = reverse;
            transitionTargetRealtime_ = Time.fixedUnscaledTime + restTime;
        }

        private float CalculateTransitionSpeed(float targetMarkerTime)
        {
            var transitionSpeed = (targetMarkerTime - playableDirector.time) / (transitionTargetRealtime_ - Time.fixedUnscaledTime);
            return (float) transitionSpeed;
        }
    }
}
/*
            if (reverse_ == true)
            {
                speed_ = speed_* -1;
            }
            */