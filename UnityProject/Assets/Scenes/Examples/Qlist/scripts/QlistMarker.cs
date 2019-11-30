using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace cylvester
{
    [CustomStyle("Annotation")]
    public class QlistMarker : Marker, INotification
    {
        [TextArea] public string stateName;

        public PropertyName id => stateName;
    }
}
