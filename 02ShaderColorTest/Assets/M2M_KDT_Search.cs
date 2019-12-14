using DataStructures.ViliWonka.KDTree;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//此檔案負責
//產生一個對應array存在OrignModelRecord
public class M2M_KDT_Search : MonoBehaviour
{
    int SYSTEM_MAX_TEXTURE_SIZE;
    int MAX_TEXTURE_SIZE_2;    //通常為16384*16384

    private int Now_We_Have_Texture_Number=2;

    int[][] KDT_WV_id;
    float[] count_group;
    int max_count_id = 0;

    public static M2M_KDT_Search instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        SYSTEM_MAX_TEXTURE_SIZE = SystemInfo.maxTextureSize;                //通常為16384
        MAX_TEXTURE_SIZE_2 = SYSTEM_MAX_TEXTURE_SIZE * SYSTEM_MAX_TEXTURE_SIZE;
    }

    public void Generate_M2M(OrignModel O_Model, SimpleModel S_Model, float Radius)
    {
        SimpleModelRecord S_NoteBook = S_Model.Get_S_NoteBook();
        OrignModelRecord O_NoteBook = O_Model.Get_O_NoteBook();

        //vertices_local
        Vector3[] O_vertices_local = O_NoteBook.m_Data.vertices_local;
        Vector3[] S_vertices_local = S_NoteBook.m_Data.vertices_local;

        //number_of_vertices
        int O_Vert_Len = O_NoteBook.m_Data.number_of_vertices;
        int S_Vert_Len = S_NoteBook.m_Data.number_of_vertices;

        if (O_Vert_Len < S_Vert_Len) { Debug.LogError("兩個模型位置似乎放錯了，要不檢查一下?"); return; }
        if (O_Vert_Len > MAX_TEXTURE_SIZE_2) { Debug.LogError("模型頂點數量超過texture可乘載，請聯絡資訊人員改code。\n或是更換更好的顯示卡\n您的頂點數為" + O_Vert_Len + "，顯示卡可乘載為" + MAX_TEXTURE_SIZE_2); return; }
        
        //vertices_world
        Vector3[] O_vertices_world = O_NoteBook.m_Data.vertices_world;
        Vector3[] S_vertices_world = S_NoteBook.m_Data.vertices_world;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////// K D T
        ///每個精細的O，對應到 多個不精細的S
        KDT_WV_id = new int[O_Vert_Len][];
        count_group = new float[O_Vert_Len + (4 - O_Vert_Len % 4)];  //補成4的倍數(因為要放到rgba四個一組)
        KDQuery query = new KDQuery();
        KDTree S_Vertice_tree = new KDTree(S_vertices_world, 32);
        for (int i = 0; i < O_Vert_Len; i++)
        {
            List<int> _temp = new List<int>();
            query.Radius(S_Vertice_tree, O_vertices_world[i], Radius, _temp);
            int temp_len = _temp.Count;
            KDT_WV_id[i] = _temp.ToArray();
            count_group[i] = temp_len;
            if (max_count_id < temp_len)
                max_count_id = temp_len;
        }
        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////// K D T
        //目前預設是，用固定大小的數量儲存每個點的鄰近點的id
        int pixel_you_need = (int)Mathf.Ceil(max_count_id * O_Vert_Len / 4.0f);        //多少個rgba
        if (pixel_you_need > MAX_TEXTURE_SIZE_2* Now_We_Have_Texture_Number)
        {
            int you_need_this_many_texture_to_save_KDT_vertice = (int)Mathf.Ceil((float)pixel_you_need / MAX_TEXTURE_SIZE_2);
            Debug.LogError("目前模型對應點取"+ max_count_id + "個點，總共會花費"+ pixel_you_need + "個空間儲存。" +
                           "\n建議將Radius調小或將模型放大(頂點間距離拉長)\n或請資訊人員將texture數量增加(目前數量為"+ Now_We_Have_Texture_Number + "，改為"+ you_need_this_many_texture_to_save_KDT_vertice + ")");
            return;
        }
        float[] Convert_KDTWVid_List_To_Array=new float[pixel_you_need*4];  //C#初始全為0

        for (int i = 0; i < O_Vert_Len; i++) {
            for (int j = 0; j < max_count_id; j++)
            {
                if (j >= KDT_WV_id[i].Length )
                {
                    Convert_KDTWVid_List_To_Array[i * max_count_id + j] = 0;
                    continue;
                }
                //所有id+1(從1開始)，因為0要拿來記錄空值，且rgba的a不要為0比較好(否則就是透明的了)
                //除以vertices數量，擷取id0~1
                Convert_KDTWVid_List_To_Array[i * max_count_id + j] = (float)(KDT_WV_id[i][j]+1)/ S_Vert_Len;
            }
        }
        //把count取0~1
        /*for (int i = 0; i < O_Vert_Len; i++)
        {
            count_group[i] /= max_count_id;
        }
        Draw_with_dynamic_array(count_group, orign_model.name + "_" + O_len+"_count_group_texture");
        */
        //Draw_with_dynamic_array(Convert_KDTWVid_List_To_Array, pixel_you_need, "debug");

        O_Model.Set_M2M_PassToShader(Convert_KDTWVid_List_To_Array);
        O_Model.Set_EVRN(max_count_id);
        Debug.Log("Finish!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }


}
