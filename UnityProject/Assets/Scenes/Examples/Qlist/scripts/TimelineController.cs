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

        [SerializeField] private float initTransitionFactor = 1f;
        
        private IList<QlistMarker> qlistMarkers_;
        
        public void Start()
        { 
            var timeline = (TimelineAsset)playableDirector.playableAsset;
            var markerTrack = timeline.markerTrack;
            var markers = markerTrack.GetMarkers();
            qlistMarkers_ = new List<QlistMarker>();
            foreach (var marker in markers)
                qlistMarkers_.Add((QlistMarker)marker);

            playableDirector.Stop();
            playableDirector.time = 0;
            playableDirector.Play();
        }
        
        public void OnStateChanged(IStateReader stateManager)
        {
            var stateName = stateManager.CurrentState.Title;
            foreach (var qlistMarker in qlistMarkers_)
            {
                if (qlistMarker.id != stateName) continue;

                //playableDirector.Stop(); //This resets speed to 1, so I had to cut it. 
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
            var prevState = stateManager.PreviousState.Value; //ToDO this is the same as nextState???

            Debug.Log("next State " + prevState.Title);
            Debug.Log("prev State " + prevState.Title);

            if (notification.id == nextState.Title || notification.id == prevState.Title)
            { 
                playableDirector.Pause(); // reaches the next state (marker) in timeline
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(initTransitionFactor); // Max added this instead of .Stop
            }
        }
    }
}
