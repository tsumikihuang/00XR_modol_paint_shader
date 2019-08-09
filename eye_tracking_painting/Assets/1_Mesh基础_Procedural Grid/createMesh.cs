//射線發生碰撞時，

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class createMesh : MonoBehaviour
{
    private static Mesh mesh;

    private static Vector3[] vertices;     //mesh中總共用到多少點
    private static int[] triangles;

    private float deltaZ = 0.02f;   //避免collider太近導致無法好好偵測

    private float delta = 0.2f;     //


    void Start()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "EmptyMesh";

        vertices = new Vector3[0];
        triangles = new int[0];


    }


    void Update()
    {
        Event e = Event.current;//检测输入，Event.current当前窗口的事件

        //不想让SceneView视图接收鼠标点击选择事件，只希望在Hierarchy视图选择
        HandleUtility.AddDefaultControl(0);

        RaycastHit raycastHit = new RaycastHit();
        //Ray terrain = HandleUtility.GUIPointToWorldRay(e.mousePosition);    //从鼠标位置发射一条射线

        Ray terrain = Camera.main.ScreenPointToRay(Input.mousePosition);    //从鼠标位置发射一条射线

        if (Input.GetMouseButton(0) && Physics.Raycast(terrain, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("ground"))) //射线检测名为"ground"的层
        {
            if (raycastHit.transform.name != "Empty")
            {
                Debug.Log("transform.name : " + raycastHit.transform.name);

                Vector3 hitpoint = raycastHit.point;          //取得raycast點到的3D(世界)空間座標

                Vector3 relativePoint = transform.InverseTransformPoint(hitpoint);  //世界座標轉到模型(local)座標

                Debug.Log("transform.position : " + this.transform.position);
                Debug.Log("hitpoint : " + hitpoint);
                Debug.Log("relativePoint : " + relativePoint);

                Vector3[] temp_vertices = new Vector3[vertices.Length + 3];    //原本數量增加3個點

                //為每一點賦予位置
                for (int i = 0; i < vertices.Length; i++)
                {
                    temp_vertices[i] = vertices[i];
                }
                temp_vertices[vertices.Length] = new Vector3(relativePoint.x - delta, relativePoint.y - delta, relativePoint.z - deltaZ);     //左下
                temp_vertices[vertices.Length + 1] = new Vector3(relativePoint.x, relativePoint.y + delta, relativePoint.z - deltaZ);         //上
                temp_vertices[vertices.Length + 2] = new Vector3(relativePoint.x + delta, relativePoint.y - delta, relativePoint.z - deltaZ); //右下
                vertices = temp_vertices;

                //Vector2[] temp_uv = new Vector2[uv.Length+3];



                //畫三角形~
                //Unity設定，在世界空間、模型空間下，使用左手坐標系
                //在觀察空間下，使用右手坐標系
                int[] temp_triangles = new int[triangles.Length + 3];

                for (int i = 0; i < triangles.Length; i++)
                {
                    temp_triangles[i] = triangles[i];
                }
                temp_triangles[triangles.Length] = temp_vertices.Length - 3;
                temp_triangles[triangles.Length + 1] = temp_vertices.Length - 2;
                temp_triangles[triangles.Length + 2] = temp_vertices.Length - 1;
                triangles = temp_triangles;

                mesh.vertices = vertices;
                mesh.triangles = triangles;
                GetComponent<MeshCollider>().sharedMesh = mesh;


            }
        }
    }
}