namespace cylvester
{
    public class Boundary
    {
        private readonly double? min_;
        private readonly double? max_;

        public Boundary(double? min, double? max)
        {
            min_ = min;
            max_ = max;
        }

        public bool IsInside(double value)
        {
            if (min_.HasValue)
            {
                if (value < min_.Value)
                    return false;
            }
            if (max_.HasValue)
            {
                if (value > max_.Value)
                    return false;
            }

            return true;
        }
    }
}