using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//這個做在新的一個Scene裡，我放在 Scene > Second
public class History : MonoBehaviour
{
    public float time =5;
    public GameObject model;

    private void Update()
    {
        UseHistory(time);
    }

    public void UseHistory(float Time)
    {
        List<time_info> model_history = model.GetComponent<Model_vertex_count_record>().GetHistory();
        //float[] newToGoDraw;
        //model.GetComponent<Model_vertex_count_record>().SetAllVerticeCount(newToGoDraw);
    }


}