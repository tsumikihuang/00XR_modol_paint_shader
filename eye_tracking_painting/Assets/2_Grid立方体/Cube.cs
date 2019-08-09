//https://zhuanlan.zhihu.com/p/23525545
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{

    public int xSize, ySize, zSize;

    private Mesh mesh;
    private Vector3[] vertices;
    Vector2[] uv;

    private void Awake()
    {
        //StartCoroutine(Generate());
        Generate();
    }

    //private IEnumerator Generate()
    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";
        //WaitForSeconds wait = new WaitForSeconds(0.05f);
        CreateVertices();
        CreateTriangles();
        Create_UV_Tangent();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    private void CreateTriangles()
    {
        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;    // Quad 四边形，四邊形上的三角形總數
        int[] triangles = new int[quads * 6];       //共6面四面體

        //計算要畫的三角形
        int ring = (xSize + zSize) * 2;             //周長，第一層到第二層的index差
        int t = 0, v = 0;
        for (int y = 0; y < ySize; y++, v++)        //增加y方向，變成柱狀
        {
            //for (int q = 0; q < xSize; q++, v++)        //只有一行(x)正方形著色，v是畫方形的起始點
            for (int q = 0; q < ring - 1; q++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);    //t是triangles的index，每畫一個方形用掉6個triangles 所以 t 後移6
            }
            t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
        }

        t = CreateTopFace(triangles, t, ring);      //畫上方蓋子
        t = CreateBottomFace(triangles, t, ring);

        mesh.triangles = triangles;
    }

    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)      //左下 右下 左上 右上
    {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

    private void CreateVertices()
    {
        //計算點數
        //单一面所需要的顶点数量为：(#x+1)(#y+1)
        //6个面相加以计算顶点的总数：2((#x+1)(#y+1)+(#x+1)(#z+1)+(#y+1)(#z+1))
        //但這樣算的話，邊緣將被算到2次及頂點被算到3次
        //重新計算

        //頂點數
        int cornerVertices = 8;

        //邊上不包括頂點的點數有 4(#x+#y+#z−3)
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;     // Size+1為邊上點數  Size-1就去掉了兩端頂點 

        //面的内部點數有 2[(#x−1)(#y−1)+(#x−1)(#z−1)+(#y−1)(#z−1)]
        int faceVertices = (
            (xSize - 1) * (ySize - 1) +
            (xSize - 1) * (zSize - 1) +
            (ySize - 1) * (zSize - 1)) * 2;

        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

        //移動點的位置
        int v = 0;
        for (int y = 0; y <= ySize; y++)                //加上y會變柱狀疊高
        {
            for (int x = 0; x <= xSize; x++)            //一個for只畫一個邊
            {
                vertices[v++] = new Vector3(x, y, 0);
                //yield return wait;
            }
            for (int z = 1; z <= zSize; z++)
            {
                vertices[v++] = new Vector3(xSize, y, z);
                //yield return wait;
            }
            for (int x = xSize - 1; x >= 0; x--)
            {
                vertices[v++] = new Vector3(x, y, zSize);
                //yield return wait;
            }
            for (int z = zSize - 1; z > 0; z--)
            {
                vertices[v++] = new Vector3(0, y, z);
                //yield return wait;
            }
        }

        //加上頂蓋及底蓋
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                vertices[v++] = new Vector3(x, ySize, z);
                //yield return wait;
            }
        }
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                vertices[v++] = new Vector3(x, 0, z);
                //yield return wait;
            }
        }
        mesh.vertices = vertices;
    }

    private void Create_UV_Tangent()
    {
        uv = new Vector2[vertices.Length];

        float uv_len = (float)Math.Sqrt((uv.Length));

        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2((float)i / vertices.Length, ((float)i % uv_len) / uv_len);
        }        

        mesh.uv = uv;

    }

    private int CreateTopFace(int[] triangles, int t, int ring)
    {
        #region 上蓋第一層
        int v = ring * ySize;
        for (int x = 0; x < xSize - 1; x++, v++)    //上蓋的第一層，剛剛好可以用上述類似的方式完成
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);
        #endregion

        #region 上蓋中間層
        //頂面的第二行以後
        int vMin = ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;
        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)     //最後一行較特殊，所以只算到最後的前一行
        {
            //第二行第一個方形
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);

            //第二行中間部分
            for (int x = 1; x < xSize - 1; x++, vMid++)
            {
                t = SetQuad(
                    triangles, t,
                    vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
            }

            //第二行最後一個方形
            t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
        }
        #endregion

        #region 上蓋最後一層
        //最後一行
        int vTop = vMin - 2;
        //最後一行的第一個方形
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);

        //最後一行的中間的方形
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        }

        //最後一個方形
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
        #endregion

        return t;
    }

    //就沒有慢慢看了
    private int CreateBottomFace(int[] triangles, int t, int ring)
    {
        int v = 1;
        int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (int x = 1; x < xSize - 1; x++, v++, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

        int vMin = ring - 2;
        vMid -= xSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
            for (int x = 1; x < xSize - 1; x++, vMid++)
            {
                t = SetQuad(
                    triangles, t,
                    vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
        }

        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

        return t;
    }

    private void OnDrawGizmos()
    {
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