using UnityEngine;

namespace cylvester
{
    interface IPitchPosition
    {
        float Position { set; }
    }
    public class PitchPosition : MonoBehaviour
{
       [SerializeField] float pitchMultiplier = 0.1f;
        [SerializeField] float yOffset = 0;
        [SerializeField] float xOffset = 0;
        [SerializeField] float zOffset = 0;
       private Vector3 originalPosition;

        public void Start()
        {
            originalPosition = GetComponent<Transform>().position;
        }

        public float Position
        {
            set
            {
                var pitchOffset = value * pitchMultiplier;
                transform.localPosition = new Vector3(originalPosition.x + (pitchOffset * xOffset),
                    originalPosition.y + (pitchOffset * yOffset),
                    originalPosition.z + (pitchOffset + zOffset));
            }
        }
    }
}
