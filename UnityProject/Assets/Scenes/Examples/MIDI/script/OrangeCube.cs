using UnityEngine;

namespace cylvester
{
    public class OrangeCube : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        private static readonly int AnimTrigger = Animator.StringToHash("AnimTrigger");

        public void OnTriggerReceived()
        {
            anim.SetTrigger(AnimTrigger);
        }
        
        
    }
}
