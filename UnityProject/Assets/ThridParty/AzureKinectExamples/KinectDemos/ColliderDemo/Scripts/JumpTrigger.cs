using UnityEngine;
using System.Collections;


namespace com.rfilkov.components
{
    public class JumpTrigger : MonoBehaviour
    {
        void OnTriggerEnter()
        {
            //Debug.Log("Jump trigger entered.");

            // start the animation clip
            Animation animation = gameObject.GetComponent<Animation>();
            if (animation != null && !animation.isPlaying)
            {
                animation.Play();
            }

            // play the audio clip
            AudioSource audioSrc = gameObject.GetComponent<AudioSource>();
            if (audioSrc != null && !audioSrc.isPlaying)
            {
                audioSrc.Play();
            }
        }

    }
}
