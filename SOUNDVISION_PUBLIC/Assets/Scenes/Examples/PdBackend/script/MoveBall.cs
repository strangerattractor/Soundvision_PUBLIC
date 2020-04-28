using UnityEngine;

namespace cylvester
{
    interface IMoveBall
    {
        float Offset { set; }
    }

    public class MoveBall : MonoBehaviour, IMoveBall
    {
        float x = 1.0f;

        public float Offset
        {
            set
            {
               x = value * 0.1f;
               transform.localPosition = new Vector3(x, 4f, 3f);
            }
        }
    }




}
