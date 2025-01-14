using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ProceduralMesh : MonoBehaviour
{
    private Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }
    void Update(){
        makeMeshData();
        SetMesh();
    }
    private void SetMesh(){
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    private void makeMeshData()
    {
        vertices = new Vector3[]{new Vector3(0,YValue.ins.yValue,0), new Vector3(0,0,1), new Vector3(1,0,0), new Vector3(1,0,1)};
        triangles = new int[]{0,1,2,2,1,3};

    }


}
