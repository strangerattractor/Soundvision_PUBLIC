using System;
using System.IO.MemoryMappedFiles;

namespace cylvester
{
    public interface IPdArray
    {
        float[] Data { get; }
    }

    public class PdArray : IDisposable, IPdArray, IUpdater
    {
        public float[] Data { get; }

        private readonly MemoryMappedFile memoryMappedFile_;
        private readonly MemoryMappedViewAccessor viewAccessor_;
        
        public PdArray(string name, int size)
        {
            memoryMappedFile_ = MemoryMappedFile.OpenExisting(name);
            if (memoryMappedFile_ == null)
                throw new ArgumentException("no such shared memory");
            
            viewAccessor_ = memoryMappedFile_.CreateViewAccessor();
            Data = new float[size];
        }

        public void Update()
        {
            viewAccessor_.ReadArray(0, Data, 0, Data.Length);
        }
        
        public void Dispose()
        {
            memoryMappedFile_.Dispose();
            viewAccessor_.Dispose();
        }
    }
}
