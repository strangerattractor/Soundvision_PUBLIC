using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;
using UnityEngine;

namespace cylvester
{
    public class Skeleton : MonoBehaviour
    {
        [SerializeField] private GameObject spherePrefab;

        private GameObject[] balls_;
        private void Start()
        {
            balls_ = new GameObject[25];
            for (var i = 0; i < 25; ++i)
            {
                balls_[i] = Instantiate(spherePrefab, gameObject.transform, true);
            }
        }


        public void OnSkeletonFrameReceived(Body[] bodies)
        {
            if (bodies.Length == 0)
                return;

            var body = bodies[0];

            var i = 0;
            foreach(var pair in body.Joints)
            {
                var joint = pair.Value;
                if(joint.TrackingState == TrackingState.NotTracked)
                    balls_[i].SetActive(false);
                else
                {
                    balls_[i].SetActive(true);
                    balls_[i].transform.position = new Vector3(joint.Position.X, joint.Position.Y, 0f);
                }
                i++;
            }
        }
    }
}
