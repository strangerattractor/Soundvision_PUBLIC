using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace cylvester
{
    interface IPdProcess
    {
        void Start(string mainPatch, int numInputChannels);
        void Stop();
    }
    
    public class PdProcess : IPdProcess
    {
        private static PdProcess instance_ = null;
        private Process pdProcess_;

        private PdProcess() // cannot be instantiate normally
        {
        }

        public static PdProcess Instance
        {
            get { return instance_ ?? (instance_ = new PdProcess()); }
        }

        public void Start(string mainPatch, int numInputChannels)
        {
            if (pdProcess_ != null)
                return;
    
            pdProcess_ = new Process();
            pdProcess_.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pdProcess_.StartInfo.UseShellExecute = true;
            pdProcess_.StartInfo.FileName = Application.streamingAssetsPath + "/pd/win/pd.com";

            var path = Application.streamingAssetsPath + "/pd/patch/" + mainPatch;
            pdProcess_.StartInfo.Arguments = "-nogui -rt -inchannels " + numInputChannels + " " + path;
            pdProcess_.Start();
            Debug.Log("Pd Process started");

        }
    
        public void Stop()
        {
            pdProcess_?.Kill();
            pdProcess_ = null;
            Debug.Log("Pd Process stopped");
        }
    }
}