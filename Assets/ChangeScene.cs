using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ChangeScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Button myButton = GetComponent<Button>();
        GetComponent<Button>().onClick.AddListener(() => {
            OnClicked();
        });
    }

    public void OnClicked()
    {
        if(this.name== "Button1")
            SceneManager.LoadScene("First");
        else if (this.name == "Button2")
            SceneManager.LoadScene("Second");
        else if (this.name == "Button3")
            SceneManager.LoadScene("Second_Button");
    }
}
