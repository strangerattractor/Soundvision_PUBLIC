using System;

namespace cylvester
{
    public interface IEditorToggle
    {
        event Action ToggleStateChanged;
        bool State { get; set; }
    }
    
    public class EditorToggle : IEditorToggle
    {
        private bool state_;
        
        public event Action ToggleStateChanged = () => { };
        
        public bool State
        {
            get => state_;
            set
            {
                if (state_ != value)
                {
                    state_ = value;
                    ToggleStateChanged.Invoke();
                }
            }
        }
    }
}