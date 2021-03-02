using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject UIMenu;
    public GameObject UICredits;
    public GameObject UIOption;
    public AudioSource music;



    /// <summary>
    /// Запуск игры
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
        DontDestroyOnLoad(music);
        
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void ShowCredits()
    {
        UIMenu.SetActive(false);
        UICredits.SetActive(true);
    }

    public void HideCredits()
    {
        UIMenu.SetActive(true);
        UICredits.SetActive(false);
    }

    public void ShowOption()
    {
        UIMenu.SetActive(false);
        UIOption.SetActive(true);
    }

    public void HideOption()
    {
        UIMenu.SetActive(true);
        UIOption.SetActive(false);
    }

}
