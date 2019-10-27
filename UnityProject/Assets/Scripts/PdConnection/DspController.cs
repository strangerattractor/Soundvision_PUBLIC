using System.Text;

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
                sender_.Send ("processing " + (value ? '1' : '0'));
            }
        }
    }
}