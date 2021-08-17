using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Music controller for music menu buttons to change what audio source is currently active
public class MusicController : MonoBehaviour
{
   
    // Audio sources to be played

    [SerializeField] AudioSource track1;
    [SerializeField] AudioSource track2;
    [SerializeField] AudioSource track3;
    [SerializeField] AudioSource track4;
    [SerializeField] AudioSource track5;
    [SerializeField] AudioSource track6;
    [SerializeField] AudioSource track7;
    [SerializeField] AudioSource track8;
    [SerializeField] AudioSource track9;
    [SerializeField] AudioSource track10;
    [SerializeField] AudioSource track11;
    [SerializeField] AudioSource track12;
    [SerializeField] AudioSource track13;
    [SerializeField] AudioSource track14;
    [SerializeField] AudioSource track15;
    [SerializeField] AudioSource track16;
    [SerializeField] AudioSource track17;
    [SerializeField] AudioSource track18;
    [SerializeField] AudioSource track19;
    [SerializeField] AudioSource track20;

    // Pause/Play Button
    [SerializeField] Button pausePlayButton;

    // Public functions for music menu buttons to run

    public void PlayTrack1()
    {
        PlayTrack(track1);
    }

    public void PlayTrack2()
    {
        PlayTrack(track2);
    }

    public void PlayTrack3()
    {
        PlayTrack(track3);
    }

    public void PlayTrack4()
    {
        PlayTrack(track4);
    }

    public void PlayTrack5()
    {
        PlayTrack(track5);
    }

    public void PlayTrack6()
    {
        PlayTrack(track6);
    }

    public void PlayTrack7()
    {
        PlayTrack(track7);
    }

    public void PlayTrack8()
    {
        PlayTrack(track8);
    }

    public void PlayTrack9()
    {
        PlayTrack(track9);
    }

    public void PlayTrack10()
    {
        PlayTrack(track10);
    }

    public void PlayTrack11()
    {
        PlayTrack(track11);
    }

    public void PlayTrack12()
    {
        PlayTrack(track12);
    }

    public void PlayTrack13()
    {
        PlayTrack(track13);
    }

    public void PlayTrack14()
    {
        PlayTrack(track14);
    }

    public void PlayTrack15()
    {
        PlayTrack(track15);
    }

    public void PlayTrack16()
    {
        PlayTrack(track16);
    }

    public void PlayTrack17()
    {
        PlayTrack(track17);
    }

    public void PlayTrack18()
    {
        PlayTrack(track18);
    }

    public void PlayTrack19()
    {
        PlayTrack(track19);
    }

    public void PlayTrack20()
    {
        PlayTrack(track20);
    }

    // Function to play a track
    private void PlayTrack(AudioSource track)
    {
        // Checks if there is an active audio source to effect before performing any commands on it
        if (FindObjectOfType<AudioSource>() != null)
        {
            // Deactivates the prior audio source that was active
            FindObjectOfType<AudioSource>().gameObject.SetActive(false);

            // Activates track
            track.gameObject.SetActive(true);

            // Plays track from the beginning
            track.GetComponent<AudioSource>().Play(0);


            //Harrison Stokes added 
            //Changes the play/pause text back to pause when a new track is picked 
            pausePlayButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pause";
            FindObjectOfType<AudioSource>().UnPause();

        }
    }

    // Function to pause/play a track
    public void PausePlayTrack()
    {
        // If an active audio source is playing, this function will pause it. Otherwise, it will play it.
        // The text of the button utilizing this function switches between "Pause" and "Play" accordingly.
        if (FindObjectOfType<AudioSource>() != null)
        {
            if (FindObjectOfType<AudioSource>().isPlaying)
            {
                pausePlayButton.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
                FindObjectOfType<AudioSource>().Pause();
            }
            else
            {
                pausePlayButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pause";
                FindObjectOfType<AudioSource>().UnPause();
            }
        }
    }
}