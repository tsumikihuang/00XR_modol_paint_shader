using DataStructures.ViliWonka.KDTree;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class M2M_KDT_Search : MonoBehaviour
{
    public float Radius = 5;
    int SYSTEM_MAX_TEXTURE_SIZE;
    int MAX_TEXTURE_SIZE_2;    //通常為16384*16384
    public MeshCollider orign_model;
    public MeshCollider simplify_model;
    private int Now_We_Have_Texture_Number=2;

    int[][] KDT_WV_id;
    float[] count_group;
    int max_id_count=0;
    private void Awake()
    {
        SYSTEM_MAX_TEXTURE_SIZE = SystemInfo.maxTextureSize;                //通常為16384
        MAX_TEXTURE_SIZE_2 = SYSTEM_MAX_TEXTURE_SIZE * SYSTEM_MAX_TEXTURE_SIZE;
    }
    void Start()
    {
        if (orign_model == null || orign_model.sharedMesh == null
            || simplify_model == null || simplify_model.sharedMesh == null)
        {
            Debug.LogError("there is no meshCollider or sharedMesh");
            return;
        }

        //vertices_local
        Vector3[] O_vertices_local = orign_model.sharedMesh.vertices;
        Vector3[] S_vertices_local = simplify_model.sharedMesh.vertices;

        //number_of_vertices
        int O_len = orign_model.sharedMesh.vertexCount;
        int S_len = simplify_model.sharedMesh.vertexCount;

        if (O_len < S_len) { Debug.LogError("兩個模型位置似乎放錯了，要不檢查一下?"); return; }
        if (O_len > MAX_TEXTURE_SIZE_2) { Debug.LogError("模型頂點數量超過texture可乘載，請聯絡資訊人員改code。\n或是更換更好的顯示卡\n您的頂點數為" + O_len + "，顯示卡可乘載為" + MAX_TEXTURE_SIZE_2); return; }


        //vertices_world
        Vector3[] O_vertices_world = new Vector3[O_len];
        for (int i = 0; i < O_len; i++)
            O_vertices_world[i] = orign_model.GetComponent<Collider>().transform.TransformPoint(O_vertices_local[i]);
        Vector3[] S_vertices_world = new Vector3[S_len];
        for (int i = 0; i < S_len; i++)
            S_vertices_world[i] = simplify_model.GetComponent<Collider>().transform.TransformPoint(S_vertices_local[i]);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////// K D T
        ///每個精細的O，對應到 多個不精細的S
        KDT_WV_id = new int[O_len][];
        count_group = new float[O_len+ (4 - O_len % 4)];  //補成4的倍數
        KDQuery query = new KDQuery();
        KDTree S_Vertice_tree = new KDTree(S_vertices_world, 32);
        for (int i = 0; i < O_len; i++)
        {
            List<int> _temp = new List<int>();
            query.Radius(S_Vertice_tree, O_vertices_world[i], Radius, _temp);
            int temp_len = _temp.Count;
            KDT_WV_id[i] = _temp.ToArray();
            count_group[i] = temp_len;
            if (max_id_count < temp_len)
                max_id_count = temp_len;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////// K D T
        //目前預設是，用固定大小的數量儲存每個點的鄰近點的id
        int pixel_you_need = (int)Mathf.Ceil(max_id_count * O_len/4.0f);
        if (pixel_you_need > MAX_TEXTURE_SIZE_2* Now_We_Have_Texture_Number)
        {
            int you_need_this_many_texture_to_save_KDT_vertice = (int)Mathf.Ceil((float)pixel_you_need / MAX_TEXTURE_SIZE_2);
            Debug.LogError("目前模型對應點取"+ max_id_count +"個點，總共會花費"+ pixel_you_need + "個空間儲存。" +
                           "\n建議將Radius調小或將模型放大(頂點間距離拉長)\n或請資訊人員將texture數量增加(目前數量為"+ Now_We_Have_Texture_Number + "，改為"+ you_need_this_many_texture_to_save_KDT_vertice + ")");
            return;
        }
        float[] Convert_KDTWVid_List_To_Array=new float[pixel_you_need*4];  //C#初始全為0

        for (int i = 0; i < O_len; i++) {
            for (int j = 0; j < max_id_count; j++)
            {
                if (j >= KDT_WV_id[i].Length )
                {
                    Convert_KDTWVid_List_To_Array[i * max_id_count + j] = 0;
                    continue;
                }
                //所有id+1(從1開始)，因為0要拿來記錄空值，且rgba的a不要為0比較好(否則就是透明的了)
                //除以vertices數量，擷取id0~1
                Convert_KDTWVid_List_To_Array[i * max_id_count + j] = (float)(KDT_WV_id[i][j]+1)/ S_len;
            }
        }
        //把count取0~1
        for (int i = 0; i < O_len; i++)
        {
            count_group[i] /= max_id_count;
        }

        Draw_with_dynamic_array(Convert_KDTWVid_List_To_Array, orign_model.name+"_"+max_id_count + "_nearst_id_texture");
        Draw_with_dynamic_array(count_group, orign_model.name + "_" + O_len+"_count_group_texture");
        Debug.Log("Finish!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }

    Texture2D Draw_with_dynamic_array(float[] ConvertArray, string name)
    {
        int pixel_num = ConvertArray.Length/4;
        int weight = pixel_num;
        if (pixel_num > SYSTEM_MAX_TEXTURE_SIZE)
            weight = SYSTEM_MAX_TEXTURE_SIZE;
        int height = pixel_num / SYSTEM_MAX_TEXTURE_SIZE + 1;
        Texture2D input = new Texture2D(weight, height, TextureFormat.RGBA32, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < weight;i++)
            {
                if (j * SYSTEM_MAX_TEXTURE_SIZE + i >= pixel_num)
                {
                    input.SetPixel(i, j, new Color(0, 0, 0, 0));
                    continue;
                }
                float colorX = ConvertArray[(j * SYSTEM_MAX_TEXTURE_SIZE + i)*4];
                float colorY = ConvertArray[(j * SYSTEM_MAX_TEXTURE_SIZE + i)*4+1];
                float colorZ = ConvertArray[(j * SYSTEM_MAX_TEXTURE_SIZE + i)*4+2];
                float colorW = ConvertArray[(j * SYSTEM_MAX_TEXTURE_SIZE + i)*4+3];
                input.SetPixel(i, j, new Color(colorX, colorY, colorZ, colorW));
            }
        }
        ////////////////////////////////////////////////////////////////////
        var bytes = input.EncodeToPNG();
        var dirPath = Application.dataPath + "/SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + name + ".png", bytes);
        //File.WriteAllBytes(Application.dataPath + "/SaveImages/Image333.png", input.EncodeToPNG());
        ////////////////////////////////////////////////////////////////////

        input.Apply();

        return input;
    }

}
