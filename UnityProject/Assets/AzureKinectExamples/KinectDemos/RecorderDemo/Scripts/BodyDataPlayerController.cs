using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// BodyDataPlayerController starts and stops the body data recording and replaying.
    /// </summary>
    public class BodyDataPlayerController : MonoBehaviour
    {
        // reference to BodyDataRecorderPlayer
        private BodyDataRecorderPlayer saverPlayer;


        void Start()
        {
            saverPlayer = BodyDataRecorderPlayer.Instance;
        }

        void Update()
        {
            // alternatively, use the keyboard
            if (Input.GetButtonDown("Jump") && !saverPlayer.IsPlaying())  // start or stop recording
            {
                if (saverPlayer)
                {
                    if (!saverPlayer.IsRecording())
                    {
                        saverPlayer.StartRecording();
                    }
                    else
                    {
                        saverPlayer.StopRecordingOrPlaying();
                    }
                }
            }

            if (Input.GetButtonDown("Fire1") && !saverPlayer.IsRecording())  // start or stop playing
            {
                if (saverPlayer)
                {
                    if (!saverPlayer.IsPlaying())
                    {
                        saverPlayer.StartPlaying();
                    }
                    else
                    {
                        saverPlayer.StopRecordingOrPlaying();
                    }
                }
            }

        }

    }
}
