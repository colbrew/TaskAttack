using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseButton : MonoBehaviour {

    public GameObject pauseMenu;
    public AudioSource buttonSound;

    private void Awake()
    {
        pauseMenu.SetActive(false);
    }

    public void ActivatePauseMenu()
    {
        //buttonSound.Play();
        pauseMenu.SetActive(true);
        Player.Paused = true;
        BlurScreen.S.Blur();////blur screen on any type of pause except textinterlude
        Time.timeScale = 0;
        GameManager.S.masterMixer.SetFloat("musicVol", -10f);
        GameManager.S.masterMixer.SetFloat("sfxVol", -10f);
    }

    public void ResumeGame()
    {
        //buttonSound.Play();
        pauseMenu.SetActive(false);
        Player.Paused = false;
        BlurScreen.S.UnBlur();
        Time.timeScale = 1;
        GameManager.S.masterMixer.SetFloat("musicVol", 0);
        GameManager.S.masterMixer.SetFloat("sfxVol", 0);
    }

    public void QuitGame()
    {
        //buttonSound.Play();
        if (!GameManager.S.infinitePlay)
        {
            GameManager.S.dataController.SetHighestLevel(GameManager.S.nextLevel);
        }
        GameManager.S.masterMixer.SetFloat("musicVol", 0);
        GameManager.S.masterMixer.SetFloat("sfxVol", 0);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Destroy(Player.S);
        SceneManager.LoadScene("MainMenu");
    }

}
