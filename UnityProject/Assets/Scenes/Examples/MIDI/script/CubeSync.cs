using cylvester;
using UnityEngine;

public class CubeSync : MonoBehaviour
{
    private float currentX_;
    private float targetX_;
    private float lastCallBack_;
    private float callbackInterval_ = 0.05f;

    public void OnSyncReceived(MidiSync midiSync, int count)
    {
        if (midiSync == MidiSync.Clock)
        {
            var now = Time.realtimeSinceStartup;
            callbackInterval_ = now - lastCallBack_;
            lastCallBack_ = now;
            currentX_ = (count % 24 - 12) * 0.2f;
            targetX_ = (count % 24 - 12) * 0.2f;
        }
    }

    public void Update()
    {
        var timeSinceLastCallback = Time.realtimeSinceStartup - lastCallBack_;
        var elapsedRatio = timeSinceLastCallback / callbackInterval_;
        var animationX = Mathf.Lerp(currentX_, targetX_, elapsedRatio);
        transform.position = new Vector3(animationX, 0f, 0f);
    }
}
