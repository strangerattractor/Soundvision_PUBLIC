namespace cylvester
{
    public interface IPdArrayContainer
    {
        IPdArray this[int index] { get; }
    }

    public interface IUpdater
    {
        void Update();
    }
    
    public class PdArrayContainer : IPdArrayContainer, IUpdater
    {
        private readonly IPdArray[] arrays_;
        private readonly IUpdater[] updaters_;

        public PdArrayContainer(string prefix)
        {
            arrays_ = new IPdArray[PdConstant.NumMaxInputChannels];
            updaters_ = new IUpdater[PdConstant.NumMaxInputChannels];
            
            for (var i = 0; i < PdConstant.NumMaxInputChannels; ++i)
            {
                arrays_[i] = new PdArray(prefix + i, PdConstant.BlockSize);
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