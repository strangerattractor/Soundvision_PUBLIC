namespace cylvester
{
    public interface IPdArraySelector
    {
        int Selection { set; }
        float[] SelectedArray { get; }
        
    }
    
    
    public class PdArraySelector : IPdArraySelector
    {
        private int selection_;
        private readonly IPdArrayContainer arrayContainer_;

        public PdArraySelector(IPdArrayContainer arrayContainer)
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