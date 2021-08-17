using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    //create an instance of the slider that will be used for volume control
    [SerializeField] Slider volumeControl;
    
    // Start is called before the first frame update
    void Start()
    {
        //if there is a slider present and the player wants to adjust the sound, then set it's maximum value to 1
        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            
            PlayerPrefs.SetFloat("musicVolume", 1);
            Load();
        }
        //get the volume of the sound
        else
        {
            Load();
        }
    }

    //adjusts as the user is sliding the  knob up and down
    public void VolumeChange()
    {
        AudioListener.volume = volumeControl.value;
        Save();

    }

    //gives the current volume of the music slider

    private void Load()
    {
        volumeControl.value = PlayerPrefs.GetFloat("musicVolume");
    }

    //saves tge positon of the handle on slider
    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", volumeControl.value);
    }
 
}
