using System.Collections.Generic;
using Windows.Kinect;

namespace cylvester
{
    public class BodyHolder
    {
        private readonly List<Body> elements_;
        private readonly int capacity_;
        
        public BodyHolder(int capacity)
        {
            capacity_ = capacity;
            elements_ = new List<Body>();
        }

        public bool Add(Body newBody)
        {
            if (elements_.Count == capacity_)
                return false;

            if (elements_.Contains(newBody))
                return false;
            
            elements_.Add(newBody);
            return true;
        }

        public bool Exist(Body element)
        {
            return elements_.Contains(element);
        }
        
        public int? IndexOf(Body element)
        {
            var index = elements_.FindIndex(e => e.Equals(element));
            return (index < 0) ? (int?) null : index;
        }
        
        public void Remove(Body element)
        {
            elements_.Remove(element);
        }
    }
}