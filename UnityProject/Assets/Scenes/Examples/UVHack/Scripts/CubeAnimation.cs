using UnityEngine;

namespace cylvester
{
    public class CubeAnimation : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        private float speed_ = 1.0f;
        private static readonly int RotX = Animator.StringToHash("BoxAnimationRotX");
        private static readonly int RotY = Animator.StringToHash("BoxAnimationRotY");
        private static readonly int RotZ = Animator.StringToHash("BoxAnimationRotZ");
        private static readonly int UVAmount = Animator.StringToHash("UVAmountAnimation");
        private bool trigger_;

        public void OnTriggerReceived()
        {
            anim.speed = speed_;
            float r = Random.Range(0, 3);
            if (r < 1)
            {
                anim.Play(RotX, -1, 0f);
            }
            else if (r < 2)
            {
                anim.Play(RotY, -1, 0f);
            }
            else
            {
                anim.Play(RotZ, -1, 0f);
            }
        }

        public void OnStateChanged(IStateReader currentState)
        {
            speed_ = currentState.CurrentState.Speed;
        }
    }
}
