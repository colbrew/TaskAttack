using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour {

    public GameObject resetScreen;
    public AudioSource buttonSound;

    public void ResetPressed()
    {
        buttonSound.Play();
        resetScreen.SetActive(true);
    }

    public void NoPressed()
    {
        buttonSound.Play();
        resetScreen.SetActive(false);
    }
}
