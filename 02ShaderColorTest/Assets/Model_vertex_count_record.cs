//附在每一個物件上
//當raycast點到某物件時，就會呼叫此物件的NewChange()
using DataStructures.ViliWonka.KDTree;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Model_vertex_count_record : MonoBehaviour
{
    private int EVRN = 10;   //every_vert_record_num

    [Range(0.1f, 10f)]
    public float shaderRadius = 5.0f;

    private ModelRecord Scriptable_Data;      //儲存模型資料的 ScriptableObject
    public string ModelName;

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
        ModelName = name;
        Init_ModelRecord();
        material = this.GetComponent<Renderer>().material;
        Scriptable_Data.m_Data.hey_need_update = true;
        //NewChange();
    }

    private void Update()
    {
        //NewChange();
    }

    public void CheckChange()
    {
        if (Scriptable_Data.m_Data.hey_need_update)
        {
            print("model paint!!");

            Scriptable_Data.m_Data.hey_need_update = false;
            NewChange();
        }
    }

    //清空所有值
    private void Init_ModelRecord()
    {
        if (Scriptable_Data == null)
        {
            string filePath = @"Assets/ModelRecord/"+name+".asset";
            Scriptable_Data = (ModelRecord)AssetDatabase.LoadAssetAtPath(filePath, typeof(ModelRecord));
            if (Scriptable_Data == null)
            {
                Debug.LogError("這個model沒有自己的ModelRecord檔案(必須放在Resources資料夾下，且檔案名稱與物件名稱相同)");
                return;
            }
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
        int len = Scriptable_Data.m_Data.number_of_vertices = meshCollider.sharedMesh.vertexCount;

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

    public ModelRecord GetModelRecord()
    {
        return Scriptable_Data;
    }

    //取得所有count!!!!!!
    public float[] GetAllVerticeCount()
    {
        return Scriptable_Data.m_Data.count;
    }

    //設置所有count!!!!!!
    public void SetAllVerticeCount(float[] inputCount)
    {
        Scriptable_Data.m_Data.count = inputCount;
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
        one_to_ten_and_range01();
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
                float colorX = ConvertArray[j * 10000 + i].x ;
                float colorY = ConvertArray[j * 10000 + i].y ;
                float colorZ = ConvertArray[j * 10000 + i].z ;
                float colorW = ConvertArray[j * 10000 + i].w ;
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

    public void one_to_ten_and_range01()
    {
        float elementsRangeX = Scriptable_Data.m_Data.MaxElement_x - Scriptable_Data.m_Data.MinElement_x;
        float elementsRangeY = Scriptable_Data.m_Data.MaxElement_y - Scriptable_Data.m_Data.MinElement_y;
        float elementsRangeZ = Scriptable_Data.m_Data.MaxElement_z - Scriptable_Data.m_Data.MinElement_z;
        float elementsRangeCount = Scriptable_Data.m_Data.MaxElement_count - Scriptable_Data.m_Data.MinElement_count;

        int vNum = Scriptable_Data.m_Data.number_of_vertices;

        Vector4[] format = new Vector4[vNum * EVRN];

        for (int i = 0; i < vNum; i++)
        {
            KDQuery query = new KDQuery();
            KDTree Vertice_tree = new KDTree(Scriptable_Data.m_Data.vertices_world, 32);
            List<int> _temp = new List<int>();
            if (EVRN == 1)
                _temp.Add(i);
            else
                query.KNearest(Vertice_tree, Scriptable_Data.m_Data.vertices_world[i], EVRN, _temp);
            for (int j = 0; j < EVRN; j++)
            {
                if (j >= _temp.Count || Scriptable_Data.m_Data.count[_temp[j]] == 0) {
                    format[i * EVRN + j]=new Vector4(0, 0, 0, 0);
                    continue;
                }

                if (Scriptable_Data.m_Data.count[_temp[j]] >= 10)
                    Scriptable_Data.m_Data.count[_temp[j]] = 9.999f;

                float colorX = (Scriptable_Data.m_Data.vertices_world[_temp[j]].x - Scriptable_Data.m_Data.MinElement_x) / elementsRangeX;
                float colorY = (Scriptable_Data.m_Data.vertices_world[_temp[j]].y - Scriptable_Data.m_Data.MinElement_y) / elementsRangeY;
                float colorZ = (Scriptable_Data.m_Data.vertices_world[_temp[j]].z - Scriptable_Data.m_Data.MinElement_z) / elementsRangeZ;
                float colorW = (Scriptable_Data.m_Data.count[_temp[j]] - Scriptable_Data.m_Data.MinElement_count) / elementsRangeCount;
                format[i * EVRN + j]=new Vector4(colorX, colorY, colorZ, colorW);
            }
        }
        material.SetTexture("array", Draw_with_dynamic_array(format, "_DebugName"));

        material.SetInt("vertice_count", vNum);
        material.SetFloat("_Radius", shaderRadius);
        material.SetFloat("_EVRN", EVRN);

        material.SetFloat("_MinX", Scriptable_Data.m_Data.MinElement_x);
        material.SetFloat("_RangeX", elementsRangeX);
        material.SetFloat("_MinY", Scriptable_Data.m_Data.MinElement_y);
        material.SetFloat("_RangeY", elementsRangeY);
        material.SetFloat("_MinZ", Scriptable_Data.m_Data.MinElement_z);
        material.SetFloat("_RangeZ", elementsRangeZ);
        material.SetFloat("_MinW", Scriptable_Data.m_Data.MinElement_count);
        material.SetFloat("_RangeW", elementsRangeCount);
        //material.SetFloat("_MaxCount", Scriptable_Data.m_Data.MaxCount);

    }

}