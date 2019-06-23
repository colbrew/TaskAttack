using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownAudio : MonoBehaviour {
    
    public AudioSource buttonSound;

    public void SelectedDifficultySound()
    {
        buttonSound.Play();
    }
}
