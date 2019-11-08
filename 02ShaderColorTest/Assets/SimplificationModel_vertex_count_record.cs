//附在每一個物件上
//當raycast點到某物件時，就會呼叫此物件的NewChange(Vector3 _point)
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SimplificationModel_vertex_count_record : MonoBehaviour
{
    ModelRecord Scriptable_Data;      //儲存模型資料的 ScriptableObject

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
    }

    private void Update()
    {
        NewChange();
    }

    private void Init_ModelRecord()
    {
        if (Scriptable_Data == null)
        {
            Scriptable_Data = (ModelRecord)Resources.Load("ModelRecord/" + name, typeof(ModelRecord));
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

        //number_of_vertices
        int len = Scriptable_Data.m_Data.number_of_vertices = Scriptable_Data.m_Data.vertices_local.Length;

        //vertices_world
        Scriptable_Data.m_Data.vertices_world = new Vector3[len];

        //count
        Scriptable_Data.m_Data.count = new float[len];

        for (int i = 0; i < len; i++)
        {
            Scriptable_Data.m_Data.vertices_world[i] = GetComponent<Collider>().transform.TransformPoint(Scriptable_Data.m_Data.vertices_local[i]);
            Scriptable_Data.m_Data.count[i] = 0;
        }
    }

    //重畫!!

    public void NewChange()
    {
        //將此 model 的 vertex 資料，整理成 (x,y,z,count)，且數值在0~10
        //Scriptable_Data.m_Data.normalizeDATA_Pass2Shader = FormatVertexInfo();

        //大小為vertice數量，只傳vertice count，且數值在0~10
        Scriptable_Data.m_Data.normalizeDATA_Pass2Shader_count = FormatVertexCount();

        Draw_with_dynamic_array();

    }
    
    Vector4[] FormatVertexCount()
    {
        Vector4[] list_temp = new Vector4[Scriptable_Data.m_Data.number_of_vertices];
        
        for (int i = 0; i < list_temp.Length; i++)
        {
            if (Scriptable_Data.m_Data.count[i] >= 10)
                Scriptable_Data.m_Data.count[i] = 9.999f;
            list_temp[i].x = (int)Scriptable_Data.m_Data.count[i] % 10;                //整數
            list_temp[i].y = (int)(Scriptable_Data.m_Data.count[i] * 10) % 10;         //小數點後1位
            list_temp[i].z = (int)(Scriptable_Data.m_Data.count[i] * 100) % 100;       //小數點後2位
            list_temp[i].w = (int)(Scriptable_Data.m_Data.count[i] * 1000) % 1000;     //小數點後3位
        }

        return list_temp;
    }

    public void Draw_with_dynamic_array()
    {
        int point_num = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader_count.Length;
        int weight = point_num;
        if( point_num > 10000)
            weight = 10000;
        int height = point_num / 10000 + 1;
        Texture2D input = new Texture2D(weight, height, TextureFormat.RGBA32, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < weight; i++)
            {
                if(j * 10000 + i >= point_num)
                {
                    input.SetPixel(i, j, new Color(0, 0, 0, 0));
                    continue;
                }
                float colorX = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader_count[j * 10000 + i].x / 10.0f;
                float colorY = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader_count[j * 10000 + i].y / 10.0f;
                float colorZ = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader_count[j * 10000 + i].z / 10.0f;
                float colorW = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader_count[j * 10000 + i].w / 10.0f;
                input.SetPixel(i, j, new Color(colorX, colorY, colorZ, colorW));
            }
        }
        input.Apply();
        ////////////////////////////////////////////////////////////////////
        /*var bytes = input.EncodeToPNG();
        var dirPath = Application.dataPath + "/SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image333" + ".png", bytes);*/
        //File.WriteAllBytes(Application.dataPath + "/SaveImages/Image333.png", input.EncodeToPNG());
        ////////////////////////////////////////////////////////////////////
        material.SetInt("pixel_count", point_num);
        material.SetFloat("_Radius", 5.0f );
        //material.SetFloat("_MaxCount", hotSpot.MaxCount);
        material.SetTexture("array", input);
    }
}