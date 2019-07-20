//原文：https://blog.csdn.net/tom_221x/article/details/61920213 

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public  class Mesh2Obj: MonoBehaviour
{
    public string datPath= "Assets/";
    public GameObject meshGO;//I GUESS...
    public MeshFilter meshF;
    public string projectPath= "Assets/";

    private void Start()
    {
        using (StreamWriter streamWriter = new StreamWriter(string.Format("{0}{1}.obj", datPath, this.meshGO.name)))
        {
            /*
             * Unity加载obj文件的时候，顶点的X轴是翻转的
             * 所以，在写入数据的时候，我们把scale.x设置为-1, 翻转X轴。
             * 正常情况下Mesh的顶点索引就是Triangles，需要逆时针才不会被摄像机剔除。
             * 当这里我们翻转了X顶点，同步我们需要在生成Mesh Triangles的时候，使用顺时针排列。
             * 翻转X轴以后，对摄像机来说，顶点索引又是逆时针排列的了，就可以看见了
             */
            streamWriter.Write(MeshToString(meshF, new Vector3(-1f, 1f, 1f)));
            streamWriter.Close();
        }

        /*
         * 生成了obj模型文件以后，我们可以通过这个文件加载一个Mesh对象。动态生成一个Prefab到本地，把obj模型文件中的Mesh对象赋值给它，成为一个正确加载Mesh的Prefab。
         */
        // create prefab
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(string.Format("{0}{1}.obj", projectPath, this.meshGO.name));
        meshF.mesh = mesh;

        PrefabUtility.CreatePrefab(string.Format("{0}{1}.prefab", projectPath, this.meshGO.name), this.meshGO);
        
        AssetDatabase.Refresh();

    }


    public  string MeshToString(MeshFilter mf, Vector3 scale)
    {
        Mesh mesh = mf.mesh;
        Material[] sharedMaterials = mf.GetComponent<Renderer>().sharedMaterials;
        Vector2 textureOffset = mf.GetComponent<Renderer>().material.GetTextureOffset("_MainTex");
        Vector2 textureScale = mf.GetComponent<Renderer>().material.GetTextureScale("_MainTex");

        StringBuilder stringBuilder = new StringBuilder().Append("mtllib design.mtl")
            .Append("\n")
            .Append("g ")
            .Append(mf.name)
            .Append("\n");

        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vector = vertices[i];
            stringBuilder.Append(string.Format("v {0} {1} {2}\n", vector.x * scale.x, vector.y * scale.y, vector.z * scale.z));
        }

        stringBuilder.Append("\n");

        Dictionary<int, int> dictionary = new Dictionary<int, int>();

        if (mesh.subMeshCount > 1)
        {
            int[] triangles = mesh.GetTriangles(1);

            for (int j = 0; j < triangles.Length; j += 3)
            {
                if (!dictionary.ContainsKey(triangles[j]))
                {
                    dictionary.Add(triangles[j], 1);
                }

                if (!dictionary.ContainsKey(triangles[j + 1]))
                {
                    dictionary.Add(triangles[j + 1], 1);
                }

                if (!dictionary.ContainsKey(triangles[j + 2]))
                {
                    dictionary.Add(triangles[j + 2], 1);
                }
            }
        }

        for (int num = 0; num != mesh.uv.Length; num++)
        {
            Vector2 vector2 = Vector2.Scale(mesh.uv[num], textureScale) + textureOffset;

            if (dictionary.ContainsKey(num))
            {
                stringBuilder.Append(string.Format("vt {0} {1}\n", mesh.uv[num].x, mesh.uv[num].y));
            }
            else
            {
                stringBuilder.Append(string.Format("vt {0} {1}\n", vector2.x, vector2.y));
            }
        }

        for (int k = 0; k < mesh.subMeshCount; k++)
        {
            stringBuilder.Append("\n");

            if (k == 0)
            {
                stringBuilder.Append("usemtl ").Append("Material_design").Append("\n");
            }

            if (k == 1)
            {
                stringBuilder.Append("usemtl ").Append("Material_logo").Append("\n");
            }

            int[] triangles2 = mesh.GetTriangles(k);

            for (int l = 0; l < triangles2.Length; l += 3)
            {
                stringBuilder.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", triangles2[l] + 1, triangles2[l + 2] + 1, triangles2[l + 1] + 1));
            }
        }

        return stringBuilder.ToString();
    }

}
