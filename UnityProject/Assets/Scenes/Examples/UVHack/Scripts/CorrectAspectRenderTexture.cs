using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectAspectRenderTexture : MonoBehaviour
{
    RenderTexture rt;
    public RenderTexture destinationTexture;

    // Start is called before the first frame update
    void Start()
    {
        var cam = GetComponent<Camera>();
        rt = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 16, RenderTextureFormat.ARGB32);
        rt.Create();
        cam.targetTexture = rt;
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.Blit(rt, destinationTexture);
    }
}
