namespace cylvester
{
    public interface IDspController
    {
        bool State { set; }
    }
    
    public class DspController : IDspController
    {
        private IPdSender sender_;

        public DspController(IPdSender sender)
        {
            sender_ = sender;
        }
        
        public bool State
        {
            set
            {
                sender_.Send (new[]{(byte)PdMessage.Dsp, (byte)(value?1:0)});
            }
        }
    }
}