using UnityEngine;

namespace cylvester
{
    public class OrangeCube : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        private float speed_ = 1.0f;
        private static readonly int Moving = Animator.StringToHash("Moving");
        private bool trigger_;
        
        public void OnTriggerReceived()
        {
            anim.speed = speed_;
            
            anim.SetBool(Moving, false);
            anim.SetBool(Moving, true);
        }

        public void OnStateChanged(IStateReader currentState)
        {
            speed_ = currentState.CurrentState.Speed;
        }
    }
}
