using UnityEngine;

namespace cylvester
{
    public class ConstructionColumnBehaviour : MonoBehaviour
    {
		private int frameCount_;        

        private void Start()
        {
			transform.position = new Vector3(Random.Range(-5.0f, 5.0f), 0f,  Random.Range(-5.0f, 5.0f));
        }

        private void Update()
        {
            frameCount_++;

			if(frameCount_ == 60)
				Destroy(this.gameObject);
        }
    }
}