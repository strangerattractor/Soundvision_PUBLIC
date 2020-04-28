using UnityEngine;

namespace cylvester
{
    public class RandomPosition : MonoBehaviour
    {
        public void OnLoopStarted()
        {
           var x =  Random.Range(-3f, 3f);
           var y =  Random.Range(-3f, 3f);
           transform.localPosition = new Vector3(x, y, 0f);
        }
    }
}
