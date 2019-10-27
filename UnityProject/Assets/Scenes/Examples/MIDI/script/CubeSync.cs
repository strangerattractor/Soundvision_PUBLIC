using UnityEngine;

public class CubeSync : MonoBehaviour
{
    private int counter_ = 0;
    private float currentX_;
    private float targetX_;
    private float lastCallBack_;
    private float callbackInterval_ = 0.05f;

    public void onClockReceived()
    {
        var now = Time.realtimeSinceStartup;
        callbackInterval_ = now - lastCallBack_;
        lastCallBack_ = now;
        currentX_ =  (counter_ - 12) * 0.2f;
        counter_++;
        targetX_ =  (counter_ - 12) * 0.2f;
        counter_ %= 24;
        
    }

    public void Update()
    {
        var timeSinceLastCallback = Time.realtimeSinceStartup - lastCallBack_;
        var elapsedRatio = timeSinceLastCallback / callbackInterval_;
        var animationX = Mathf.Lerp(currentX_, targetX_, elapsedRatio);
        Debug.Log(elapsedRatio);
        transform.position = new Vector3(animationX, 0f, 0f);
    }
}
