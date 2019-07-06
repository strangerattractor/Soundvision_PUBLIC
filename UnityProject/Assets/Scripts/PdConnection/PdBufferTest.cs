using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PdConnection
{
    
    public class PdBufferTest : MonoBehaviour
    {
        void Start()
        {
            var pdBuf = new PdBuffer("testarray", 100);
            pdBuf.Update();
            foreach(var val in pdBuf.Data)
                Debug.Log(val);
        }


    }


}
