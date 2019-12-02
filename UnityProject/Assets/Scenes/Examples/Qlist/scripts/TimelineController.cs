using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace cylvester
{
    public class TimelineController : MonoBehaviour, INotificationReceiver
    {
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private StateManager stateManager;
        
        private IList<QlistMarker> qlistMarkers_;
        
        public void Start()
        { 
            var timeline = (TimelineAsset)playableDirector.playableAsset;
            var markerTrack = timeline.markerTrack;
            var markers = markerTrack.GetMarkers();
            qlistMarkers_ = new List<QlistMarker>();
            foreach (var marker in markers)
                qlistMarkers_.Add((QlistMarker)marker);
        }
        
        public void OnStateChanged(IStateReader stateManager)
        {
            var stateName = stateManager.CurrentState.Title;
            foreach (var qlistMarker in qlistMarkers_)
            {
                if (qlistMarker.id != stateName) continue;
                playableDirector.Stop();
                playableDirector.time = qlistMarker.time;
                playableDirector.Play();
                break;
            }
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (!stateManager.NextState.HasValue)
                return;

            var nextState = stateManager.NextState.Value;
            if(notification.id == nextState.Title)
                playableDirector.Pause(); // reaches the next state (marker) in timeline
        }
    }
}
