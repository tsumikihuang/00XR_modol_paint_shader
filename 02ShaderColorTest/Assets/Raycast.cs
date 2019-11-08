using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class point_info
{
    public int vertex_index;    //UpdateModelRecordCount會用到
    Vector3 point;
    string model_name;
    public float weight;

    public point_info(int _vertex_index, Vector3 _point, string _model_name, float _weight)
    {
        vertex_index = _vertex_index;
        point = _point;
        model_name = _model_name;
        weight = _weight;
    }
}

public class Raycast : MonoBehaviour
{
    //KBV >> Can Be View
    public static List<string> KBV_Model_name = new List<string>();
    public static List<ModelRecord> temp_Data = new List<ModelRecord>();             //用 KBV_Model_name 找到資料庫
    public static List<point_info> KBV_WorldVertices = new List<point_info>();       //存放所有可以被看到的點，之後要先去做 KD Tree 判斷
    public static List<point_info> KDT_WorldVertices = new List<point_info>();       //存放 KD Tree 覺得接近的點

    Camera cam;

    void Start()
    {
        reCalculateKBV();

        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (true)
        {
            //reCalculateKBV();
            reCalculateCameraDepth();
            KDT_WorldVertices.Clear();
            Paint();
        }
    }

    void reCalculateCameraDepth()
    {
        cam.depthTextureMode = DepthTextureMode.Depth;
        //Debug.Log("stop!!");
    }

    void reCalculateKBV()
    {
        //第一層---模型過濾(該模型有沒有在視錐體範圍內)
        //...

        //第二層---vertex是否在screen上可被見
        //...

        //隨便無作上述動作層(?
        //model全包
        //vertices全包

        //先只看單一一個模型
        KBV_Model_name.Add("stanford-bunny (1)");
        temp_Data.Add((ModelRecord)Resources.Load("ModelRecord/" + KBV_Model_name[0], typeof(ModelRecord)));
        for (int i = 0; i < temp_Data[0].m_Data.number_of_vertices; i++)
        {
            point_info temp = new point_info(i, temp_Data[0].m_Data.vertices_world[i], temp_Data[0].m_Data.Model_Name,0);
            KBV_WorldVertices.Add(temp);
        }

    }

    void Paint()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        Vector3 p = hit.point;

        //STEP 1 --- 利用 KD Tree 把靠近的點的 vertice_id 和 model名稱紀錄下來
        KDT_find_vertices(hit);

        //STEP 2 --- 將對應的modelRecord的count修改
        UpdateModelRecordCount();

    }

    void KDT_find_vertices(RaycastHit _hit)
    {
        //if(滿足KDT)

        //目前只做，把p所在的該三角形的頂點輸出
        #region 之後應該用不到的部分
        MeshCollider meshCollider = _hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return;
        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        int p0_index = triangles[_hit.triangleIndex * 3 + 0];
        int p1_index = triangles[_hit.triangleIndex * 3 + 1];
        int p2_index = triangles[_hit.triangleIndex * 3 + 2];

        Vector3 p0 = vertices[p0_index];
        Vector3 p1 = vertices[p1_index];
        Vector3 p2 = vertices[p2_index];
        Transform hitTransform = _hit.collider.transform;
        p0 = hitTransform.TransformPoint(p0);
        p1 = hitTransform.TransformPoint(p1);
        p2 = hitTransform.TransformPoint(p2);
        Debug.DrawLine(p0, p1, Color.red, 10f, true);
        Debug.DrawLine(p1, p2, Color.red, 10f, true);
        Debug.DrawLine(p2, p0, Color.red, 10f, true);
        #endregion
        Vector3 p_hit= hitTransform.TransformPoint(_hit.point);

        float All_area = Vector3.Cross(p1-p0, p2-p0).magnitude / 2;
        float _area0= Vector3.Cross(p1 - p_hit, p2 - p_hit).magnitude / 2;
        float _area1= Vector3.Cross(p0 - p_hit, p2 - p_hit).magnitude / 2;
        float _area2= Vector3.Cross(p0 - p_hit, p1 - p_hit).magnitude / 2;
        point_info temp0 = new point_info( p0_index, p0, meshCollider.gameObject.name, All_area / (_area0 * 3));
        point_info temp1 = new point_info( p1_index, p1, meshCollider.gameObject.name, All_area / (_area1 * 3));
        point_info temp2 = new point_info( p2_index, p2, meshCollider.gameObject.name, All_area / (_area2 * 3));

        KDT_WorldVertices.Add(temp0);
        KDT_WorldVertices.Add(temp1);
        KDT_WorldVertices.Add(temp2);
    }

    void UpdateModelRecordCount()
    {
        for (int i = 0; i < KDT_WorldVertices.Count; i++) {
            temp_Data[0].m_Data.count[KDT_WorldVertices[i].vertex_index]+= KDT_WorldVertices[i].weight;
        }

    }


}
