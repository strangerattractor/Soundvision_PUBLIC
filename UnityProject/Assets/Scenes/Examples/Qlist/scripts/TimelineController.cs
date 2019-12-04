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
        [SerializeField] private MidiTransitionController transitionController;

        [SerializeField] private float initTransitionTime = 16f;
        
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

                if (transitionController != null)
                { 
                    playableDirector.playableGraph.GetRootPlayable(0)
                        .SetSpeed(transitionController.TimelinePlaybackSpeed()); //Set Playback Speed of Timeline for CYL transitions
                }
                else
                playableDirector.playableGraph.GetRootPlayable(0)
                        .SetSpeed(initTransitionTime);
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
