using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrangeCubeScript : MonoBehaviour
{
    public bool go;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Go()
{
    go = true;
}

    private void Update()
    {
        if (go == true)
        {
                gameObject.GetComponent<Animator>().SetTrigger("AnimTrigger");
        }
    }
    
    
}
