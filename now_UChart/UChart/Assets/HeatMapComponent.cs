using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class HeatMapComponent : MonoBehaviour
{
    #region 變數們
    private Material m_material = null;
    public Material material
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
    }
    
    private float influenceRadius = 1.0f;   // 热力影响半径
    #endregion

    private void Start()
    {
        hotSpot.Radius = influenceRadius;
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A))    //開始畫畫
        //{
            shader_dynamic_array();
        //}
    }
    public static Vector4[] elements;

    /****************************************************************************************************************************/
    //傳動態長度的資料，由於HLSL無法動態產生array大小。所以透過傳圖片的方式，將資料存在顏色內，已達成目的。
    //可因為color參數介於0~1，所以這裡之前需記錄最大值和最小值，以便壓縮資料。
    //缺點是範圍太大的話，資料壓縮太多，可能失真。所以可能最後有需要固定說每XXX距離就壓縮成一張圖
    public void shader_dynamic_array()
    {
        elements = hotSpot.HS_Vector_list;
        int count = elements.Length;
        Texture2D input = new Texture2D(count, 1, TextureFormat.RGBA32, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;
        /*
        Color32 xxxColor32Color = new Color(0.1f, 0.2f, 0.3f, 0.4f);    // This would return Value of (25, 51, 76, 102)
        Color xxxColorColor32 = new Color32(25, 51, 76, 102);    // This would return  Value of (0.098, 0.200, 0.298, 0.400)

        Color32 xxxColor32Color_2 = new Color(0.5f, 0.6f, 0.7f, 0.8f);    // This would return Value of (127, 153, 178, 204)
        Color xxxColorColor32_2 = new Color32(127, 153, 178, 204);    // This would return  Value of (0.498, 0.600, 0.698, 0.800)

        Color32 xxxColor32Color_3 = new Color(0.9f, 1, 0, 0);    // This would return Value of (229, 255, 0, 0)
        Color xxxColorColor32_3 = new Color32(229, 255, 0, 0);    // This would return  Value of (0.898, 1.000, 0.000, 0.000)
        */
        for (int i = 0; i < count; i++)
        {
            float colorX = elements[i].x / 10;
            float colorY = elements[i].y / 10;
            float colorZ = elements[i].z / 10;
            float colorW = elements[i].w / 10;
            //Debug.Log("new Color(colorX, colorY, colorZ, colorW) : " + (colorX, colorY, colorZ, colorW));
            input.SetPixel(i, 0, new Color(colorX, colorY, colorZ, colorW));
        }
        input.Apply();

        ////////////////////////////////////////////////////////////////////
        byte[] bytes = input.EncodeToPNG();
        var dirPath = Application.dataPath + "/SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
        ////////////////////////////////////////////////////////////////////

        material.SetTexture("array", input);
        material.SetInt("pixel_count", count);

        material.SetFloat("_Radius", hotSpot.Radius);
        material.SetFloat("_MaxCount", hotSpot.MaxCount);

        //Debug.Log("array : " + input);
        Debug.Log("pixel_count : " + count+";;; _Radius : " + hotSpot.Radius+";;; _MaxCount : " + hotSpot.MaxCount);
    }
}