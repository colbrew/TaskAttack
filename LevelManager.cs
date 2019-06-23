using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    
    int waveNumber = 1;
    public int wavesInLevel;
    public float delayBetweenWaves = 2;
    public float levelPowerupChance = 1;
    public float levelEnemyFireChance = 0;
    bool startingNextWave = false;
    bool stillActive = true;

    void Awake()
    {
        // turn off all children (the wave paths)
        for (int j = 0; j < transform.childCount; j++)
        {
            transform.GetChild(j).gameObject.SetActive(false);

        }

        if (wavesInLevel > 0)
        {
            // Start the first wave
            StartWave();
        }
    }
    
    // Update is called once per frame
    void Update () {
        if (wavesInLevel > 0) // check if this is infinite level with "0" waves
        {
            // start assuming the wave is over
            if (!startingNextWave)
            {
                stillActive = false;

                // check to see if any of the current wave paths are still active
                for (int j = 0; j < transform.childCount; j++)
                {
                    if (transform.GetChild(j).tag == "Wave " + waveNumber.ToString())
                    {
                        if (transform.GetChild(j).gameObject.activeInHierarchy)
                        {
                            stillActive = true;
                        }
                    }
                }
            }

            // if none of current wave paths are still active, start the next wave
            if (!stillActive)
            {
                stillActive = true;
                startingNextWave = true;
                waveNumber++;
                Invoke("StartWave", delayBetweenWaves);
            }
        }
    }

    void StartWave()
    {
        // check to see if we've reached the end of the level
        if(waveNumber > wavesInLevel && Player.S.gameObject.activeInHierarchy)
        {
            Player.S.Celebrate();
            Destroy(gameObject);
        }

        if (waveNumber == wavesInLevel)
        {
            GameManager.S.gameplayWIntro.Stop();
            GameManager.S.gameplayNoIntro.Stop();
            GameManager.S.bossMusic.Play();
        }

        // set the current wave enemies to Active
        for (int j = 0; j < transform.childCount; j++)
        {
            if (transform.GetChild(j).tag == "Wave " + waveNumber.ToString())
            {
                transform.GetChild(j).gameObject.SetActive(true);
            }
        }

        startingNextWave = false;
    }

    public void DelayedRestart(float delay)
    {
        Invoke("Restart", delay);
    }

    public void Restart()
    {
        SceneManager.LoadScene("Main");
    }
}
