using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleClass : MonoBehaviour
{
    public List<Vector4> tempStructureList = new List<Vector4>();   //123存座標 4存count
    
    Camera cam;
    bool isWaiting=false;

    public Transform o1, o2, o3, o4, o5;
    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            AddCountInList(o1.position);
            AddCountInList(o2.position);
            AddCountInList(o3.position);
            AddCountInList(o4.position);
            //AddCountInList(o5.position);
        }
        hotSpot.HS_Vector_list = FormatPointInfo();

        cam = GetComponent<Camera>();
    }

    private float m_timer = 0.0f;
    // 热力图刷新间隔 TODO: 支持设置为每帧刷新
    private float interval = 0.5f;

    public GameObject obj1, obj2, obj3,obj4, obj5, obj6, obj7;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            obj1.transform.position = tempStructureList[0];
            obj2.transform.position = tempStructureList[1];
            obj3.transform.position = tempStructureList[2];
            obj4.transform.position = tempStructureList[3];
            obj5.transform.position = tempStructureList[4];
            obj6.transform.position = tempStructureList[5];
            obj7.transform.position = tempStructureList[6];

            /*
            Debug.Log(" X: " + HeatMapComponent.elements[0].x + " Y: " + HeatMapComponent.elements[0].y + "    Z: " + HeatMapComponent.elements[0].z + "    W: " + HeatMapComponent.elements[0].w);
            Debug.Log(" X: " + HeatMapComponent.elements[1].x + " Y: " + HeatMapComponent.elements[1].y + "    Z: " + HeatMapComponent.elements[1].z + "    W: "+ HeatMapComponent.elements[1].w);
            Debug.Log(" X: " + HeatMapComponent.elements[2].x + " Y: " + HeatMapComponent.elements[2].y + "    Z: " + HeatMapComponent.elements[2].z + "    W: "+ HeatMapComponent.elements[2].w);
            */
        }

        /*m_timer += Time.deltaTime;
        if (!isWaiting && m_timer > interval )
        {
            isWaiting = true;
            m_timer = 0.0f;
            Paint();
        }
        else return;*/
        if (Input.GetKeyDown(KeyCode.B))    //產生射線
        {
            Paint();
        }
    }

    void Paint()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        #region vertice color
        /*
        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return;

        //抓取該目標物的點數和三角面數
        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
        
        Transform hitTransform = hit.collider.transform;

        //抓取raycast點到位置的三角形上三個點的世界座標
        p0 = hitTransform.TransformPoint(p0);       //從 hitTransform 的 local space TO world space
        p1 = hitTransform.TransformPoint(p1);
        p2 = hitTransform.TransformPoint(p2);

        Debug.Log("p0: " + p0 + "  p1: " + p1 + "  p2: " + p2);

        AddCountInList(p0);
        AddCountInList(p1);
        AddCountInList(p2);
        */
        #endregion
        Vector3 p = hit.point;
        //取到小數點後第一位
        Vector3 new_p = new Vector3((float)Math.Round(p.x, 1), (float)Math.Round(p.y, 1), (float)Math.Round(p.z, 1));
        AddCountInList(new_p);

        hotSpot.HS_Vector_list = FormatPointInfo();
        isWaiting = false;
    }

    void AddCountInList(Vector4 p )
    {
        int imatch = tempStructureList.FindIndex(x => x.x == p.x && x.y == p.y && x.z == p.z);

        if (imatch != -1)
            tempStructureList[imatch] = new Vector4(tempStructureList[imatch].x, tempStructureList[imatch].y, tempStructureList[imatch].z, tempStructureList[imatch].w + 1);
        else
        {
            tempStructureList.Add(new Vector4(p.x, p.y, p.z, 1));
            imatch = tempStructureList.Count-1;
        }
        
        CheckMaxSwitch(tempStructureList[imatch].w, ref hotSpot.MaxCount);        
    }

    void CheckMaxSwitch(float Temp, ref float MaxValue) {
        if (Temp > MaxValue)
            MaxValue = Temp;
    }

    Vector4[] FormatPointInfo()
    {
        /*
         list_temp[i].x >> x座標
         list_temp[i].y >> y座標
         list_temp[i].z >> z座標
         list_temp[i].w >> 次數

         ans[i].x >> 座標的十位數
         ans[i].y >> 座標的個位數
         ans[i].z >> 座標的小數後一位
         ans[i].w >> 處裡正負，1>>負
         */
        Vector4[] list_temp = tempStructureList.ToArray();
        Vector4[] ans = new Vector4[list_temp.Length * 4];

        for (int i = 0; i < list_temp.Length; i ++)
        {
            ///x
            ans[i * 4].w = 5; 
            if (list_temp[i].x < 0)
            {
                list_temp[i].x *= (-1);
                ans[i * 4].w = 10;
            }
            ans[i * 4].x = (int)list_temp[i].x / 10;          //x座標的十位數
            ans[i * 4].y = (int)list_temp[i].x % 10;          //x座標的個位數
            ans[i * 4].z = (int)(list_temp[i].x * 10) % 10;     //x座標的小數後一位

            ///y
            ans[i * 4 + 1].w = 5;
            if (list_temp[i].y < 0)
            {
                list_temp[i].y *= (-1);
                ans[i * 4 + 1].w = 10;
            }
            ans[i * 4 + 1].x = (int)list_temp[i].y / 10;
            ans[i * 4 + 1].y = (int)list_temp[i].y % 10;        
            ans[i * 4 + 1].z = (int)(list_temp[i].y * 10) % 10;    

            ///z
            ans[i * 4 + 2].w = 5;
            if (list_temp[i].z < 0)
            {
                list_temp[i].z *= (-1);
                ans[i * 4 + 2].w = 10;
            }
            ans[i * 4 + 2].x = (int)list_temp[i].z / 10;        
            ans[i * 4 + 2].y = (int)list_temp[i].z % 10;     
            ans[i * 4 + 2].z = (int)(list_temp[i].z * 10) % 10;  

            ///w
            ans[i * 4 + 3].x = (int)list_temp[i].w / 1000;       
            ans[i * 4 + 3].y = (int)list_temp[i].w % 1000 / 100;   
            ans[i * 4 + 3].z = (int)list_temp[i].w % 100 / 10;
            ans[i * 4 + 3].w = (int)list_temp[i].w % 10;
        }

        return ans;
    }
}
