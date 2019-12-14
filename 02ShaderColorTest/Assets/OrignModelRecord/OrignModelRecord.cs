
//此產生的物件名稱務必和inspector視窗內的名稱相同

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Orign_Data
{
    public string Model_Name = "";

    public int EVRN;        //every vertices record number

    public float[] M2M_PassToShader;

    public int number_of_vertices;
    public Vector3[] vertices_local;
    public Vector3[] vertices_world;
}
[CreateAssetMenu(fileName = "OrignModelRecord", menuName = "HeatMap/Orign Model Record", order = 1)]
public class OrignModelRecord : ScriptableObject
{
    public Orign_Data m_Data;
}