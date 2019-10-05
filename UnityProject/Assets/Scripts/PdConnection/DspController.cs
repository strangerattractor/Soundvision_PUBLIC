namespace cylvester
{
    public interface IDspController
    {
        bool State { set; }
    }
    
    public class DspController : IDspController
    {
        private IPdSocket socket_;

        public DspController(IPdSocket socket)
        {
            socket_ = socket;
        }
        
        public bool State
        {
            set
            {
                socket_.Send (new[]{(byte)PdMessage.Dsp, (byte)(value?1:0)});
            }
        }
    }
}