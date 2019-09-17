using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowOneBall : MonoBehaviour
{
    public List<Vector4> tempStructureList = new List<Vector4>();   //123存座標 4存count

    Camera cam;
    bool isWaiting = false;

    void Start()
    {
        tempStructureList.Add(new Vector4(o1.position.x, o1.position.y, o1.position.z, 5));
        cam = GetComponent<Camera>();
    }

    private float m_timer = 0.0f;

    public Transform o1;
    void Update()
    {
        tempStructureList[0] = new Vector4(o1.position.x, o1.position.y, o1.position.z, 5);
        CheckMaxSwitch(tempStructureList[0].w, ref hotSpot.MaxCount);
        hotSpot.HS_Vector_list = FormatPointInfo();
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
