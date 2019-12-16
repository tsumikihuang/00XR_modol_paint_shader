
//此產生的物件名稱務必和inspector視窗內的名稱相同

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Simple_Data
{
    //在model Start()時設定，後面不需要被跟改的資訊
    public string Model_Name="";
    public int number_of_vertices;
    public Vector3[] vertices_local;        //  模型座標  mesh.vertices
    public Vector3[] vertices_world;        //  世界座標  Transform.TransformPoint()

    public Vector3[] vertices_normal;       //紀錄每個點的normal，用來之後判斷哪些點是面向攝影機的

    public bool hey_need_update;

    public List<time_info> History;

    //隨Update更新
    public float[] count;
    //public Vector4[] normalizeDATA_Pass2Shader_10xyzcount;   //每一個點 紀錄最近10點的座標和count

    ////////////////////////////////////////////////
    public float MaxElement_x= 50;
    public float MaxElement_y= 50;
    public float MaxElement_z= 50;
    public float MaxElement_count=10;
    public float MinElement_x=-50;
    public float MinElement_y=-50;
    public float MinElement_z=-50;
    public float MinElement_count=0;
}

[CreateAssetMenu(fileName ="SimpleModelRecord",menuName = "HeatMap/Simple Model Record", order =2)]
public class SimpleModelRecord : ScriptableObject
{
    public Simple_Data m_Data;
}
