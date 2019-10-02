using UnityEngine;

namespace cylvester
{
    interface IBoomBall
    {
        float Size { set; }
    }

    public class BoomBall : MonoBehaviour, IBoomBall
    {
        public float Size
        {
            set
            {
                var scale = value * 0.01f + 0.1f;
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }


}
