using UnityEngine;

namespace cylvester
{
    public class OrangeCube : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        private float speed_ = 1.0f;
        private static readonly int OrangeBoxAnimation1 = Animator.StringToHash("OrangeBoxAnimation1");
        private bool trigger_;
        
        public void OnTriggerReceived()
        {
            anim.speed = speed_;
            anim.Play(OrangeBoxAnimation1, -1, 0f);
        }

        public void OnStateChanged(IStateReader currentState)
        {
            speed_ = currentState.CurrentState.Speed;
        }
    }
}
