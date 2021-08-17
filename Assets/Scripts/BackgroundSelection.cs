using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundSelection : MonoBehaviour
{
    //public List<Button> background = new List<Button>();
    public List<Sprite> bgImages = new List<Sprite>();
    public Image newBackground;
    public Button selection;
    public GameObject currentBackground; 
   
    // utilized based on button click. If button is pressed, the background will change based on button pressed

    public void CheckImageChoice(int number)
    {
        

        switch (number)
        {

            //if the button pressed is Desert Oasis: display
            case 0:
                newBackground.GetComponent<Image>().sprite = Resources.Load<Sprite>("Desert Oasis");
                break;
                
            //if the button pressed is Galactic Skies: display
            case 1:
                newBackground.GetComponent<Image>().sprite = Resources.Load<Sprite>("Galactic Skies");
                break;

            //if the button pressed is Medieval era: display
            case 2:
                newBackground.GetComponent<Image>().sprite = newBackground.sprite = Resources.Load<Sprite>("Medieval Era");
                break;

            //if the button pressed is Misty Forest: display
            case 3:
                newBackground.GetComponent<Image>().sprite = Resources.Load<Sprite>("Misty Forest 1");
                break;

            //if the button pressed is Oceanic Views: display
            case 4:
                newBackground.GetComponent<Image>().sprite = Resources.Load<Sprite>("Oceanic View");
                break;

            //if the button pressed is Rainbow Gradient: display
            case 5:
                
                newBackground.GetComponent<Image>().sprite = Resources.Load<Sprite>("Rainbow Gradient");
                break;

            //if the button pressed is Starry Skies: display
            case 6:
                newBackground.GetComponent<Image>().sprite = Resources.Load<Sprite>("Starry Skies");
                break;

            //if the button pressed is Tranquil Views: display
            case 7:
                newBackground.GetComponent<Image>().sprite = Resources.Load<Sprite>("Tranquil View");
                break;

            //if the button pressed is Under The Sea: display
            case 8:
                newBackground.GetComponent<Image>().sprite = Resources.Load<Sprite>("Under The Sea");
                break;

            //if the button pressed is Vibrant Forest: display
            case 9:
                newBackground.GetComponent<Image>().sprite = Resources.Load<Sprite>("Vibrant Forest");
                break;
        }
    }
    public void Back()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void SubmitChangedImage()
    {
        currentBackground.GetComponent<SpriteRenderer>().sprite = newBackground.sprite;
    }
}

