using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutButton : MonoBehaviour {

    public GameObject aboutScreen;
    public AudioSource buttonSound;

    public void ButtonPressed()
    {
        //buttonSound.Play();
        aboutScreen.SetActive(true);
    }

    public void BackPressed()
    {
        //buttonSound.Play();
        aboutScreen.SetActive(false);
    }
}
