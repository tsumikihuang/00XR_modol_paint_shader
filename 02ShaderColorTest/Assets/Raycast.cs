using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class point_info
{
    public int KBV_M_V_C_R_id;  //Can Be View Model Vertices Count Recoud Id
    public int vertex_index;    //UpdateSimpleModelRecordCount會用到
    public Vector3 point;
    public string model_name;
    public float weight;

    public point_info(int _KBV_M_V_C_R_id,int _vertex_index, Vector3 _point, string _model_name, float _weight)
    {
        KBV_M_V_C_R_id = _KBV_M_V_C_R_id;
        vertex_index = _vertex_index;
        point = _point;
        model_name = _model_name;
        weight = _weight;
    }
}

public class Raycast : MonoBehaviour
{
    #region 可調變數們
    private float lastTime;
    public float deltaTime = 1;

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

    //KBV >> Can Be View
    public static List<SimpleModel> KBV_SimpleModel = new List<SimpleModel>();             //用 KBV_Model_name 找到資料庫

    public static List<point_info> KBV_WorldVertices = new List<point_info>();       //存放所有可以被看到的點，之後要先去做 KD Tree 判斷
    public static point_info[] KDT_WorldVertices;                           //存放 KD Tree 覺得接近的點

    Camera cam;
    Plane[] planes;

    /*public static Raycast instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }*/

    void Start()
    {
        lastTime = Time.time;
        cam = GetComponent<Camera>();

        planes = GeometryUtility.CalculateFrustumPlanes(cam);

        //為了減少效能，找可見模型只做一次(之後再看看怎麼做比較好)
        reCalculateKBV_Model();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))  //使用滑鼠左鍵開始paint
        //if (true)                       //每個update都paint
        {
            reCalculateKBV_Vertice();
            //reCalculateCameraDepth();

            Paint();
        }
    }

    //目前沒用到
    /*void reCalculateCameraDepth()
    {
        cam.depthTextureMode = DepthTextureMode.Depth;
        //Debug.Log("stop!!");
    }*/

    public GameObject GoToCount;
    void reCalculateKBV_Model()
    {
        KBV_SimpleModel.Clear();

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

            KBV_SimpleModel.Add(subObj.GetComponent<SimpleModel>());
        }
    }

    void reCalculateKBV_Vertice()
    {
        KBV_WorldVertices.Clear();

        for (int j = 0; j < KBV_SimpleModel.Count; j++)
            for (int i = 0; i < KBV_SimpleModel[j].Get_S_NoteBook().m_Data.number_of_vertices; i++)
            {
                //第三層---判斷vertex normal 是否朝向 camera
                Vector3 nowVerticeWorldPos = KBV_SimpleModel[j].Get_S_NoteBook().m_Data.vertices_world[i];
                Vector3 pointCameraDirection = transform.position - nowVerticeWorldPos;
                if (Vector3.Dot(KBV_SimpleModel[j].Get_S_NoteBook().m_Data.vertices_normal[i], pointCameraDirection) > 0)
                    continue;

                //第四層---vertex是否在screen上可被見(從該點發射射線往cemera
                RaycastHit temp_hit;
                if (Physics.Raycast(nowVerticeWorldPos, pointCameraDirection, out temp_hit))
                    if (!(temp_hit.transform.tag == "cameraBackWall"))
                        continue;
                point_info temp = new point_info(j,i, nowVerticeWorldPos, KBV_SimpleModel[j].ModelName, 0);
                KBV_WorldVertices.Add(temp);
            }
    }
    
    void Paint()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        //Debug.DrawRay(this.transform.position, hit.point- this.transform.position,Color.red);
        //Vector3 p = hit.point;

        //STEP 1 --- 利用 KD Tree 把靠近的點的 vertice_id 和 model名稱紀錄下來
        KDT_find_vertices(hit.point);

        //STEP 2 --- 將對應的SimpleModelRecord的count修改
        UpdateSimpleModelRecordCount();

    }
    KDQuery query;
    void KDT_find_vertices(Vector3 point)
    {
        Vector3[] temp_vertices_forKBVuse = new Vector3[KBV_WorldVertices.Count];

        for (int i = 0; i < KBV_WorldVertices.Count; i++)
        {
            temp_vertices_forKBVuse[i] = KBV_WorldVertices[i].point;
        }
        query = new KDQuery();
        KDTree tree = new KDTree(temp_vertices_forKBVuse, 32);
        List<int> _resultPoint = new List<int>();
        query.Radius(tree, point, Eye_Influence_Radius, _resultPoint);

        KDT_WorldVertices = new point_info[_resultPoint.Count];

        //KDT 的結果放在 _resultPoint
        for (int i = 0; i < _resultPoint.Count; i++)
        {
            point_info tempP = KBV_WorldVertices[_resultPoint[i]];

            float x = Vector3.Distance(tempP.point, point);
            //高斯函數(x軸-->距離，y軸-->weight)
            float Gaussian_func = Gaus_a * (float)Math.Exp(0 - Math.Pow(x - Gaus_b, 2) / (2 * Gaus_c * Gaus_c));
            tempP.weight = Gaussian_func;

            KDT_WorldVertices[i]=tempP;
        }
    }

    void UpdateSimpleModelRecordCount()
    {
        //更新新增的count
        for (int i = 0; i < KDT_WorldVertices.Length; i++)
        {
            int temp_id = KDT_WorldVertices[i].KBV_M_V_C_R_id;
            KBV_SimpleModel[temp_id].Get_S_NoteBook().m_Data.count[KDT_WorldVertices[i].vertex_index] += KDT_WorldVertices[i].weight;
            KBV_SimpleModel[temp_id].Get_S_NoteBook().m_Data.hey_need_update = true;
        }

        //叫每個模型的shader去上新的顏色
        /*for (int i = 0; i < KBV_SimpleModel.Count; i++)
        {
            KBV_SimpleModel[i].CheckChange();

            //處理(紀錄)時間軸
            time_info temp_time_info = new time_info();
            float nowTime = Time.time;
            temp_time_info.delta_time = nowTime - lastTime;
            lastTime = nowTime;
            temp_time_info.all_vertice_count = (float[])KBV_SimpleModel[i].GetAllVerticeCount().Clone() ;
            KBV_SimpleModel[i].AddHistory(temp_time_info);
        }*/

    }

    public void Buttom_GoToDraw()
    {
        for (int i = 0; i < KBV_SimpleModel.Count; i++)
        {
            //KBV_SimpleModel[i].CheckChange();
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void OnDrawGizmos()
    {
        if (query == null)
            return;

        Vector3 size = 0.2f * Vector3.one;

        //可見點
        for (int i = 0; i < KBV_WorldVertices.Count; i++)
            Gizmos.DrawCube(KBV_WorldVertices[i].point, size);

        var resultIndices = new List<int>();

        Color markColor = Color.red;
        markColor.a = 0.5f;
        Gizmos.color = markColor;

        //視野範圍
        Gizmos.DrawWireSphere(transform.position, Eye_Influence_Radius);

        //上色點
        for (int i = 0; i < KDT_WorldVertices.Length; i++)
        {
            Gizmos.DrawCube(KDT_WorldVertices[i].point, 2f * size);
        }
/*
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, 4f * size);

        if (DrawQueryNodes)
        {
            query.DrawLastQuery();
        }*/
    }

}
[System.Serializable]
public class time_info
{
    public float delta_time;    //紀錄與上一個紀錄相差(經過)多少時間
    public float[] all_vertice_count;
}

