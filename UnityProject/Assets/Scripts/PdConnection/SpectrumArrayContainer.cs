namespace cylvester
{
    public interface ISpectrumArrayContainer
    {
        IPdArray this[int index] { get; }
        void Update();
    }
    
    public class SpectrumArrayContainer : ISpectrumArrayContainer
    {
        private readonly IPdArray[] arrays_;

        public SpectrumArrayContainer()
        {
            arrays_ = new IPdArray[16];
            for(var i  = 0; i < 16; ++i)
                arrays_[i] = new PdArray("fft_" + i, 512);
        }

        public void Update()
        {
            foreach (var array in arrays_)
            {
                array.Update();
            }
        }
        
        public IPdArray this[int index] => arrays_[index];
    }
}