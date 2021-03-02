using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HotKeys : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            var music = GameObject.Find("Music");
            Destroy(music);
            SceneManager.LoadScene("_Prospector_Menu");
        }
    }
}
