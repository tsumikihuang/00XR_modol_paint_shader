//附在每一個物件上
//當raycast點到某物件時，就會呼叫此物件的NewChange()
using DataStructures.ViliWonka.KDTree;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Model_vertex_count_record : MonoBehaviour
{
    public int EVRN = 1;   //every_vert_record_num

    [Range(1f, 10f)]
    public float shaderRadius = 5.0f;

    private ModelRecord Scriptable_Data;      //儲存模型資料的 ScriptableObject

    #region material
    private Material material;
    //private Material m_material = null;
    /*public Material material
        {
            get
            {
                if (null == m_material)
                {
                    Renderer render = this.GetComponent<Renderer>();
                    if (null == render)
                        Debug.LogError("Can not found Renderer component on HeatMapComponent");
                    m_material = render.material;
                }
                return m_material;
            }
        }*/
    #endregion

    //public GameObject origin_obj;
    private void Awake()
    {
        Init_ModelRecord();
        material = this.GetComponent<Renderer>().material;
        NewChange();
    }

    private void Update()
    {
        NewChange();
    }

    public void CheckChange()
    {
        if (Scriptable_Data.m_Data.hey_need_update)
        {
            Scriptable_Data.m_Data.hey_need_update = false;
            NewChange();
        }
    }

    //清空所有值
    private void Init_ModelRecord()
    {
        if (Scriptable_Data == null)
        {
            Scriptable_Data = (ModelRecord)Resources.Load("ModelRecord/" + name, typeof(ModelRecord));
        }
        if (Scriptable_Data == null)
        {
            Debug.LogError("這個model沒有自己的ModelRecord檔案(必須放在Resources資料夾下，且檔案名稱與物件名稱相同)");
        }

        //裡面已經有資料
        if (Scriptable_Data.m_Data.Model_Name != "")
            return;

        //name
        Scriptable_Data.m_Data.Model_Name = name;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null || meshCollider.sharedMesh == null)
        {
            Debug.LogError("there is no meshCollider or sharedMesh");
            return;
        }

        //vertices_local
        Scriptable_Data.m_Data.vertices_local = meshCollider.sharedMesh.vertices;

        //normals_local
        Scriptable_Data.m_Data.vertices_normal = meshCollider.sharedMesh.normals;

        //number_of_vertices
        int len = Scriptable_Data.m_Data.number_of_vertices = Scriptable_Data.m_Data.vertices_local.Length;

        //vertices_world
        Scriptable_Data.m_Data.vertices_world = new Vector3[len];

        //reset history
        Scriptable_Data.m_Data.History = new List<time_info>();

        //count
        Scriptable_Data.m_Data.count = new float[len];

        //Init Record vertices
        for (int i = 0; i < len; i++)
        {
            Scriptable_Data.m_Data.vertices_world[i] = GetComponent<Collider>().transform.TransformPoint(Scriptable_Data.m_Data.vertices_local[i]);
            Scriptable_Data.m_Data.count[i] = 0;
        }
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    //取得所有count!!!!!!
    public float[] GetAllVerticeCount()
    {
        return Scriptable_Data.m_Data.count;
    }

    //設置所有count!!!!!!
    public void SetAllVerticeCount(float[] inputCount)
    {
        Scriptable_Data.m_Data.count= inputCount;
    }

    //新增time line
    public void AddHistory(time_info info)
    {
        Scriptable_Data.m_Data.History.Add(info);
    }

    public List<time_info> GetHistory()
    {
        return Scriptable_Data.m_Data.History;
    }
    
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //重上色時，呼叫這個!!!!!!
    public void NewChange()
    {
        //此 model 的每個 vertex ，紀錄各自的< 鄰近十個點的 x,y,z,count >，且數值在0~EVRN
        FormatVertexInfo();
    }

    void FormatVertexInfo()
    {
        int vNum = Scriptable_Data.m_Data.number_of_vertices;
        Vector4[] list_temp10X = new Vector4[vNum * EVRN];        //Vector4 >> 百、十、個、小數點後一位
        Vector4[] list_temp10Y = new Vector4[vNum * EVRN];
        Vector4[] list_temp10Z = new Vector4[vNum * EVRN];
        Vector4[] list_temp10Count = new Vector4[vNum * EVRN];    //Vector4 >> 個、小數點後 一、二、三 位

        for (int i = 0; i < vNum; i++)
        {
            ///KDT
            KDQuery query = new KDQuery();
            KDTree Vertice_tree = new KDTree(Scriptable_Data.m_Data.vertices_world, 32);
            List<int> _temp = new List<int>();
            if (EVRN == 1)
                _temp.Add(i);
            else
                query.KNearest(Vertice_tree, Scriptable_Data.m_Data.vertices_world[i], EVRN, _temp);

            for (int j = 0; j < EVRN; j++)
            {
                if (j >= _temp.Count || Scriptable_Data.m_Data.count[_temp[j]] == 0)
                {
                    list_temp10X[i * EVRN + j] = new Vector4(0, 0, 0, 0);
                    list_temp10Y[i * EVRN + j] = new Vector4(0, 0, 0, 0);
                    list_temp10Z[i * EVRN + j] = new Vector4(0, 0, 0, 0);
                    list_temp10Count[i * EVRN + j] = new Vector4(0, 0, 0, 0);
                    continue;
                }

                float temp_value;
                ///x
                temp_value = Scriptable_Data.m_Data.vertices_world[_temp[j]].x;
                list_temp10X[i * EVRN + j].w = 5;
                if (temp_value < 0)
                {
                    temp_value *= (-1);
                    list_temp10X[i * EVRN + j].w = 10;
                }
                list_temp10X[i * EVRN + j].x = (int)temp_value / 10;          //x座標的十位數
                list_temp10X[i * EVRN + j].y = (int)temp_value % 10;          //x座標的個位數
                list_temp10X[i * EVRN + j].z = (int)(temp_value * 10) % 10;   //x座標的小數後一位

                ///y
                temp_value = Scriptable_Data.m_Data.vertices_world[_temp[j]].y;
                list_temp10Y[i * EVRN + j].w = 5;
                if (temp_value < 0)
                {
                    temp_value *= (-1);
                    list_temp10Y[i * EVRN + j].w = 10;
                }
                list_temp10Y[i * EVRN + j].x = (int)temp_value / 10;          //x座標的十位數
                list_temp10Y[i * EVRN + j].y = (int)temp_value % 10;          //x座標的個位數
                list_temp10Y[i * EVRN + j].z = (int)(temp_value * 10) % 10;   //x座標的小數後一位

                ///z
                temp_value = Scriptable_Data.m_Data.vertices_world[_temp[j]].z;
                list_temp10Z[i * EVRN + j].w = 5;
                if (temp_value < 0)
                {
                    temp_value *= (-1);
                    list_temp10Z[i * EVRN + j].w = 10;
                }
                list_temp10Z[i * EVRN + j].x = (int)temp_value / 10;          //x座標的十位數
                list_temp10Z[i * EVRN + j].y = (int)temp_value % 10;          //x座標的個位數
                list_temp10Z[i * EVRN + j].z = (int)(temp_value * 10) % 10;   //x座標的小數後一位


                if (Scriptable_Data.m_Data.count[_temp[j]] >= 10)
                    Scriptable_Data.m_Data.count[_temp[j]] = 9.999f;
                list_temp10Count[i * EVRN + j].x = (int)Scriptable_Data.m_Data.count[_temp[j]] % 10;                //整數
                list_temp10Count[i * EVRN + j].y = (int)(Scriptable_Data.m_Data.count[_temp[j]] * 10) % 10;         //小數點後1位
                list_temp10Count[i * EVRN + j].z = (int)(Scriptable_Data.m_Data.count[_temp[j]] * 100) % 10;       //小數點後2位
                list_temp10Count[i * EVRN + j].w = (int)(Scriptable_Data.m_Data.count[_temp[j]] * 1000) % 10;     //小數點後3位

            }
        }

        material.SetTexture("array_10X", Draw_with_dynamic_array(list_temp10X, "list_temp10X"));
        material.SetTexture("array_10Y", Draw_with_dynamic_array(list_temp10Y, "list_temp10Y"));
        material.SetTexture("array_10Z", Draw_with_dynamic_array(list_temp10Z, "list_temp10Z"));
        material.SetTexture("array_10Count", Draw_with_dynamic_array(list_temp10Count, "list_temp10Count"));

        material.SetInt("vertice_count", vNum);
        material.SetFloat("_Radius", shaderRadius);
        material.SetFloat("_EVRN", EVRN);

    }

    Texture2D Draw_with_dynamic_array(Vector4[] ConvertArray, string name)
    {
        int point_num = ConvertArray.Length;
        int weight = point_num;
        if (point_num > 10000)
            weight = 10000;
        int height = point_num / 10000 + 1;
        Texture2D input = new Texture2D(weight, height, TextureFormat.RGBA32, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < weight; i++)
            {
                if (j * 10000 + i >= point_num)
                {
                    input.SetPixel(i, j, new Color(0, 0, 0, 0));
                    continue;
                }
                float colorX = ConvertArray[j * 10000 + i].x / 10.0f;
                float colorY = ConvertArray[j * 10000 + i].y / 10.0f;
                float colorZ = ConvertArray[j * 10000 + i].z / 10.0f;
                float colorW = ConvertArray[j * 10000 + i].w / 10.0f;
                input.SetPixel(i, j, new Color(colorX, colorY, colorZ, colorW));
            }
        }
        ////////////////////////////////////////////////////////////////////
        /*var bytes = input.EncodeToPNG();
        var dirPath = Application.dataPath + "/SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + name + ".png", bytes);*/
        //File.WriteAllBytes(Application.dataPath + "/SaveImages/Image333.png", input.EncodeToPNG());
        ////////////////////////////////////////////////////////////////////

        input.Apply();

        return input;
    }
}