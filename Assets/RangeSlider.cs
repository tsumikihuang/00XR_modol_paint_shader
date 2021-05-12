using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangeSlider : MonoBehaviour
{
    public Slider mainSlider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (mainSlider.name == "Slider")
        {
            mainSlider.minValue = History_Range.minTime;
            mainSlider.maxValue = History_Range.MaxTime;
        }
        else if(mainSlider.name == "Time_Slider")
        {
            mainSlider.maxValue = History.totalTime;
        }
        else if(mainSlider.name=="TimePick")
        {
            mainSlider.maxValue = History_Button.totalTime;
        }
        else if (mainSlider.name =="RangePick")
        {
            mainSlider.maxValue = History_Button.totalTime/2;
        }
        else
        {
            mainSlider.maxValue = History_Range.totalTime;
            //Debug.Log(mainSlider.name + ": " + mainSlider.maxValue);
        }
        //Debug.Log("minValue:" + mainSlider.minValue + "/maxValue:" + mainSlider.maxValue);
    }
}
