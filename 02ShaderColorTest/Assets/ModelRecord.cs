//此產生的物件名稱務必和inspector視窗內的名稱相同

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Data
{
    //在model Start()時設定，後面不需要被跟改的資訊
    public string Model_Name="";
    public int number_of_vertices;
    public Vector3[] vertices_local;      //  模型座標  mesh.vertices
    public Vector3[] vertices_world;      //  世界座標  Transform.TransformPoint()

    //隨Update更新
    public float[] count;
    public Vector4[] normalizeDATA_Pass2Shader;
}

[CreateAssetMenu(fileName ="ModelRecord",menuName ="HeatMap/Create Model Record",order =1)]
public class ModelRecord : ScriptableObject
{
    public Data m_Data;
}
