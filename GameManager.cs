using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
// using UnityEngine.Monetization;

public class GameManager : MonoBehaviour {
    static public GameManager S;
    static Dictionary<WeaponType, WeaponDefinition> WEAP_DICT;

    [Header("Set in Inspector")]
    public bool prototypeMode = false;
    public int startFromLevel = 0;// if in Prototype Mode, and this is set higher than 1, the level will start
    public WeaponDefinition[] weaponDefinitions;
    public GameObject prefabPowerUp;
    public WeaponType[] powerUpFrequency = new WeaponType[] { WeaponType.spread };
    public GameObject[] enemies;
    public GameObject[] Levels;
    public float gameRestartDelay = 3;
    public GameObject blurScreen;
    public DifficultyLevel difficulty;
    public AudioSource gameplayNoIntro;
    public AudioSource gameplayWIntro;
    public AudioSource bossMusic;
    public GameObject instructions;
    public GameObject pauseButton;
    public GameObject hardDance;
    public GameObject finalDance;
    public AudioMixer masterMixer;
    

    [Header("Set Dynamically")]
    public string[] texts;
    public int nextLevel = 0;
    public static Text levelText;
    public static Text scoreText;
    public static Text highScore;
    Text highScoreTitle;
    public static Text powerUpCounter;
    public static Image powerUpIcon;
    float levelPUChance = 1;
    public bool bringBackCheer = false; // bool for returning cheer text after a pause but before next level starts

    public int score = 0;
    public bool infinitePlay = false;

    public TextMessageNotification textMessageNotification;
    public DataController dataController;

    public static Transform AUDIO_ANCHOR;


    // Use this for initialization
    void Awake()
    {
        if (S == null)
        {
            S = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // set up blur screen
        Instantiate(blurScreen);
        blurScreen.SetActive(true);

        // set up fresh audiosourcecontroller pool list
        AudioPoolManager.Instance._pool = new List<AudioSourceController>();

        // Instantiate multiple audiocontrollers at start of runtime
        for (int i = 0; i < 15; i++)
        {
            AudioSourceController controller = AudioPoolManager.Instance.GetController();
            controller.Play();
        }

        // initialize variables
        textMessageNotification = GetComponent<TextMessageNotification>();
        levelText = GameObject.Find("CenterBillboard").GetComponent<Text>();
        scoreText = GameObject.Find("Score").GetComponent<Text>();
        scoreText.fontSize = 20; // reset to 20 in case player scored over 1 million and shrunk text
        highScore = GameObject.Find("HighScore").GetComponent<Text>();
        highScoreTitle = GameObject.Find("HighScoreTitle").GetComponent<Text>();
        powerUpCounter = GameObject.Find("PowerUpCounter").GetComponent<Text>();
        powerUpCounter.text = "";
        powerUpIcon = GameObject.Find("PowerUpIcon").GetComponent<Image>();
        powerUpIcon.enabled = !powerUpIcon.enabled;

        // a generic Dictionary with WeaponType as key
        WEAP_DICT = new Dictionary<WeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            WEAP_DICT[def.type] = def;
        }

        // set score to 0
        AddScore(0);

        // set start level
        dataController = GetComponent<DataController>();

        // using PlayerPrefs for setup to avoid race condition where we miss nextLevel setup (moved that to Start())
        if (PlayerPrefs.GetInt("startLevel") == 6)
        {
            infinitePlay = true;
        }
        else // if not infinite play, turn off Wave Counter
        {
            GameObject.Find("WaveCounterText").SetActive(false);
        }

        // set High Score title and number
        if (PlayerPrefs.GetInt("startLevel") == 6)
        {
            highScoreTitle.text = "Infinite High Score";
        }
        else
        {
            highScoreTitle.text = "High Score";
        }

    }

    private void Start()
    {
        // set start level and high score
        nextLevel = dataController.StartLevel();
        highScore.text = dataController.GetHighestPlayerScore().ToString();

        // Start Game - decide how to start (if prototypeMode and level 0, do not start game at all)
        if (prototypeMode && startFromLevel > 0)
        {
            PlayerPrefs.SetInt("startLevel", startFromLevel);
            nextLevel = dataController.StartLevel();
            TextInterlude();
            PlayerPrefs.SetInt("startLevel", 0);
        }
        else if (!prototypeMode) // if not prototype mode, start game as usual
        {
            if (PlayerPrefs.GetInt("startLevel") == 0 && (difficulty.difficulty == 1 || difficulty.difficulty == 0)) // if loading level one on normal or easy, show instructions
            {
                Instantiate(instructions);
            }
            else
            {
                TextInterlude();
            }
        }
    }

    public void TextInterlude()
    {
        textMessageNotification.StartNotification();
        TaskAttackAnalytics.StartedLevel(nextLevel, difficulty.difficulty);
        bringBackCheer = true;
    }

    public void StartNextLevel()
    {
        // check if we are restarting the level after a player death
        if (!Player.S.gameObject.activeInHierarchy)
        {
            // destroy current active level
            Destroy(GameObject.FindWithTag("Level"));
            // reposition and reactivate the player
            Player.S.gameObject.transform.position = Player.PLAYERSTARTLOCATION;
            Player.S.gameObject.SetActive(true);
        }

        if (nextLevel == Levels.Length - 1 && !infinitePlay) // check if at end of regular levels
        {
            DelayedRestart(3);
        }
        else
        {
            Invoke("DelayedLevelStart", 3);
        }
    }

    void DelayedLevelStart()
    {
        bringBackCheer = false;
        levelText.text = "";
        GameObject go = Instantiate(Levels[nextLevel]);
        levelPUChance = go.GetComponent<LevelManager>().levelPowerupChance;
        // nextLevel++; we moved this to the celebration script
    }

    public void AddScore(int x)
    {
        // score multiplier for higher levels
        if (difficulty.difficulty > 0)
        {
            x = x * difficulty.difficulty;
        }
        score += x;
        scoreText.text = score.ToString();
    }

    // Potentially generate a powerup when enemy destroyed
    public void PowerUpDrop(EnemyBase e)
    {
        if(Random.value < e.powerUpDropChance * levelPUChance && !Player.S.powerupOn && !GameObject.FindWithTag("PowerUp"))
        {
            WeaponType puType = powerUpFrequency[0];
            // Spawn a power up
            GameObject go = Instantiate(prefabPowerUp, e.transform.position, Quaternion.identity) as GameObject;
            PowerUp pu = go.GetComponent<PowerUp>();
            pu.SetType(puType);
        }
    }

    static public WeaponDefinition GetWeaponDefinition(WeaponType wt)
    {
        if (WEAP_DICT.ContainsKey(wt))
            return (WEAP_DICT[wt]);

        // if key not found, return empty
        return (new WeaponDefinition());
    }

    public void DelayedRestart(float delay)
    {
        if (!infinitePlay)
        {
            dataController.SetHighestLevel(nextLevel);

            if (nextLevel == (Levels.Length - 1)) // if player has reached the end of the levels for a certain difficulty, we show a variation of "You Win" message
            {
                if (difficulty.difficulty == 3)
                {
                    levelText.text = "You Win!\n\n\nYou are\n\nthe\n\nTask MASTER!";
                    Destroy(Player.S.gameObject);
                    Instantiate(finalDance);
                    PlayerPrefs.SetInt("ResetActive", 1); // turn on main menu reset button now that player has beat the entire game
                    PlayerPrefs.SetInt("BeatGame", 1); // used to trigger AskForReview() one last time.
                }
                else if (difficulty.difficulty == 2)
                {
                    // We instantiate the alien dance partners in Celebration script to get timing right
                    if (PlayerPrefs.GetInt("TaskMasterUnlocked") != 1)
                    {
                        levelText.text = "You Win!\n\n\nTask Master unlocked";
                        Destroy(Player.S.gameObject);
                        Instantiate(hardDance);
                        PlayerPrefs.SetInt("TaskMasterUnlocked", 1);
                        PlayerPrefs.SetInt("FirstTimeMaster", 1);
                    }
                    else
                    {
                        levelText.text = "You Win!";
                    }
                }
                else if (difficulty.difficulty == 1)
                {
                    if (PlayerPrefs.GetInt("HardUnlocked") != 1)
                    {
                        levelText.text = "You Win!\n\n\nHard unlocked\n\n+\n\nInfinite play unlocked";
                        PlayerPrefs.SetInt("HardUnlocked", 1);
                        PlayerPrefs.SetInt("FirstTimeInfinite", 1);
                    }
                    else
                    {
                        levelText.text = "You Win!";
                    }
                }
                else if (difficulty.difficulty == 0)
                {
                    if (PlayerPrefs.GetInt("BeatEasy") != 2)
                    {
                        levelText.text = "You Win!\n\n\nAre you ready for Normal?";
                        PlayerPrefs.SetInt("BeatEasy", 1);
                    }
                    else
                    {
                        levelText.text = "You Win!";
                    }
                }

                // raise difficulty by one so it loads next difficult levels in Main Menu
                if (difficulty.difficulty < 4)
                {
                    difficulty.difficulty++;
                }
            }
            else
            {
                levelText.text = "Game Over\n\nBack to Reality";
            }
        }
        else
        {
            if (score > PlayerPrefs.GetInt("highestInfiniteScore"))
            {
                levelText.text = "New High Score!\n\n" + score.ToString();
            }
            else
            {
                levelText.text = "Even infinity has a deadline.";
            }
        }

        dataController.SubmitNewPlayerScore(score);

        if (difficulty.difficulty == 4 && !infinitePlay) // if final level of the entire game, 67 second pause before return to main menu
        {
            difficulty.difficulty = 3;
            Invoke("BackToMainMenu", 67f);
        }
        else
        {
            Invoke("BackToMainMenu", delay);
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
