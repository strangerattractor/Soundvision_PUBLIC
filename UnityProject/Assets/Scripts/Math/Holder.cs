using System;
using System.Collections.Generic;

namespace cylvester
{
    public class Holder<T>  where T : IEquatable<T>
    {
        private readonly List<T> elements_;
        private readonly int capacity_;
        
        public Holder(int capacity)
        {
            capacity_ = capacity;
            elements_ = new List<T>();
        }

        public bool Add(T newElement)
        {
            if (elements_.Count == capacity_)
                return false;

            if (elements_.Contains(newElement))
                return false;
            
            elements_.Add(newElement);
            return true;
        }

        public bool Exist(T element)
        {
            return elements_.Contains(element);
        }
        
        public int? IndexOf(T element)
        {
            var index = elements_.FindIndex(e => e.Equals(element));
            return (index < 0) ? (int?) null : index;
        }
        
        public void Remove(T element)
        {
            elements_.Remove(element);
        }
    }
}