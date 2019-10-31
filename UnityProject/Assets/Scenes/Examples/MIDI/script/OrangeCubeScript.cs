using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace cylvester
{
    public class OrangeCubeScript : MonoBehaviour
    {
        [SerializeField] private Animator anim;

        public void Go()
        {
            anim.SetTrigger("AnimTrigger");
        }
    }
}
