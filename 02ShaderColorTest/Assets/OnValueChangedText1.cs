using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class OnValueChangedText1 : MonoBehaviour
{
    public static Text ValueText;

    // Start is called before the first frame update
    public void Start()
    {
        ValueText = GetComponent<Text>();
    }

    // Update is called once per frame
    public void OnSliderValueChanged(float value)
    {
        ValueText.text = value.ToString("0.00");//取到小數第二位
    }
}
