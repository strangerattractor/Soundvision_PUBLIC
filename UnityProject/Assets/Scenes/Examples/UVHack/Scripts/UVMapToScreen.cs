using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVMapToScreen : MonoBehaviour
{
    public Camera cam;
    public float amount = 0;
    public bool correctSquare = true;
    public float textureSpread = 1;
    public float dampening = 0.1f;

    void CorrectUVMap(float lerpRate)
    {
        if (lerpRate <= 0) return;
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;

        float longSide = Mathf.Max(cam.scaledPixelWidth, cam.scaledPixelHeight);
        for (var i = 0; i < vertices.Length; i++)
        {
            //var u = cam.WorldToViewportPoint(transform.TransformPoint(vertices[i]));
            var u = cam.WorldToScreenPoint(transform.TransformPoint(vertices[i]));
            if (correctSquare)
            {
                u.x -= cam.scaledPixelWidth * 0.5f;
                u.y -= cam.scaledPixelHeight * 0.5f;
                u.x /= longSide;
                u.y /= longSide;
                u.x += 0.5f;
                u.y += 0.5f;
            }
            else
            {
                u.x -= cam.scaledPixelWidth * 0.5f;
                u.y -= cam.scaledPixelHeight * 0.5f;
                u.x /= cam.scaledPixelWidth * textureSpread;
                u.y /= cam.scaledPixelHeight * textureSpread;
                u.x += 0.5f;
                u.y += 0.5f;
            }
            uv[i] = new Vector2(Mathf.Lerp(uv[i].x, u.x, lerpRate), Mathf.Lerp(uv[i].y, u.y, lerpRate));
        }

        mesh.uv = uv;
    }

    // Start is called before the first frame update
    void Start()
    {
        CorrectUVMap(1);
    }

    // Update is called once per frame
    void Update()
    {
        CorrectUVMap(amount * dampening);
    }
}
