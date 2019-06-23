using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Celebration : MonoBehaviour {

    Vector3 startPosition;
    float startTime;
    float movingSpeed = 2;
    public GameObject celebrationFlyover;
    public GameObject[] fireworks;
    int celebrationMoveNumber = 0;
    GameObject activeCelebrationMoves;
    GameObject activeFireworks;
    public bool celebrationStarted = false;
    bool danceStarted = true;
    bool bounceStarted = true;
    bool bounceEnded = true;
    Text centerBillboard;
    float testStartTime;
    float testCelebrationTotalTime;
    public AudioSource celebrationMusic;

    // Use this for initialization
    void Start () {
        celebrationMoveNumber = GameManager.S.nextLevel;

        centerBillboard = GameObject.Find("CenterBillboard").GetComponent<Text>();
    }
    
    // Update is called once per frame
    void Update () {

        if(celebrationStarted)
        {
            // Stop boss music and playy celebration music
            GameManager.S.bossMusic.Stop();
            celebrationMusic.Play();

            // Deactivate Pause button
            GameManager.S.pauseButton.SetActive(false);

            // destroy any player projectiles remaining on the screen
            GameObject[] liveProjectiles = GameObject.FindGameObjectsWithTag("PlayerProjectile");
            foreach (GameObject projectile in liveProjectiles)
            {
                Destroy(projectile);
            }

            // destroy any player laser projectiles remaining on the screen
            liveProjectiles = GameObject.FindGameObjectsWithTag("PlayerProjectileLaser");
            foreach (GameObject projectile in liveProjectiles)
            {
                Destroy(projectile);
            }

            // destroy any enemy projectiles remaining on the screen
            liveProjectiles = GameObject.FindGameObjectsWithTag("EnemyProjectile");
            foreach (GameObject projectile in liveProjectiles)
            {
                Destroy(projectile);
            }

            // destroy any Powerups remaining on the screen
            liveProjectiles = GameObject.FindGameObjectsWithTag("PowerUp");
            foreach (GameObject projectile in liveProjectiles)
            {
                Destroy(projectile);
            }

            TaskAttackAnalytics.LevelComplete(GameManager.S.nextLevel, GameManager.S.difficulty.difficulty);

            // we've made it to the next level(this will unlock the next level in the main menu, 
            // even if player quits game during celebration or text interlude)
            GameManager.S.nextLevel++; 

            centerBillboard.text = "Victory!";
            startPosition = transform.position;
            startTime = Time.time;
            celebrationStarted = false;
            danceStarted = false;
            testStartTime = Time.time;
            Debug.Log("Start time: " + testStartTime.ToString());
        }

        if(!danceStarted) // move the player into celebration start location
        {
            float u = (Time.time - startTime) * movingSpeed;
            if(u > 1)
            {
                danceStarted = true;
                Invoke("Celebrate", 1); 
            }
            transform.position = Vector3.Lerp(startPosition, Player.PLAYERSTARTLOCATION, u);
        }

        if(activeCelebrationMoves != null) // check if celebration moves have been instantiated
        {
            // check if they are not active, if so start the bouncing
            if (!activeCelebrationMoves.activeInHierarchy && !bounceStarted) 
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
                Player.S.engines.SetActive(true);
                Destroy(activeCelebrationMoves);
                startPosition = transform.position;
                startTime = Time.time;
                bounceEnded = false;
                bounceStarted = true;

                Invoke("TextInterrupt", 3);
            }
        }

        if(bounceStarted && !bounceEnded) // happy bounce
        {
            float z = Mathf.Abs(Mathf.Sin((Time.time - startTime) * movingSpeed * 2.5f));
            Vector3 position = startPosition;
            position.y += z;
            transform.position = position;
        }
    }

    void Celebrate()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        Player.S.engines.SetActive(false);
        activeCelebrationMoves = Instantiate(celebrationFlyover);
        activeFireworks = Instantiate(fireworks[celebrationMoveNumber]);
        celebrationMoveNumber++;
        bounceStarted = false;
    }

    void TextInterrupt()
    {
        testCelebrationTotalTime = Time.time - testStartTime;
        Debug.Log("Celebration duration: " + testCelebrationTotalTime.ToString());

        // see if we have completed all 6 levels. 
        // If so, StartNextLevel will catch that and end the game.
        if (GameManager.S.nextLevel == GameManager.S.Levels.Length - 1)
        {
            GameManager.S.StartNextLevel();
        }
        else // if we have not completed all levels, start the next textinterlude
        {
            bounceEnded = true;
            Destroy(activeFireworks);
            centerBillboard.text = "";
            Player.S.celebration = false;
            celebrationMusic.Stop();
            GameManager.S.TextInterlude();
        }
    }
}
