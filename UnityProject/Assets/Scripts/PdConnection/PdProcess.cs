using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace cylvester
{
    public class PdProcess
    {
        private static PdProcess instance_ = null;
        private Process pdProcess_;

        private PdProcess()
        {
        } // cannot be instantiate normally
        
        public static PdProcess Instance => instance_ ?? (instance_ = new PdProcess());

        public void Start(string mainPatch)
        {

            if (pdProcess_ != null)
            {
                pdProcess_.Refresh();
                if (!pdProcess_.HasExited)
                    return;
            }

            pdProcess_ = new Process();
            pdProcess_.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pdProcess_.StartInfo.UseShellExecute = false;
            pdProcess_.StartInfo.FileName = Application.streamingAssetsPath + "/pd/win/pd.com";
            var path = Application.streamingAssetsPath + "/pd/patch/" + mainPatch;
            pdProcess_.StartInfo.Arguments = "-nogui -rt \"" + path + "\"";

            if (!pdProcess_.Start())
            {
                throw new Exception("Pd process failed to start");
            }
            Thread.Sleep(500);
            Debug.Log("Pd Process started");
        }
    
        public void Stop()
        {
            if (pdProcess_ == null)
                return;
            
            pdProcess_.Kill();
            pdProcess_ = null;
            Debug.Log("Pd Process stopped");
        }

        public bool Running
        {
            get
            {
                pdProcess_.Refresh();
                if (pdProcess_ == null)
                    return false;
                if (pdProcess_.HasExited)
                    return false;
                
                return true;
            }
        }
    }
}