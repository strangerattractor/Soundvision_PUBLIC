using UnityEngine;

namespace cylvester
{ 
    public class AnimatorMidiSync : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        [SerializeField] private string animationName_ = "Animation_Name";
        private float speed_ = 1.0f;
        private int animationNameHash_;
        private bool trigger_;

        public void Start()
       {
           animationNameHash_ = Animator.StringToHash(animationName_);
        }

        public void OnTriggerReceived()
        {
            anim.speed = speed_;
            anim.Play(animationNameHash_, -1, 0f);
        }

        public void OnStateChanged(IStateReader currentState)
        {
            speed_ = currentState.CurrentState.Speed;
        }
    }
}
