using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class point_info
{
    public int vertex_index;    //UpdateModelRecordCount會用到
    public Vector3 point;
    public string model_name;
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
    #region 可調變數們
    private float lastTime;
    public float deltaTime = 1;
    static float _dTime = 0;
    [Range(0.5f, 10f)]
    public float influence_Radius = 5;

    //高斯函數參數：https://www.geogebra.org/graphing/kxg6mru2
    [Range(0.1f, 5f)]
    //最高y值
    public float Gaus_a = 1.0f;
    //中心位移
    private float Gaus_b = 0.0f;
    [Range(0f, 5f)]
    public float Gaus_c = 3.0f;
    #endregion

    //KBV >> Can Be View
    public static List<string> KBV_Model_name = new List<string>();
    public static List<ModelRecord> KBV_Model_Record = new List<ModelRecord>();             //用 KBV_Model_name 找到資料庫
    public static List<Model_vertex_count_record> KBV_Model_vertex_count_record = new List<Model_vertex_count_record>();             //用 KBV_Model_name 找到資料庫

    public static List<point_info> KBV_WorldVertices = new List<point_info>();       //存放所有可以被看到的點，之後要先去做 KD Tree 判斷
    public static List<point_info> KDT_WorldVertices = new List<point_info>();       //存放 KD Tree 覺得接近的點

    Camera cam;
    Plane[] planes;

    static Raycast instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

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
        _dTime += Time.deltaTime;
        //if (Input.GetMouseButton(0))  //使用滑鼠左鍵開始paint
        //if (_dTime >= deltaTime)      //間隔deltaTime時間就paint
        if (true)                       //每個update都paint
        {
            _dTime = 0;
            print("paint!!");

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

    public GameObject GoToDraw;
    void reCalculateKBV_Model()
    {
        KBV_Model_name.Clear();
        KBV_Model_Record.Clear();
        KBV_Model_vertex_count_record.Clear();

        //第一層---讀取GoToDraw物件下的所有子物件資料
        foreach (Transform subObj in GoToDraw.transform)
        {
            if (!subObj.gameObject.activeSelf)
                continue;
            if (!subObj.GetComponent<Model_vertex_count_record>())
                continue;

            //第二層---模型過濾(該模型有沒有在視錐體範圍testPlaneAABB內)
            if (!GeometryUtility.TestPlanesAABB(planes, subObj.GetComponent<Collider>().bounds))
                return;
            KBV_Model_name.Add(subObj.name);
            KBV_Model_Record.Add((ModelRecord)Resources.Load("ModelRecord/" + subObj.name, typeof(ModelRecord)));
            KBV_Model_vertex_count_record.Add(subObj.GetComponent<Model_vertex_count_record>());
        }
    }

    void reCalculateKBV_Vertice()
    {
        KBV_WorldVertices.Clear();

        for (int j = 0; j < KBV_Model_Record.Count; j++)
            for (int i = 0; i < KBV_Model_Record[j].m_Data.number_of_vertices; i++)
            {
                //第三層---判斷vertex normal 是否朝向 camera
                Vector3 nowVerticeWorldPos = KBV_Model_Record[j].m_Data.vertices_world[i];
                Vector3 pointCameraDirection = transform.position - nowVerticeWorldPos;
                if (Vector3.Dot(KBV_Model_Record[j].m_Data.vertices_normal[i], pointCameraDirection) > 0)
                    continue;

                //第四層---vertex是否在screen上可被見(從該點發射射線往cemera
                /*RaycastHit temp_hit;
                if (Physics.Raycast(nowVerticeWorldPos, pointCameraDirection, out temp_hit))
                    if (!(temp_hit.transform.tag == "cameraBackWall"))
                        continue;*/
                point_info temp = new point_info(i, nowVerticeWorldPos, KBV_Model_Record[j].m_Data.Model_Name, 0);
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
        KDT_WorldVertices.Clear();

        Vector3[] temp_vertices_forKBVuse = new Vector3[KBV_WorldVertices.Count];

        for (int i = 0; i < KBV_WorldVertices.Count; i++)
        {
            temp_vertices_forKBVuse[i] = KBV_WorldVertices[i].point;
        }
        KDQuery query = new KDQuery();
        KDTree tree = new KDTree(temp_vertices_forKBVuse, 32);
        List<int> _resultPoint = new List<int>();
        query.Radius(tree, _hit.point, influence_Radius, _resultPoint);

        //KDT 的結果放在 _resultPoint
        for (int i = 0; i < _resultPoint.Count; i++)
        {
            point_info tempP = KBV_WorldVertices[_resultPoint[i]];

            float x = Vector3.Distance(tempP.point, _hit.point);
            //高斯函數(x軸-->距離，y軸-->weight)
            float Gaussian_func = Gaus_a * (float)Math.Exp(0 - Math.Pow(x - Gaus_b, 2) / (2 * Gaus_c * Gaus_c));
            tempP.weight = Gaussian_func;

            KDT_WorldVertices.Add(tempP);
        }

    }

    void UpdateModelRecordCount()
    {
        //更新新增的count
        for (int i = 0; i < KDT_WorldVertices.Count; i++)
        {
            int temp_id = KBV_Model_name.FindIndex(x => x == KDT_WorldVertices[i].model_name);
            KBV_Model_Record[temp_id].m_Data.count[KDT_WorldVertices[i].vertex_index] += KDT_WorldVertices[i].weight;
            KBV_Model_Record[temp_id].m_Data.hey_need_update = true;

            //處理(紀錄)時間軸
            time_info temp_time_info = new time_info();
            float nowTime = Time.time;
            temp_time_info.delta_time = nowTime - lastTime;
            lastTime = nowTime;
            temp_time_info.all_vertice_count = KBV_Model_vertex_count_record[temp_id].GetAllVerticeCount();
            KBV_Model_vertex_count_record[temp_id].AddHistory(temp_time_info);

        }

        //叫每個模型的shader去上新的顏色
        for (int i = 0; i < KBV_Model_vertex_count_record.Count; i++)
        {
            KBV_Model_vertex_count_record[i].CheckChange();
        }

    }

}

[System.Serializable]
public class time_info
{
    public float delta_time;    //紀錄與上一個紀錄相差(經過)多少時間
    public float[] all_vertice_count;
}