using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextMessageNotification : MonoBehaviour {
    
    float startTime;
    int notificationNumber = 0;
    bool notificationStarted = false;
    public GameObject textMessageText;
    public GameObject textMessageBox;
    Text centerBillboard;
    string[] textNames = new string[] { "", "Cindi Smith", "Troy's Car Repair", "Bob Robertson", "Babysitter", "HEADLINE NEWS", "Overseer"};
    string[] textMessages = new string[] { "", "Are you wasting time playing video games? I need you to pick up carrots for dinner!", "You're gonna need a new muffler. Call for a quote.", "Don't forget to finish the report for the board meeting today.", "Where are you? Your kids are ready to be picked up.", "You have 78 breaking news alerts.", "Welcome to infinite play. Good luck."};
    string[] cheers = new string[] { "", "Carrots can wait! You've got a universe to save.", "My other car is a STARFIGHTER!", "Thanks, but I've got a 2 o'clock with DESTINY.", "Busy kicking alien ass, kiddos!", "I'll be making the headlines tonight!", ""};
    public AudioData chordNotificationSound;
    bool unpausedAfterText = false;

    // Use this for initialization
    void Awake () {
        centerBillboard = GameObject.Find("CenterBillboard").GetComponent<Text>();

        // initialize the infinite play cheer with current high score
        cheers[6] = "Score to Beat      " + PlayerPrefs.GetInt("highestInfiniteScore");
    }

    // Update is called once per frame
    void Update () {
        if (textMessageText.activeInHierarchy)
        {
            if (!textMessageBox.activeInHierarchy)
            {
                textMessageText.SetActive(false);
                centerBillboard.text = null;

            }
            else if(Time.time - startTime > 3)
            {
                if (!unpausedAfterText)
                {
                    BlurScreen.S.UnBlur();
                    centerBillboard.text = "Blast that Task!";
                    Player.S.isTextInterlude = false;
                    Player.Paused = false;
                    unpausedAfterText = true;
                    GameManager.S.pauseButton.SetActive(true);
                }
            }
        }
        else // text message has been destroyed
        {
            if(notificationStarted)
            {
                if(GameManager.S.nextLevel == 0)
                {
                    GameManager.S.gameplayNoIntro.Play();
                }
                else if (GameManager.S.nextLevel != 6)
                {
                    GameManager.S.gameplayWIntro.Play();

                    GameManager.S.gameplayNoIntro.PlayDelayed(33.571f);
                }
                centerBillboard.text = cheers[notificationNumber];
                notificationNumber++;
                GameManager.S.StartNextLevel();
                notificationStarted = false;
            }
            else if(GameManager.S.bringBackCheer && !Player.Paused)
            {
                centerBillboard.text = cheers[notificationNumber-1];
            }
        }
    }

    public void StartNotification()
    {
        notificationNumber = GameManager.S.nextLevel;
        if (notificationNumber > 0)
        {
            // destroy any player projectiles remaining on the screen
            GameObject[] liveProjectiles = GameObject.FindGameObjectsWithTag("PlayerProjectile");
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

            GameManager.S.pauseButton.SetActive(false);
            Player.S.isTextInterlude = true;
            Player.Paused = true;
            BlurScreen.S.Blur();
            Player.AutoFire = false;
            unpausedAfterText = false;
            textMessageText.SetActive(true);
            textMessageBox.SetActive(true);
            chordNotificationSound.Play(transform);
            GameObject.Find("Message").GetComponent<Text>().text = textMessages[notificationNumber];
            GameObject.Find("Name").GetComponent<Text>().text = textNames[notificationNumber];
            startTime = Time.time;
        }
        notificationStarted = true;
    }
}

