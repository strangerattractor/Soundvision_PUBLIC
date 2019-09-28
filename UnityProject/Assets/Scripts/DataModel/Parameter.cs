using System;

namespace cylvester
{
    interface IParameter<T> where T : IComparable<T> 
    {
        T Value { set; get; }
        
        event Action ValueChanged;
    }
    
    public class Parameter<T> : IParameter<T> where T : IComparable<T>
    {
        public Parameter(T initial)
        {
            Value = initial;
        }
        
        private T value_;
        
        public T Value
        {
            get => value_;
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