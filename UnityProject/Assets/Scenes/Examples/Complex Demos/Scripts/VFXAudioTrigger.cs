using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class VFXAudioTrigger : MonoBehaviour
{

    [SerializeField] private VisualEffect vfx;
    [SerializeField] private string eventName;

    public void OnThresholdExceeded()
    {
        vfx.SendEvent(eventName);
    }

}
