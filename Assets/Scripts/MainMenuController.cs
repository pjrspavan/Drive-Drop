using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuControllwe : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {

        if (Input.anyKeyDown)
        {
            PlayerPrefs.SetInt("balance",0);
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
        }
    }
}
