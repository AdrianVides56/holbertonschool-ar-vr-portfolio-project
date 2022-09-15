using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public GameObject mainMenuObj;
    public GameObject optionsMenuObj;

    public void StartGame()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    public void EnterSettings()
    {
        mainMenuObj.SetActive(false);
        optionsMenuObj.SetActive(true);
    }
    public void LeaveSettings()
    {
        mainMenuObj.SetActive(true);
        optionsMenuObj.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
