// This script draws a debug line around mesh triangles
// as you move the mouse over them.
using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour
{
    Camera cam;
    public Color color;
    static int[] times;
    void Start()
    {
        times = new int[1000] ;
        for (int i = 0; i <1000; i++)
            times[i] = 0;

        cam = GetComponent<Camera>();
    }

    void Update()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;
        
        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return;

        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        //原本的顏色
        Color[] StartColor = mesh.colors;
        if (StartColor.Length != vertices.Length)
        {
            StartColor = new Color[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                StartColor[i] = new Color(1, 1, 1);
        }

        ///***
        Color[] newColor = StartColor;

        Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

        newColor[triangles[hit.triangleIndex * 3 + 0]] = DecideColor(ref times[triangles[hit.triangleIndex * 3 + 0]]);
        newColor[triangles[hit.triangleIndex * 3 + 1]] = DecideColor(ref times[triangles[hit.triangleIndex * 3 + 1]]);
        newColor[triangles[hit.triangleIndex * 3 + 2]] = DecideColor(ref times[triangles[hit.triangleIndex * 3 + 2]]);
        
        Transform hitTransform = hit.collider.transform;
        p0 = hitTransform.TransformPoint(p0);
        p1 = hitTransform.TransformPoint(p1);
        p2 = hitTransform.TransformPoint(p2);

        //Debug.Log("p0"+ p0+ " ; p1" + p1+ " ; p2" + p2);

        //出現再scene視窗
        Debug.DrawLine(p0, p1, Color.red, 10f, true);
        Debug.DrawLine(p1, p2, Color.red, 10f, true);
        Debug.DrawLine(p2, p0, Color.red, 10f, true);

        ///***
        hit.transform.gameObject.GetComponent<MeshFilter>().mesh.colors = newColor;

    }

    public Color DecideColor(ref int times)
    {
        times += 10;
        return new Color(1, 0, 0);
        
    }
}