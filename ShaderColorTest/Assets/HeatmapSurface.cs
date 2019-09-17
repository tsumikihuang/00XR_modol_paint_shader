using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HeatmapSurface : MonoBehaviour
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
        shader_dynamic_array();
    }

    public static Vector4[] elements;

    public void shader_dynamic_array()
    {
        if (hotSpot.HS_Vector_list == null)
            return;
        elements = hotSpot.HS_Vector_list;
        int count = elements.Length;
        Texture2D input = new Texture2D(count, 1, TextureFormat.RGBA32, false);
        input.filterMode = FilterMode.Point;
        input.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < count; i++)
        {
            float colorX = elements[i].x / 10.0f;
            float colorY = elements[i].y / 10.0f;
            float colorZ = elements[i].z / 10.0f;
            float colorW = elements[i].w / 10.0f;
            input.SetPixel(i, 0, new Color(colorX, colorY, colorZ, colorW));
        }
        input.Apply();

        
        material.SetInt("pixel_count", count);
        material.SetFloat("_Radius", hotSpot.Radius);
        material.SetFloat("_MaxCount", hotSpot.MaxCount);
        material.SetTexture("array", input);
    }

}