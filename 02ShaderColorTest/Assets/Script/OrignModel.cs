using System.IO;
using UnityEditor;
using UnityEngine;

//原始模型
//負責上色，把上色資料傳給Shader

public class OrignModel : MonoBehaviour
{
    int SYSTEM_MAX_TEXTURE_SIZE;
    public SimpleModel S_Model;
    private Simple_Data S_NoteBook_Data;

    private OrignModelRecord O_NoteBook;      //儲存模型資料的 ScriptableObject
    private Orign_Data O_Data;
    private Material O_Material;
    public float O2S_Radius;
    public float ShaderRadius=0.5f;
    public int Max=10;
    private bool ShaderValueIsPassed = false;

    private void Awake()
    {
        SYSTEM_MAX_TEXTURE_SIZE = SystemInfo.maxTextureSize;                //通常為16384

        O_Material = this.GetComponent<Renderer>().material;
        //ShaderRadius = O2S_Radius/2;
        Init_OrignModelRecord();
    }
    private void Start()
    {
        S_NoteBook_Data = S_Model.Get_S_NoteBook().m_Data;
        S_NoteBook_Data.hey_need_update = true;
    }
    private void Update()
    {
        //這個只需做一次
        if (!ShaderValueIsPassed && O_Data.M2M_PassToShader != null)
        {
            PassM2M_OneTime();
            ShaderValueIsPassed = true;
        }

        CheckChange();
    }

    public void CheckChange()
    {
        if (S_NoteBook_Data.hey_need_update)
        {
            S_NoteBook_Data.hey_need_update = false;
            if (O_Data.M2M_PassToShader != null)
                NewChange();
        }
    }
    public void PassM2M_OneTime()
    {
        O_Material.SetInt("_EVRN", O_Data.EVRN);
        O_Material.SetTexture("M2M_array", Draw_with_dynamic_array_FA(O_Data.M2M_PassToShader, "debug"));

        //固定只需要傳一次的值
        O_Material.SetInt("SYSTEM_MAX_TEXTURE_SIZE", SYSTEM_MAX_TEXTURE_SIZE);
        O_Material.SetInt("S_len", S_NoteBook_Data.number_of_vertices);
        O_Material.SetInt("O_vertice_count", O_Data.number_of_vertices);
        O_Material.SetFloat("_MinX", S_NoteBook_Data.MinElement_x);
        O_Material.SetFloat("_RangeX", 100);
        O_Material.SetFloat("_MinY", S_NoteBook_Data.MinElement_y);
        O_Material.SetFloat("_RangeY", 100);
        O_Material.SetFloat("_MinZ", S_NoteBook_Data.MinElement_z);
        O_Material.SetFloat("_RangeZ", 100);
        O_Material.SetFloat("_MinW", S_NoteBook_Data.MinElement_count);
        O_Material.SetFloat("_RangeW", 10);

    }
    private void NewChange()
    {
        O_Material.SetTexture("SimpleModel_vertexINFO_array", Doing_S_Model_Pass_Vertices());
        O_Material.SetFloat("_Radius", ShaderRadius);
        O_Material.SetInt("_Max", Max);
    }

    public void Delete_M2M()
    {
        O_Data.M2M_PassToShader = null;
    }

    public float[] Read_M2M_PassToShader()
    {
        if (O_NoteBook == null)
            return null;
        if (O_Data.M2M_PassToShader == null)
            return null;
        if (O_Data.M2M_PassToShader.Length == 0)
            return null;
        else
            return O_Data.M2M_PassToShader;
    }

    public void Set_M2M_PassToShader(float[] Convert_KDTWVid_List_To_Array)
    {
        O_Data.M2M_PassToShader = (float[])Convert_KDTWVid_List_To_Array.Clone();
    }

    public void Set_EVRN(int max_id_count)
    {
        O_Data.EVRN = max_id_count;
    }

    public void OrignTexture()
    {

    }
    public void HeatMapTexture()
    {

    }

    public OrignModelRecord Get_O_NoteBook()
    {
        return O_NoteBook;
    }

    private void Init_OrignModelRecord()
    {
        O_NoteBook = Resources.Load<OrignModelRecord>("OrignModelRecord/" + name);
        if (O_NoteBook == null)
        {
            OrignModelRecord NewFile = new OrignModelRecord();
            AssetDatabase.CreateAsset(NewFile, "Assets/Resources/OrignModelRecord/" + name + ".asset");
            //Debug.LogError("這個model沒有自己的OrignModelRecord檔案(必須放在OrignModelRecord資料夾下，且檔案名稱與物件名稱相同)");
            Debug.LogWarning("以自動建立一個OrignModelRecord檔案>>" + name + ".asset" + "，放在Resources/OrignModelRecord資料夾下");
            O_NoteBook = Resources.Load<OrignModelRecord>("OrignModelRecord/" + name);
        }

        O_Data = O_NoteBook.m_Data;

        //裡面已經有資料
        if (O_Data.Model_Name != "")
            return;

        //沒資料，初始
        //name
        O_Data.Model_Name = name;
        O_Data.M2M_PassToShader = null;

        MeshFilter O_Collider = GetComponent<MeshFilter>();
        if (O_Collider == null || O_Collider.sharedMesh == null)
        {
            Debug.LogError("[Orign Model] There is no meshCollider or sharedMesh");
            return;
        }
        //vertices_local
        O_Data.vertices_local = O_Collider.sharedMesh.vertices;
        //number_of_vertices
        int len = O_Data.number_of_vertices = O_Collider.sharedMesh.vertexCount;
        //vertices_world
        O_Data.vertices_world = new Vector3[len];
        for (int i = 0; i < len; i++)
            O_Data.vertices_world[i] = GetComponent<MeshFilter>().transform.TransformPoint(O_Data.vertices_local[i]);

    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private Texture2D Draw_with_dynamic_array_FA(float[] ConvertArray, string name)
    {
        int pixel_num = ConvertArray.Length / 4;
        int weight = pixel_num;
        if (pixel_num > SYSTEM_MAX_TEXTURE_SIZE)
            weight = SYSTEM_MAX_TEXTURE_SIZE;
        int height = pixel_num / SYSTEM_MAX_TEXTURE_SIZE + 1;
        Texture2D input = new Texture2D(weight, height, TextureFormat.RGBAFloat, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < weight; i++)
            {
                int pixel_index_x4 = (j * weight + i) * 4;
                if ((j * weight + i) >= pixel_num)
                {
                    input.SetPixel(i, j, new Color(0, 0, 0, 0));
                    continue;
                }
                int range = S_NoteBook_Data.number_of_vertices + 1;
                float colorX = ConvertArray[pixel_index_x4] / range;
                float colorY = ConvertArray[pixel_index_x4 + 1] / range;
                float colorZ = ConvertArray[pixel_index_x4 + 2] / range;
                float colorW = ConvertArray[pixel_index_x4 + 3] / range;
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
        File.WriteAllBytes(dirPath + "M2M_id_23333" + ".png", bytes);*/
        //File.WriteAllBytes(Application.dataPath + "/SaveImages/Image333.png", input.EncodeToPNG());
        ////////////////////////////////////////////////////////////////////

        return input;
    }

    private Texture2D Doing_S_Model_Pass_Vertices()
    {
        float elementsRangeX = S_NoteBook_Data.MaxElement_x - S_NoteBook_Data.MinElement_x;
        float elementsRangeY = S_NoteBook_Data.MaxElement_y - S_NoteBook_Data.MinElement_y;
        float elementsRangeZ = S_NoteBook_Data.MaxElement_z - S_NoteBook_Data.MinElement_z;
        float elementsRangeCount = S_NoteBook_Data.MaxElement_count - S_NoteBook_Data.MinElement_count;

        int S_vNum = S_NoteBook_Data.number_of_vertices;

        Vector4[] format = new Vector4[S_vNum];

        for (int i = 0; i < S_vNum; i++)
        {
            if (S_NoteBook_Data.count[i] == 0)
            {
                format[i] = new Vector4(0, 0, 0, 0);
                continue;
            }

            if (S_NoteBook_Data.count[i] >= 10)
                S_NoteBook_Data.count[i] = 9.999f;

            float colorX = (S_NoteBook_Data.vertices_world[i].x - S_NoteBook_Data.MinElement_x) / elementsRangeX;
            float colorY = (S_NoteBook_Data.vertices_world[i].y - S_NoteBook_Data.MinElement_y) / elementsRangeY;
            float colorZ = (S_NoteBook_Data.vertices_world[i].z - S_NoteBook_Data.MinElement_z) / elementsRangeZ;
            float colorW = (S_NoteBook_Data.count[i] - S_NoteBook_Data.MinElement_count) / elementsRangeCount;
            format[i] = new Vector4(colorX, colorY, colorZ, colorW);
        }

        return Draw_with_dynamic_array_V4(format, "_DebugName");
    }

    private Texture2D Draw_with_dynamic_array_V4(Vector4[] ConvertArray, string debug_name)
    {
        int point_num = ConvertArray.Length;
        int weight = point_num;
        if (point_num > SYSTEM_MAX_TEXTURE_SIZE)
            weight = SYSTEM_MAX_TEXTURE_SIZE;
        int height = point_num / SYSTEM_MAX_TEXTURE_SIZE + 1;
        Texture2D input = new Texture2D(weight, height, TextureFormat.RGBA32, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < weight; i++)
            {
                if (j * weight + i >= point_num)
                {
                    input.SetPixel(i, j, new Color(0, 0, 0, 0));
                    continue;
                }
                int index = j * weight + i;
                float colorX = ConvertArray[index].x;
                float colorY = ConvertArray[index].y;
                float colorZ = ConvertArray[index].z;
                float colorW = ConvertArray[index].w;
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
        File.WriteAllBytes(dirPath + "S_Model_Pos_Count" + ".png", bytes);*/
        //File.WriteAllBytes(Application.dataPath + "/SaveImages/Image333.png", input.EncodeToPNG());
        ////////////////////////////////////////////////////////////////////

        return input;
    }

}
