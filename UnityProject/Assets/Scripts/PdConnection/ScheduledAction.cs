using System;

namespace cylvester
{
    public class ScheduledAction
    {
        public ScheduledAction(Action action)
        {
            action_ = action;
        }

        public void Ready()
        {
            standBy_ = true;
        }

        public void Go()
        {
            if (!standBy_) return;
            
            action_.Invoke(); 
            standBy_ = false;
        }

        private bool standBy_; 
        private readonly Action action_;
    }
}