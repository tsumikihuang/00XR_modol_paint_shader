using UnityEngine;
using System.Collections;

public class putColor : MonoBehaviour
{

    public Color color;
    public Color color_00;
    public Color color_01;
    public Color color_02;

    // Use this for initialization
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        Color[] nc = new Color[mf.mesh.vertices.Length];

        for (int i = 0; i < nc.Length/2; i++)
        {
            nc[i] = color;
        }

        nc[14] = color_00;
        nc[15] = color_01;
        nc[16] = color_02;

        Debug.Log("mf.mesh.vertices.Length" + mf.mesh.vertices.Length);
        mf.mesh.colors = nc;

    }

    // Update is called once per frame
    void Update()
    {

    }
}