using System;

namespace cylvester
{
    interface IChangeObserver<T> where T : IComparable<T> 
    {
        T Value { set; }
        event Action ValueChanged;
    }
    
    public class ChangeObserver<T> : IChangeObserver<T> where T : IComparable<T>
    {
        private T value_;
        public ChangeObserver(T initial)
        {
            Value = initial;
        }
        
        public T Value
        {
            set
            {
                if (value.CompareTo(value_) == 0) 
                    return;
                
                value_ = value;
                ValueChanged.Invoke();
            } 
        }
        
        public event Action ValueChanged = () => { };
    }
}