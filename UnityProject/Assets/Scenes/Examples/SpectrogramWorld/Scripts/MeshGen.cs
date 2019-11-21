using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]

public class MeshGen : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public float xSize = 1;
    public float zSize = 1;
    public int xRes = 100;
    public int zRes = 100;

    Vector2[] uvs;
    Vector4[] tangents;

    private void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        CreateShape();
        UpdateMesh();
    }

    private void CreateShape()
    {
        //vertices being calculated
        vertices = new Vector3[(xRes + 1) * (zRes + 1)];
        uvs = new Vector2[vertices.Length];
        tangents = new Vector4[vertices.Length];

        for (int i = 0, z = 0; z <= zRes; z++)
        {
            for (int x = 0; x <= xRes; x++)
            {
                //convert int to float, then map range 0-1, then move by half size (origin at center), then scale by size
                vertices[i] = new Vector3(((x * 1.0f / xRes - 0.5f) * xSize), 0,(z * 1.0f / zRes - 0.5f) * zSize); 
                //map uvs 0-1 depending onf Resolution of mesh
                uvs[i] = new Vector2(1.0f - x * 1.0f / xRes, 1.0f - z * 1.0f / zRes);
                //set all tangents to plane
                tangents[i] = new Vector4(1f, 0f, 0f, -1f);
                i++;
            }
        }

        //triangles being calculated
        triangles = new int[xRes * zRes * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zRes; z++)
        {
            for (int x = 0; x < xRes; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xRes + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xRes + 1;
                triangles[tris + 5] = vert + xRes + 2;

                vert++;
                tris += 6;

            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.uv = uvs;
        mesh.tangents = tangents;

        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;

    }
    /*
        private void OnDrawGizmos()
     {
         if (vertices == null)
             return;

         for (int i = 0; i < vertices.Length; i++)
         {
             Gizmos.DrawSphere(vertices[i], .1f);
         }
     }
     */


}

  