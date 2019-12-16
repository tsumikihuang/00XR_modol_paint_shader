using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;

public class point_info
{
    public int KBV_M_V_C_R_id;  //Can Be View Model Vertices Count Recoud Id
    public int vertex_index;    //UpdateSimpleModelRecordCount會用到
    public Vector3 point;
    public float weight;

    public point_info(int _KBV_M_V_C_R_id, int _vertex_index, Vector3 _point, float _weight)
    {
        KBV_M_V_C_R_id = _KBV_M_V_C_R_id;
        vertex_index = _vertex_index;
        point = _point;
        weight = _weight;
    }
}

public class Raycast : MonoBehaviour
{
    #region 可調變數們
        [Range(0.5f, 10f)]
        public float Eye_Influence_Radius = 5;

        //高斯函數參數：https://www.geogebra.org/graphing/kxg6mru2
        [Range(0.1f, 5f)]
        //最高y值
        public float Gaus_a = 1.0f;
        //中心位移
        private const float Gaus_b = 0.0f;
        [Range(0.1f, 5f)]
        public float Gaus_c = 3.0f;
    #endregion

    public static point_info[] All_Vertices;
    //KBV >> Can Be View
    public static List<Simple_Data> All_SimpleData = new List<Simple_Data>();
    public static point_info[] KDT_FindVertices;                           //存放 KD Tree 覺得接近的點

    Camera cam;
    Plane[] planes;

    KDQuery query;
    KDTree tree;
    public GameObject GoToCount;
    int all_vertices_len = 0;

    void Start()
    {
        cam = GetComponent<Camera>();
        //planes = GeometryUtility.CalculateFrustumPlanes(cam);

        //STEP 1 --- 抓取場景中所有模型。     //為了減少效能，找可見模型只做一次(之後再看看怎麼做比較好)
        reCalculateKBV_Model();

        //STEP 2 --- 建立所有頂點的 KD Tree
        Create_All_Vertice_Tree();
        
    }

    void reCalculateKBV_Model()
    {
        All_SimpleData.Clear();

        //第一層---讀取GoToDraw物件下的所有子物件資料
        foreach (Transform subObj in GoToCount.transform)
        {
            if (!subObj.gameObject.activeSelf)
                continue;
            if (!subObj.GetComponent<SimpleModel>())
                continue;

            //註解掉是因為 目前沒有每個update計算reCalculateKBV_Model VR頭盔一開始不一定會看到物體 所以可能導致失敗
            //第二層---模型過濾(該模型有沒有在視錐體範圍testPlaneAABB內)
            //if (!GeometryUtility.TestPlanesAABB(planes, subObj.GetComponent<Collider>().bounds))
            //    return;

            All_SimpleData.Add(subObj.GetComponent<SimpleModel>().Get_S_NoteBook().m_Data);

            all_vertices_len += subObj.GetComponent<SimpleModel>().Get_S_NoteBook().m_Data.number_of_vertices;
        }
    }

    void Create_All_Vertice_Tree()
    {
        All_Vertices = new point_info[all_vertices_len];
        for (int j = 0, index = 0; j < All_SimpleData.Count; j++)
            for (int i = 0; i < All_SimpleData[j].number_of_vertices; i++, index++)
            {
                All_Vertices[index] = new point_info(j, i, All_SimpleData[j].vertices_world[i], 0);
            }
        Vector3[] temp_vertices_forKBVuse = new Vector3[All_Vertices.Length];
        for (int i = 0; i < temp_vertices_forKBVuse.Length; i++)
        {
            temp_vertices_forKBVuse[i] = All_Vertices[i].point;
        }
        query = new KDQuery();
        tree = new KDTree(temp_vertices_forKBVuse, 32);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))  //使用滑鼠左鍵開始paint
        //if (true)                       //每個update都paint
        {
            Paint();
        }
    }

    //目前沒用到
    /*void reCalculateCameraDepth()
    {
        cam.depthTextureMode = DepthTextureMode.Depth;
        //Debug.Log("stop!!");
    }*/

    Vector3 look_pos;
    void Paint()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;
        Debug.DrawRay(cam.transform.position, hit.point - cam.transform.position, Color.red);
        look_pos = hit.point;

        //STEP 3 --- 用 KD Tree 把靠近的點的 vertice_id 和 model名稱紀錄下來
        Find_Vertices_In_Range(hit.point);

        //STEP 4 --- 將對應的SimpleModelRecord的count修改
        UpdateSimpleModelRecordCount();

        //reCalculateCameraDepth();

    }

    void Find_Vertices_In_Range(Vector3 point)
    {
        List<int> _resultPoint = new List<int>();
        query.Radius(tree, point, Eye_Influence_Radius, _resultPoint);

        KDT_FindVertices = new point_info[_resultPoint.Count];

        //KDT 的結果放在 _resultPoint
        for (int i = 0; i < _resultPoint.Count; i++)
        {
            point_info tempP = All_Vertices[_resultPoint[i]];

            if (!Judge_Vertice_KBV(tempP))
            {
                KDT_FindVertices[i] = null;
                continue;
            }

            float x = Vector3.Distance(tempP.point, point);
            //高斯函數(x軸-->距離，y軸-->weight)
            float Gaussian_func = Gaus_a * (float)Math.Exp(0 - Math.Pow(x - Gaus_b, 2) / (2 * Gaus_c * Gaus_c));
            tempP.weight = Gaussian_func;

            KDT_FindVertices[i] = tempP;
        }
    }

    private bool Judge_Vertice_KBV(point_info p)
    {
        //第一層---判斷vertex normal 是否朝向 camera
        Vector3 nowVerticeWorldPos = p.point;
        Vector3 pointCameraDirection = cam.transform.position - nowVerticeWorldPos;
        if (Vector3.Dot(cam.transform.forward, pointCameraDirection) > 0)
            return false;

        //第二層---vertex是否在screen上可被見(從該點發射射線往cemera
        RaycastHit temp_hit;
        if (Physics.Raycast(nowVerticeWorldPos, pointCameraDirection, out temp_hit))
            if (temp_hit.transform.tag == "cameraBackWall")
                return true;
        return false;
    }

    void UpdateSimpleModelRecordCount()
    {
        //更新新增的count
        for (int i = 0; i < KDT_FindVertices.Length; i++)
        {
            if (KDT_FindVertices[i] == null) continue;
            int temp_id = KDT_FindVertices[i].KBV_M_V_C_R_id;
            All_SimpleData[temp_id].count[KDT_FindVertices[i].vertex_index] += KDT_FindVertices[i].weight;
            All_SimpleData[temp_id].hey_need_update = true;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void OnDrawGizmos()
    {
        if (query == null || KDT_FindVertices == null)
            return;

        Vector3 size = 0.2f * Vector3.one;

        //可見點
        for (int i = 0; i < All_Vertices.Length; i++)
            Gizmos.DrawCube(All_Vertices[i].point, size);
        var resultIndices = new List<int>();

        Color markColor = Color.red;
        markColor.a = 0.5f;
        Gizmos.color = markColor;
        //上色點
        for (int i = 0; i < KDT_FindVertices.Length; i++)
        {
            if (KDT_FindVertices[i] == null) continue;
            Gizmos.DrawCube(KDT_FindVertices[i].point, 2f * size);
        }

        //視野範圍
        Gizmos.color = new Color(255, 255, 0, 0.3f);
        Gizmos.DrawSphere(look_pos, Eye_Influence_Radius);

    }

}
[System.Serializable]
public class time_info
{
    public float delta_time;    //紀錄與上一個紀錄相差(經過)多少時間
    public float[] all_vertice_count;
}

