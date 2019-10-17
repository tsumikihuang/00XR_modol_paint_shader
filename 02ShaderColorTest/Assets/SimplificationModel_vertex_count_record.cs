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

    public GameObject origin_obj;
    private void Start()
    {
        Init_ModelRecord();
        material = origin_obj.GetComponent<Renderer>().material;
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
        Scriptable_Data.m_Data.count = new int[len];

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
        Scriptable_Data.m_Data.normalizeDATA_Pass2Shader = FormatVertexInfo();

        Draw_with_dynamic_array();

    }
    
    Vector4[] FormatVertexInfo()
    {
        List<Vector4> temp_record = new List<Vector4>();

        for (int i=0;i< Scriptable_Data.m_Data.number_of_vertices;i++)
        {
            if (Scriptable_Data.m_Data.count[i] == 0)
                continue;
            temp_record.Add(new Vector4(Scriptable_Data.m_Data.vertices_world[i].x, Scriptable_Data.m_Data.vertices_world[i].y, Scriptable_Data.m_Data.vertices_world[i].z, Scriptable_Data.m_Data.count[i]));
        }

        Vector4[] list_temp = temp_record.ToArray();
        Vector4[] ans = new Vector4[list_temp.Length * 4];

        for (int i = 0; i < list_temp.Length; i++)
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

    public void Draw_with_dynamic_array()
    {
        int point_num = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader.Length;
        Texture2D input = new Texture2D(point_num, 1, TextureFormat.RGBA32, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < point_num; i++)
        {
            float colorX = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader[i].x / 10.0f;
            float colorY = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader[i].y / 10.0f;
            float colorZ = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader[i].z / 10.0f;
            float colorW = Scriptable_Data.m_Data.normalizeDATA_Pass2Shader[i].w / 10.0f;
            input.SetPixel(i, 0, new Color(colorX, colorY, colorZ, colorW));
        }
        input.Apply();

        material.SetInt("pixel_count", point_num);
        material.SetFloat("_Radius", 5.0f );
        //material.SetFloat("_MaxCount", hotSpot.MaxCount);
        material.SetTexture("array", input);
    }
}