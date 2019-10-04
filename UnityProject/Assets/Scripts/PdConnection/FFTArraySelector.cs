namespace cylvester
{
    public interface ISpectrumArraySelector
    {
        int Selection { set; }
        float[] SelectedArray { get; }
    }
    
    public class SpectrumArraySelector : ISpectrumArraySelector
    {
        private int selection_;
        private readonly ISpectrumArrayContainer arrayContainer_;

        public SpectrumArraySelector(ISpectrumArrayContainer arrayContainer)
        {
            arrayContainer_ = arrayContainer;
        }

        public int Selection
        {
            set => selection_ = value;
        }

        public float[] SelectedArray => arrayContainer_[selection_].Data;
    }
}