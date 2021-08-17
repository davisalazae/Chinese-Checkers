using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject canvas;
    bool gamePaused;
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            gamePaused = !gamePaused;           
        }

        if(gamePaused)
        {
            canvas.gameObject.SetActive(true);
        }
        else
        {
            canvas.gameObject.SetActive(false);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene("Menu");
    }

    public void ToOptionsMenu()
    {
        SceneManager.LoadScene("OptionsMenu", LoadSceneMode.Additive);
    }

}
