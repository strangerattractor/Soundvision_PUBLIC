using UnityEngine;

namespace cylvester
{
    public class Mover : MonoBehaviour
    {
        public void Move(int pos)
        {
            transform.position = new Vector3(pos - 8.0f, 0f, 0f);
        }
    }


}

