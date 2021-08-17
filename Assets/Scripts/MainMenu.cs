using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   public  void PlayGame()
    {
        Globals.won = "";
        SceneManager.LoadScene("PlayerSelect");
    }
    
   /* public void NextScene()
    {
        SceneManager.LoadScene("SampleScene");
    } */
    public void NextScene(string sceneName)
    {
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        //SceneManager.LoadScene(sceneName);

        switch (sceneName)
        {
            case "2 Players":
                Globals.NumberOfPlayers = 2;
                SceneManager.LoadScene("SampleScene");
                break;
            case "4 Players":
                Globals.NumberOfPlayers = 4;
                SceneManager.LoadScene("SampleScene");
                //currentRoom = SceneManager.GetSceneByName("Lobby");
                break;
            case "6 Players":
                Globals.NumberOfPlayers = 6;
                SceneManager.LoadScene("SampleScene");
                break;
        }
    }


    public void Back()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }



    public void QuitGame()
    {
        Debug.Log("quit");
        Application.Quit();
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }

    public void BacktoGameScene()
    {
        SceneManager.UnloadSceneAsync("SampleScene");
    } 
}
