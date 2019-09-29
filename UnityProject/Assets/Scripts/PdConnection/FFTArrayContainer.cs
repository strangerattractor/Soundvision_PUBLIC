namespace cylvester
{
    public interface IFftArrayContainer
    {
        IPdArray this[int index] { get; }
        void Update();
    }
    
    public class FftArrayContainer : IFftArrayContainer
    {
        private readonly IPdArray[] arrays_;

        public FftArrayContainer()
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