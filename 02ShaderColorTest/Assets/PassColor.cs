using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PassColor : MonoBehaviour
{
    private Material m_material = null;
    public Material material
    {
        get
        {
            if (null == m_material)
            {
                Renderer render = this.GetComponent<Renderer>();
                if (null == render)
                    Debug.LogError("Can not found Renderer component");
                m_material = render.material;
            }
            return m_material;
        }
    }

    private void Start()
    {
        int count = 4;
        material.SetInt("pixel_count", count);

        Texture2D input = new Texture2D(count, 1, TextureFormat.RGBA32, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;


        float colorX = 0.2f;
        float colorY = 0.4f;
        float colorZ = 0.0f;
        float colorW = 1.0f;

        input.SetPixel(1, 0, new Color(colorX, colorY, colorZ, colorW));

        colorX = 0.0f;
        colorY = 0.0f;
        colorZ = 0.0f;
        colorW = 0.0f;

        input.SetPixel(1, 0, new Color(colorX, colorY, colorZ, colorW));

        colorX = 0.2f;
        colorY = 0.4f;
        colorZ = 0.0f;
        colorW = 0.0f;

        input.SetPixel(1, 0, new Color(colorX, colorY, colorZ, colorW));

        colorX = 0.0f;
        colorY = 0.0f;
        colorZ = 5.0f;
        colorW = 0.0f;

        input.SetPixel(1, 0, new Color(colorX, colorY, colorZ, colorW));

        input.Apply();

        material.SetTexture("array", input);

        //////////////////////////////////////////////////////////////////// 方便觀看，非必要，可刪除
        byte[] bytes = input.EncodeToPNG();
        var dirPath = Application.dataPath;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "/Image" + ".png", bytes);
        ////////////////////////////////////////////////////////////////////
    }

    void Update()
    {
        int count = 4;
        material.SetInt("pixel_count", count);

        Texture2D input = new Texture2D(count, 1, TextureFormat.RGBA32, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;


        float colorX = 0.2f;
        float colorY = 0.4f;
        float colorZ = 0.0f;
        float colorW = 1.0f;

        input.SetPixel(0, 0, new Color(colorX, colorY, colorZ, colorW));

        colorX = 0.0f;
        colorY = 0.0f;
        colorZ = 0.0f;
        colorW = 0.0f;

        input.SetPixel(1, 0, new Color(colorX, colorY, colorZ, colorW));

        colorX = 0.2f;
        colorY = 0.4f;
        colorZ = 0.0f;
        colorW = 0.0f;

        input.SetPixel(2, 0, new Color(colorX, colorY, colorZ, colorW));

        colorX = 0.0f;
        colorY = 0.0f;
        colorZ = 5.0f;
        colorW = 0.0f;

        input.SetPixel(3, 0, new Color(colorX, colorY, colorZ, colorW));

        input.Apply();

        material.SetTexture("array", input);
    }

}
