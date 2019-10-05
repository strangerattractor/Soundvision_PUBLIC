namespace cylvester
{
    enum PdMessage
    {
        Dsp = 0,
        SampleSound = 1
    }
    
    public class PdConstant
    {
        public static readonly int NumMaxInputChannels = 16;
        public static readonly string ip = "127.0.0.1";
        public static readonly int port = 54345;
        public static readonly int FftSize = 512;
    }
}