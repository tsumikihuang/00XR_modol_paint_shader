using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Raycast : MonoBehaviour
{
    //還需紀錄該點的時間
    //可是如果要記時間的話，相同位置的點的次數就不能合併在一起了!! >> 會多消耗一些效能，EX.shader讀取的資料數增加
    //不過實務上，並不需要即時繪圖，只需將資料儲存。最後用另一場景(畫面)將資料視覺化，EX.在不同時間、空間區間內，顯示相對應的熱點
    //透過空白鍵，開始偵測(錄製)視線產生熱點。再按一次空白鍵，停止偵測(錄製)。
    //按下"視覺化"按鈕，進入資料視覺化畫面(寫於StateBoard.cs)
    
    public List<Vector4> tempStructureList = new List<Vector4>();

    Camera cam;

    void Start()
    {
        hotSpot.HS_Vector_list = FormatPointInfo();
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if(StateBoard.recordMode)
            Paint();
    }

    void Paint()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        Vector3 p = hit.point;

        Vector3 new_p = new Vector3((float)Math.Round(p.x, 1), (float)Math.Round(p.y, 1), (float)Math.Round(p.z, 1));
        AddCountInList(new_p);

        hotSpot.HS_Vector_list = FormatPointInfo();
    }

    void AddCountInList(Vector4 p)
    {
        int imatch = tempStructureList.FindIndex(x => x.x == p.x && x.y == p.y && x.z == p.z);

        if (imatch != -1)
            tempStructureList[imatch] = new Vector4(tempStructureList[imatch].x, tempStructureList[imatch].y, tempStructureList[imatch].z, tempStructureList[imatch].w + 1);
        else
        {
            tempStructureList.Add(new Vector4(p.x, p.y, p.z, 1));
            imatch = tempStructureList.Count - 1;
        }

        CheckMaxSwitch(tempStructureList[imatch].w, ref hotSpot.MaxCount);
    }

    void CheckMaxSwitch(float Temp, ref float MaxValue)
    {
        if (Temp > MaxValue)
            MaxValue = Temp;
    }

    Vector4[] FormatPointInfo()
    {
        Vector4[] list_temp = tempStructureList.ToArray();
        Vector4[] ans = new Vector4[list_temp.Length * 4];

        for (int i = 0; i < list_temp.Length; i++)
        {
            ///x
            ans[i * 4].w = 5;
            if (list_temp[i].x < 0)
            {
                list_temp[i].x *= (-1);
                ans[i * 4].w = 10;
            }
            ans[i * 4].x = (int)list_temp[i].x / 10;          //x座標的十位數
            ans[i * 4].y = (int)list_temp[i].x % 10;          //x座標的個位數
            ans[i * 4].z = (int)(list_temp[i].x * 10) % 10;     //x座標的小數後一位

            ///y
            ans[i * 4 + 1].w = 5;
            if (list_temp[i].y < 0)
            {
                list_temp[i].y *= (-1);
                ans[i * 4 + 1].w = 10;
            }
            ans[i * 4 + 1].x = (int)list_temp[i].y / 10;
            ans[i * 4 + 1].y = (int)list_temp[i].y % 10;
            ans[i * 4 + 1].z = (int)(list_temp[i].y * 10) % 10;

            ///z
            ans[i * 4 + 2].w = 5;
            if (list_temp[i].z < 0)
            {
                list_temp[i].z *= (-1);
                ans[i * 4 + 2].w = 10;
            }
            ans[i * 4 + 2].x = (int)list_temp[i].z / 10;
            ans[i * 4 + 2].y = (int)list_temp[i].z % 10;
            ans[i * 4 + 2].z = (int)(list_temp[i].z * 10) % 10;

            ///w
            ans[i * 4 + 3].x = (int)list_temp[i].w / 1000;
            ans[i * 4 + 3].y = (int)list_temp[i].w % 1000 / 100;
            ans[i * 4 + 3].z = (int)list_temp[i].w % 100 / 10;
            ans[i * 4 + 3].w = (int)list_temp[i].w % 10;
        }

        return ans;
    }
}
