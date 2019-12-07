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
        
        public void Start()
        { 
            var timeline = (TimelineAsset)playableDirector.playableAsset;
            var markers = timeline.markerTrack.GetMarkers();
            qlistMarkers_ = new List<QlistMarker>();
            foreach (var marker in markers)
                qlistMarkers_.Add((QlistMarker)marker);

            playableDirector.time = 0;
            boundary_ = new Boundary(null, null);
            UpdateSpeed();
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
                boundary_ = new Boundary(previousMarkerTime, nextMarkerTime);
                playableDirector.Play();
                break;
            }
        }

        private void Update()
        {
            if (playableDirector.state == PlayState.Paused)
                return;

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
                UpdateSpeed();
            }
        }

        private void UpdateSpeed()
        {
            speed_ = initTransitionFactor;
        }
    }
}
