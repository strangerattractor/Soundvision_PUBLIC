using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnitySmoothEvent : UnityEvent<float>
{}


public class Smoother : MonoBehaviour
{
    [SerializeField] private float threshold;
    [SerializeField] private float attackTime;
    [SerializeField] private float releaseTime;
    [SerializeField] private float holdTime;
    [SerializeField] private UnitySmoothEvent onSmoothProcessed;
    
    enum State
    {
        StandBy,
        Attack,
        Hold,
        Release
    };
    
    private State state_ = State.StandBy;
    private float attackStart_ = 0f;
    private float holdStart_ = 0f;
    private float releaseStart_ = 0f;
    private float attackValue_ = 0f;
    private bool releaseReady_ = false;

    public void OnValueChanged(float value)
    {
        if (CanStartAttackPhase(value))
        {
            state_ = State.Attack;
            Debug.Log("Attack");
            attackStart_ = Time.time;
            attackValue_ = value;
        }
        else if (CanStartReleasePhase(value))
        {
            state_ = State.Release;
            Debug.Log("Release");
            releaseStart_ = Time.time;
            releaseReady_ = false;
        }
    }
    
    private void Update()
    {

        var now = Time.time;
        switch (state_)
        {
            case State.Attack:
            {
                var timeSinceAttackStart =  now - attackStart_;
                onSmoothProcessed.Invoke(attackValue_ * (timeSinceAttackStart / attackTime));

                if (timeSinceAttackStart >= attackTime)
                {
                    state_ = State.Hold;
                    Debug.Log("Hold");
                }
                break;
            }
            case State.Hold:
            {
                releaseReady_ = (now - holdStart_) > holdTime;
                onSmoothProcessed.Invoke(attackValue_);
                break;
            }
            case State.Release:
            {
                var timeSinceReleaseStart =  now - releaseStart_;
                onSmoothProcessed.Invoke(attackValue_ * (1.0f - (timeSinceReleaseStart / releaseTime)));
                if (timeSinceReleaseStart >= releaseTime)
                {
                    state_ = State.StandBy;
                }
                break;
            }
        }
    }
    
    private bool CanStartAttackPhase(float value)
    {
        return value < threshold && state_ == State.StandBy;
    }

    private bool CanStartReleasePhase(float value)
    {
        return state_ == State.Hold && value < threshold && releaseReady_;
    }
}
