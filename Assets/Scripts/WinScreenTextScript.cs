using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenTextScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!Globals.won.Equals("blue")) {
            GetComponent<Text>().text = "You lost.\nClick Retry to try again.";
        }
    }
}
