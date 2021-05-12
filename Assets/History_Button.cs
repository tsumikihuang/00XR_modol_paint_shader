using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//這個做在新的一個Scene裡，我放在 Scene > Second
[RequireComponent(typeof(Text))]
public class History_Button : MonoBehaviour
{
    public GameObject model;

    public static History_Button instance;

    public static float totalTime;

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
        for (int i=0; i< model_history.Count; i++)
        {
            totalTime += model_history[i].delta_time;
        }
    }


    public void UseHistory()
    {
        float Time1, Time2, Time, Range;
        Single.TryParse(OnValueChangedText1.ValueText.text, out Time);
        Single.TryParse(OnValueChangedText2.ValueText.text, out Range);
        Debug.Log("Time :" + Time);
        Time1 = Time - Range;
        if (Time1 < 0)
            Time1 = 0;
        Debug.Log("Time1 :"+Time1);
        Time2 = Time + Range;
        Debug.Log("History.totalTime :" + History.totalTime);
        if (Time2 > totalTime)
            Time2 = totalTime;
        Debug.Log("Time2 :" + Time2);
        float END_Time1 = 0, END_Time2 = 0, END_Time = 0;
        int id1, id2, id;
        id1 = id2 = id = 0;
        for (int i = 0; i < model_history.Count; i++)
        {
            END_Time1 += model_history[i].delta_time;
            if (END_Time1 > Time1)
            {
                if (id1 == 0)
                    id1 = i;
            }
            END_Time2 += model_history[i].delta_time;
            if (END_Time2 > Time2)
            {
                if (id2 == 0)
                    id2 = i;
            }
            END_Time += model_history[i].delta_time;
            if (END_Time > Time)
            {
                if (id == 0)
                    id = i;
            }
            if (id1 != 0 && id2 != 0 && id != 0)
                break;
           }
        Debug.Log("Time1: " + Time1 + "/Time2: " + Time2 + "/Time: " + Time + "/Range: " + Range);
        Debug.Log("id1: " + id1 + "/id2: " + id2 + "/id: " + id);
        float[] vertice_count=new float[result_noteBook.number_of_vertices];
        //float[] vertice_count=result_noteBook.GetAllVerticeCount();

        for (int i = 0; i < vertice_count.Length; i++)
            vertice_count[i] = 0;

        for (int i=id1; i<=id2; i++)
        {
            for (int j = 0; j < result_noteBook.number_of_vertices; j++) {
                vertice_count[j] += model_history[i].delta_vertice_count[j];
            }
        }
        result_noteBook.count = vertice_count;
        result_noteBook.hey_need_update = true;

    }
}