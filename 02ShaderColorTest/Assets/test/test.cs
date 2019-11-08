// This script draws a debug line around mesh triangles
// as you move the mouse over them.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
                StartColor[i] = new Color(0, 0, 0);
        }

        ///***
        Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

        ////////////////
        Transform hitTransform = hit.collider.transform;
        p0 = hitTransform.TransformPoint(p0);
        p1 = hitTransform.TransformPoint(p1);
        p2 = hitTransform.TransformPoint(p2);
        Vector3 p_hit = hitTransform.TransformPoint(hit.point);

        float All_area = Vector3.Cross(p1 - p0, p2 - p0).magnitude / 2;
        float _area0 = Vector3.Cross(p1 - p_hit, p2 - p_hit).magnitude / 2;
        float _area1 = Vector3.Cross(p0 - p_hit, p2 - p_hit).magnitude / 2;
        float _area2 = Vector3.Cross(p0 - p_hit, p1 - p_hit).magnitude / 2;
        ////////////////
        times[triangles[hit.triangleIndex * 3 + 0]] += All_area / (30 * _area0);
        StartColor[triangles[hit.triangleIndex * 3 + 0]] = new Color(times[triangles[hit.triangleIndex * 3 + 0]], 0, 0);
        times[triangles[hit.triangleIndex * 3 + 1]] += All_area / (30 * _area1);
        StartColor[triangles[hit.triangleIndex * 3 + 1]] = new Color(times[triangles[hit.triangleIndex * 3 + 1]], 0, 0);
        times[triangles[hit.triangleIndex * 3 + 2]] += All_area / (30 * _area2);
        StartColor[triangles[hit.triangleIndex * 3 + 2]] = new Color(times[triangles[hit.triangleIndex * 3 + 2]], 0, 0);


        //出現再scene視窗
        Debug.DrawLine(p0, p1, Color.red, 10f, true);
        Debug.DrawLine(p1, p2, Color.red, 10f, true);
        Debug.DrawLine(p2, p0, Color.red, 10f, true);

        ///***
        mesh.colors = StartColor;
    }

    /*public Color DecideColor(ref float times,float count)
    {
        times += count*1000;

        //white >> dark blue     (1,1,1) >> (0,0,1)
        if (times >= 0 && times <= 255)
            return new Color(1 - times % 255 / 255, 1 - times % 255 / 255,1 );

        //dark blue >>  blue     (0,0,1) >> (0,1,1)
        else if(times >= 255 && times <= 255*2)
            return new Color(0, times % 255 / 255, 1);

        //blue >> green          (0,1,1) >> (0,1,0)
        else if (times > 255*2 && times <= 255 * 3)
            return new Color(0, 1, 1 - times % 255 / 255);

        //green >> yellow        (0,1,0) >> (1,1,0)
        else if (times > 255 * 3 && times <= 255 * 4)
            return new Color(times % 255 / 255, 1, 0);

        //yellow >>red           (1,1,0) >> (1,0,0)
        else if (times > 255 * 4 && times <= 255 * 5)
            return new Color(1, 1 - times % 255 / 255, 0);
       
        //more is red
        else
            return new Color(1, 0, 0);

    }*/
}