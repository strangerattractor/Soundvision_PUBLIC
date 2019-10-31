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
            foreach (var body in bodies)
            {
                if(!body.IsTracked)
                    continue;
                
                var i = 0;
                foreach(var pair in body.Joints)
                {
                    var joint = pair.Value;
                    if(joint.TrackingState == TrackingState.NotTracked)
                        balls_[i].SetActive(false);
                    else
                    {
                        balls_[i].SetActive(true);
                        balls_[i].transform.position = new Vector3(joint.Position.X * 10f , joint.Position.Y * 10f, 0f);
                    }
                    i++;
                }
            }
        }
    }
}
