// This script draws a debug line around mesh triangles
// as you move the mouse over them.
using UnityEngine;
using System.Collections;

public class test : MonoBehaviour
{
    Camera cam;
    public Color color;
    static float[] times;
    void Start()
    {
        times = new float[1000];
        for (int i = 0; i < 1000; i++)
            times[i] = 0.0f;

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
        Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

        StartColor[triangles[hit.triangleIndex * 3 + 0]] = DecideColor(ref times[triangles[hit.triangleIndex * 3 + 0]]);
        StartColor[triangles[hit.triangleIndex * 3 + 1]] = DecideColor(ref times[triangles[hit.triangleIndex * 3 + 1]]);
        StartColor[triangles[hit.triangleIndex * 3 + 2]] = DecideColor(ref times[triangles[hit.triangleIndex * 3 + 2]]);

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
        mesh.colors = StartColor;
        
    }

    public Color DecideColor(ref float times)
    {
        times += 10;

        //white >> yello        (1,1,1) >> (1,1,0)
        if (times >= 0 && times <= 255)
            return new Color(1, 1, 1 - times % 255 / 255);

        //yello >> green        (1,1,0) >> (0,1,0)
        else if (times > 255 && times <= 255 * 2)
            return new Color(1 - times % 255 / 255, 1, 0);

        //green >> blue         (0,1,0) >> (0,1,1)
        else if (times > 255 * 2 && times <= 255 * 3)
            return new Color(0, 1, times % 255 / 255);

        //blue >> dark blue     (0,1,1) >> (0,0,1)
        else if (times > 255 * 3 && times <= 255 * 4)
            return new Color(0, 1 - times % 255 / 255, 1);

        //dark blue >> purple   (0,0,1) >> (1,0,1)
        else if (times > 255 * 4 && times <= 255 * 5)
            return new Color(times % 255 / 255, 0, 1);

        //purple >> red         (1,0,1) >> (1,0,0)
        else if (times > 255 * 5 && times <= 255 * 6)
            return new Color(1, 0, 1 - times % 255 / 255);

        //more is red
        else
            return new Color(1, 0, 0);

    }
}