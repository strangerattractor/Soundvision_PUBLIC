using System;
using System.IO.MemoryMappedFiles;

namespace PdConnection
{
    public class PdBuffer : IDisposable
    {
        public float[] Data { get; }

        private readonly MemoryMappedFile memoryMappedFile_;
        private readonly MemoryMappedViewAccessor viewAccessor_;
        
        public PdBuffer(string name, int size)
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
            viewAccessor_?.Dispose();
            memoryMappedFile_?.Dispose();
        }
    }

}
