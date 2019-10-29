using UnityEngine;

namespace cylvester
{
    interface IMoveBall
    {
        float Position { set; }
    }

    public class MoveBall : MonoBehaviour, IMoveBall
    {
        public float Position
        {
            set
            {
                var x = value * 0.1f;
                transform.localPosition = new Vector3(x, 4f, 3f);
            }
        }
    }


}
