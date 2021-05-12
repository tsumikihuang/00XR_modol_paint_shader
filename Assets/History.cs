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
    public static float totalTime;
    public static History instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    Simple_Data result_noteBook;
    List<time_info> model_history;
    private void Start()
    {
        result_noteBook = model.transform.Find("bunny_500").gameObject.GetComponent<SimpleModel>().Get_S_NoteBook().m_Data;
        model_history = result_noteBook.History;
        for (int i = 0; i < model_history.Count; i++)
        {
            totalTime += model_history[i].delta_time;
        }
    }


    public void UseHistory(float Time)
    {
        result_noteBook.count=model_history[0].all_vertice_count;
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

        result_noteBook.count = model_history[id].all_vertice_count;
        result_noteBook.hey_need_update = true;

    }
}