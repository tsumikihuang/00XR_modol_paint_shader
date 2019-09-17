using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
視覺化畫面，可以選擇...

    口 模型(顯示或不顯示熱點)                     << 模型賦予該Shader，每個物體的熱點顯示與否buttom
    口 時間區間(ex. 0~30秒)                       << if判斷
    口 只顯現count數大於某數值                    << if判斷
    口 是否於熱點處放置球體(僅於整數位置)         << 放物體前將座標簡化，四捨五入，並檢查不要重複放
    口 球體/平面，熱點查看，可放大縮小位移

 */

/*
現階段動作步驟
     
    - 前面存的資料須改成 >> 秒數 + 熱點位置 + 次數
    - 傳array給shader前，須將相同位置的點合併(用CPU減少GPU的工作)
    

*/
public class StateBoard : MonoBehaviour
{
    void Start()
    {
        
    }
    public Text HotSpot_Num;
    void Update()
    {
        HotSpot_Num.text = "" + hotSpot.HS_Vector_list.Length;
    }

    public GameObject SphereSize;
    public void OnSliderValueChanged(float value)
    {
        value = value*100;
        SphereSize.transform.localScale=new Vector3(value, value, value);
    }
}
