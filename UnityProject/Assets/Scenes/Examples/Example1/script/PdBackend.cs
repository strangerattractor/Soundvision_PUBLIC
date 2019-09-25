using System.Diagnostics;
using UnityEngine;

namespace cylvester
{
    public class PdBackend : MonoBehaviour
    {
        [SerializeField] string mainPatch;
        [SerializeField] int inchannels = 2;

        Process pdProcess_;

        void Start()
        {
            pdProcess_ = new Process();
            pdProcess_.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pdProcess_.StartInfo.UseShellExecute = true;
            pdProcess_.StartInfo.FileName = Application.streamingAssetsPath + "/pd/win/pd.com";

            var path = Application.streamingAssetsPath + "/pd/patch/" + mainPatch;
            pdProcess_.StartInfo.Arguments = "-nogui -rt -inchannels " + inchannels + " " + path;
            pdProcess_.Start();
        }

        private void OnDestroy()
        {
            pdProcess_.Kill();
        }


    }

}