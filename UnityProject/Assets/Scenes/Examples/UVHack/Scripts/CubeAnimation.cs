using UnityEngine;

namespace cylvester
{
    public class CubeAnimation : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        public float speed_ = 1.0f;
        private static readonly int RotX = Animator.StringToHash("BoxAnimationRotX");
        private static readonly int RotY = Animator.StringToHash("BoxAnimationRotY");
        private static readonly int RotZ = Animator.StringToHash("BoxAnimationRotZ");
        //private static readonly int TransX = Animator.StringToHash("BoxAnimationTransX");
        private static readonly int ScaleX = Animator.StringToHash("BoxAnimationScaleX");
        private static readonly int Origin = Animator.StringToHash("BoxAnimationOrigin");
        private bool trigger_;

        public int nextMove = 0;

        public void OnTriggerReceived()
        {
            anim.speed = speed_;
            float r = nextMove;
            if (r < 1)
            {
                anim.Play(RotX, -1, 0f);
            }
            else if (r < 2)
            {
                anim.Play(RotY, -1, 0f);
            }
            else if (r < 3)
            {
                anim.Play(RotZ, -1, 0f);
            }
            else if (r < 4)
            {
                anim.Play(ScaleX, -1, 0f);
            }
            else
            {
                anim.Play(Origin, -1, 0f);
            }
        }

        public void OnStateChanged(IStateReader currentState)
        {
            speed_ = currentState.CurrentState.Speed;
        }
    }
}
