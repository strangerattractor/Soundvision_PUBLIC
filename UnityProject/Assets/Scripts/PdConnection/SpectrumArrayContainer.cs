namespace cylvester
{
    public interface ISpectrumArrayContainer
    {
        IPdArray this[int index] { get; }
    }

    public interface IUpdater
    {
        void Update();
    }
    
    public class SpectrumArrayContainer : ISpectrumArrayContainer, IUpdater
    {
        private readonly IPdArray[] arrays_;
        private readonly IUpdater[] updaters_;

        public SpectrumArrayContainer()
        {
            arrays_ = new IPdArray[PdConstant.NumMaxInputChannels];
            updaters_ = new IUpdater[PdConstant.NumMaxInputChannels];
            
            for (var i = 0; i < PdConstant.NumMaxInputChannels; ++i)
            {
                arrays_[i] = new PdArray("fft_" + i, PdConstant.FftSize);
                updaters_[i] = (IUpdater) arrays_[i];
            }
        }

        public void Update()
        {
            foreach (var updater in updaters_)
            {
                updater.Update();
            }
        }
        
        public IPdArray this[int index] => arrays_[index];
    }
}