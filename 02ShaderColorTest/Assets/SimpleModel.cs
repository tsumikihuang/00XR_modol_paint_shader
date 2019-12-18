//附在每一個物件上
//當raycast點到某物件時，就會呼叫此物件的NewChange()
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//簡化後模型
//把自己頂點的count資料存起來
[RequireComponent(typeof(MeshCollider))]
public class SimpleModel : MonoBehaviour
{
    private SimpleModelRecord S_NoteBook;      //儲存模型資料的 ScriptableObject
    public string ModelName;

    #region material
    //private Material material;
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
        Init_S_NoteBook();
    }

    //清空所有值
    private void Init_S_NoteBook()
    {
        S_NoteBook = Resources.Load<SimpleModelRecord>("SimpleModelRecord/" + name);
        if (S_NoteBook == null)
        {
            SimpleModelRecord NewFile = new SimpleModelRecord();
            AssetDatabase.CreateAsset(NewFile, "Assets/Resources/SimpleModelRecord/" + name + ".asset");
            //Debug.LogError("這個model沒有自己的SimpleModelRecord檔案(必須放在Resources/SimpleModelRecord資料夾下，且檔案名稱與物件名稱相同)");
            Debug.LogWarning("以自動建立一個OrignModelRecord檔案>>" + name + ".asset" + "，放在Resources/SimpleModelRecord資料夾下");
            S_NoteBook = Resources.Load<SimpleModelRecord>("SimpleModelRecord/" + name);
        }

        //裡面已經有資料
        if (S_NoteBook.m_Data.Model_Name != "")
            return;

        //name
        S_NoteBook.m_Data.Model_Name = name;

        MeshCollider S_Collider = GetComponent<MeshCollider>();
        if (S_Collider == null || S_Collider.sharedMesh == null)
        {
            Debug.LogError("[Simple Model] There is no meshCollider or sharedMesh");
            return;
        }

        //vertices_local
        S_NoteBook.m_Data.vertices_local = S_Collider.sharedMesh.vertices;

        //normals_local
        S_NoteBook.m_Data.vertices_normal = S_Collider.sharedMesh.normals;

        //number_of_vertices
        int len = S_NoteBook.m_Data.number_of_vertices = S_Collider.sharedMesh.vertexCount;

        //vertices_world
        S_NoteBook.m_Data.vertices_world = new Vector3[len];

        //reset history
        S_NoteBook.m_Data.History = new List<time_info>();

        //count
        S_NoteBook.m_Data.count = new float[len];
        Init_Count();

        //Init Record vertices
        for (int i = 0; i < len; i++)
            S_NoteBook.m_Data.vertices_world[i] = GetComponent<Collider>().transform.TransformPoint(S_NoteBook.m_Data.vertices_local[i]);
    }

    public void Init_Count()
    {
        //Init Record vertices
        for (int i = 0; i < S_NoteBook.m_Data.count.Length; i++)
            S_NoteBook.m_Data.count[i] = 0;
        S_NoteBook.m_Data.hey_need_update = true;
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public SimpleModelRecord Get_S_NoteBook()
    {
        return S_NoteBook;
    }

    //取得所有count!!!!!!
    public float[] GetAllVerticeCount()
    {
        return S_NoteBook.m_Data.count;
    }

    //設置所有count!!!!!!
    public void SetAllVerticeCount(float[] inputCount)
    {
        S_NoteBook.m_Data.count = inputCount;
    }

    //新增time line
    public void AddHistory(time_info info)
    {
        S_NoteBook.m_Data.History.Add(info);
    }

    public List<time_info> GetHistory()
    {
        return S_NoteBook.m_Data.History;
    }

}