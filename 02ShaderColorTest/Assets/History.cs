using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//這個做在新的一個Scene裡，我放在 Scene > Second
[RequireComponent(typeof(Text))]
public class History : MonoBehaviour
{
    public GameObject model;

    public static History instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    Model_vertex_count_record result_noteBook;
    List<time_info> model_history;
    private void Start()
    {
        result_noteBook = model.transform.Find("gargoyle_simple").gameObject.GetComponent<Model_vertex_count_record>();
        model_history = result_noteBook.GetHistory();

    }


    public void UseHistory(float Time)
    {
        result_noteBook.SetAllVerticeCount(model_history[0].all_vertice_count);
        
        float END_Time = 0;
        int id = model_history.Count-1;
        for (int i = 0; i < model_history.Count; i++)
        {
            END_Time += model_history[i].delta_time;
            if (END_Time > Time)
            {
                id = i;
                break;
            }
        }
        Debug.Log(id);
        result_noteBook.SetAllVerticeCount(model_history[id].all_vertice_count);
        result_noteBook.NewChange();

        /*if ((float)Convert.ToDouble(OnValueChangedText.ValueText.text)!= select_Time && select_Time<=END_Time)
        {
            select_Time = (float)Convert.ToDouble(OnValueChangedText.ValueText.text);//若timeline選定成不同時間
            Debug.Log(OnValueChangedText.ValueText.text);
            float cumTime = 0;
            for (int j = 0; j < model_history.Count; j++)//尋找選定時間的history
            {
                cumTime += model_history[j].delta_time;
                //if(cumTime)//如果

            }
        }*/

    }
}