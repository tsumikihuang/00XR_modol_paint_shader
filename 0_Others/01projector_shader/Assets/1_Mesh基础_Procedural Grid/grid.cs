//用Mesh畫平面

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class grid : MonoBehaviour
{

    public int xSize, ySize;        //範例中使用10*5
    private Vector3[] vertices;     //mesh中總共用到多少點
    private Mesh mesh;


    private void Awake()
    {
        //StartCoroutine(Generate());

        Generate();
    }

    //private IEnumerator Generate()
    private void Generate()
    {
        //WaitForSeconds wait = new WaitForSeconds(0.5f);    //為了方便觀察，一個一個慢慢改變其位置

        ///賦予Mesh，必須給予三角形後才可見
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";
        ///
        
        vertices = new Vector3[(xSize + 1) * (ySize + 1)];  //xSize、ySize為格數，+1後是點的數量
        Vector2[] uv = new Vector2[vertices.Length];

        //Grid并不会因为我们将贴图应用到材质上就立刻产生崎岖不平的表面效果，还需要为Grid添加上切线向量。
        //切线向量，因为我們的Mesh是平面，所以所有的切線向量指向同一个方向，也就是右方
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                //為每一點賦予位置(0,0)到(xSize,ySize)
                vertices[i] = new Vector3(x, y);
                uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                tangents[i] = tangent;
            }
        }
        //賦予Mesh vertices
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;

        //畫三角形~
        //Unity設定，在世界空間、模型空間下，使用左手坐標系
        //在觀察空間下，使用右手坐標系
        int[] triangles = new int[xSize * ySize * 6]; ;   //triangles裡面放索引，一個正方形要6個點來畫

        //vi 第一個點的index，由於是水平畫，所以剛剛好是0 1 ... 9(第一行) 11 12 ... 21(第二行)
        //ti triangles的index
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)   //第二行時vi從11開始
        {
            //x  總共要畫xSize個正方形(一個迴圈畫一個正方形)
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
                //mesh.triangles = triangles;
                //yield return wait;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    

    //在編輯模式也會執行OnDrawGizmos()
    private void OnDrawGizmos()
    {
        //編輯模式的時候vertices為空，所以會跳出錯誤訊息。所以進一步判斷
        if (vertices == null)
        {
            return;
        }

        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }

    }

}