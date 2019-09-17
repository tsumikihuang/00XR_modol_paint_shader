using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
}
