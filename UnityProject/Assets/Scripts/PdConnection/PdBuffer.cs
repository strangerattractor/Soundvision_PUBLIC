using System;
using System.IO.MemoryMappedFiles;

namespace PdConnection
{
    public class PdBuffer : IDisposable
    {
        private readonly MemoryMappedFile memoryMappedFile_;
        private readonly MemoryMappedViewAccessor viewAccessor_;
    
        public PdBuffer()
        {
            memoryMappedFile_ = MemoryMappedFile.OpenExisting("shared_memory");
            viewAccessor_ = memoryMappedFile_.CreateViewAccessor();
        }
        
        public void Dispose()
        {
            viewAccessor_?.Dispose();
            memoryMappedFile_?.Dispose();
        }
    }

}
