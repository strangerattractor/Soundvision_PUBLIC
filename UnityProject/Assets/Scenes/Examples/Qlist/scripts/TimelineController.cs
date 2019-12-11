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
        private float restTime_;

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
                var previousMarkerTime = i > 0 ? (double?) qlistMarkers_[i - 1].time : null;
                var nextMarkerTime = i < numMarkers - 1 ? (double?) qlistMarkers_[i + 1].time : null;
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

            if (!(Time.fixedUnscaledTime >= transitionTargetRealtime_)) // check o current time >= targetTime
            {
                speed_ = (CalculateTransitionSpeed(nextMarkerTime_));// berechne tnrsitionSpeed

                Debug.Log("Rest time: " + restTime_);
                Debug.Log("Speed set to: " + speed_);
            }


            var deltaTime = Time.deltaTime;
            var expectedTimeIncrement = speed_ * deltaTime;
            var expectedTimeInTimeline = playableDirector.time + expectedTimeIncrement;

            if (boundary_.IsInside(expectedTimeInTimeline))
            {
                playableDirector.time = expectedTimeInTimeline;
                playableDirector.Evaluate();
            }
            else
            {
                playableDirector.Pause();
                ResetSpeed();
                Debug.Log("Speed Reset to " + speed_);
            }
        }

        private void ResetSpeed()
        {
            speed_ = initTransitionFactor;
        }


        public void UpdateTransitionTargetRealTime(float restTime)
        {
            transitionTargetRealtime_ = Time.fixedUnscaledTime + restTime;
            restTime_ = restTime;
        }

        private float CalculateTransitionSpeed(float targetMarkerTime)
        {
            var transitionSpeed = (targetMarkerTime - playableDirector.time) / (transitionTargetRealtime_ - Time.fixedUnscaledTime);
            return (float) transitionSpeed;
        }
    }
}
