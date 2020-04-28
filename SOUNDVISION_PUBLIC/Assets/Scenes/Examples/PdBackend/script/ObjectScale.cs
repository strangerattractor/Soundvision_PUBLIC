using UnityEngine;

namespace cylvester
{
  interface IObjectScale
    {
    float Size { set; }
            
    }

    public class ObjectScale : MonoBehaviour, IObjectScale
    {
        [SerializeField] Vector3 scale = new Vector3(0.01f, 0.01f, 0.01f);
        [SerializeField] Vector3 offset = new Vector3(0.1f, 0.1f, 0.1f);
        public float Size
           
        {
            set
            {
                var size = value;
                transform.localScale = new Vector3(size * scale.x + offset.x, size * scale.y + offset.y, size * scale.z + offset.z);
            }
        }
    }


}
