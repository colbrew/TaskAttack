using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
// using UnityEngine.Monetization;

public class GameManager : MonoBehaviour {
    public static GameManager S;
    public static DataController dataController;
    public static Image powerUpIcon;
    private static Dictionary<WeaponType, WeaponDefinition> WEAPON_DICT;

    [Header("Prototype / Testing mode")]
    [Tooltip("Prototype Mode allows start from any level.")]
    [SerializeField] private bool prototypeMode = false;
    [Tooltip("Start Level for Prototype Mode.")]
    [SerializeField] private int startFromLevel = 0;

    [Header("Set in Inspector")]
    public DifficultyLevel difficulty;
    public GameObject pauseButton;
    [Space(10)]
    public AudioMixer masterMixer;
    public AudioSource gameplayNoIntro;
    public AudioSource gameplayWIntro;
    public AudioSource bossMusic;
    [Space(10)]
    [SerializeField] private WeaponDefinition[] weaponDefinitions;
    [SerializeField] private GameObject prefabPowerUp;
    [SerializeField] private WeaponType[] powerUpFrequency = new WeaponType[] { WeaponType.spread };
    [Tooltip("Insert game level prefabs in order, with Infinite play level being last")]
    [SerializeField] private GameObject[] Levels;
    [SerializeField] private float gameRestartDelay = 3;
    [Tooltip("Insert the blur screen prefab.")]
    [SerializeField] private GameObject blurScreen;  
    [Tooltip("Insert the instructions prefab.")]
    [SerializeField] private GameObject instructions;
    [Tooltip("Insert the prefab for the dance when you beat game at Hard.")]
    [SerializeField] private GameObject hardDance;
    [Tooltip("Insert the prefab for the dance when you beat game at Master.")]
    [SerializeField] private GameObject masterDance;

    [Header("Set Dynamically")]    
    [SerializeField] private string[] texts;
    [SerializeField] private int nextLevel = 0;
    [SerializeField] private float levelPUChance = 1;
    [SerializeField] private bool bringBackCheer = false; // bool for returning cheer text after a pause but before next level starts
    [SerializeField] private TextMessageNotification textMessageNotification;
    [SerializeField] private int score = 0;
    private Text highScoreTitle;
    private Text scoreText;
    private Text highScore;

    public float GameRestartDelay { get => gameRestartDelay; private set => gameRestartDelay = value; }
    public int NextLevel { get => nextLevel; set => nextLevel = value; }
    public bool BringBackCheer { get => bringBackCheer; private set => bringBackCheer = value; }
    public bool InfinitePlay { get; set; } = false;
    public Text LevelText { get; set; }
    public Text PowerUpCounter { get; set; }

    // Use this for initialization
    void Awake()
    {   
        // singleton
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
        LevelText = GameObject.Find("CenterBillboard").GetComponent<Text>();
        scoreText = GameObject.Find("Score").GetComponent<Text>();
        scoreText.fontSize = 20; // reset to 20 in case player scored over 1 million and shrunk text
        highScore = GameObject.Find("HighScore").GetComponent<Text>();
        highScoreTitle = GameObject.Find("HighScoreTitle").GetComponent<Text>();
        PowerUpCounter = GameObject.Find("PowerUpCounter").GetComponent<Text>();
        PowerUpCounter.text = "";
        powerUpIcon = GameObject.Find("PowerUpIcon").GetComponent<Image>();
        powerUpIcon.enabled = !powerUpIcon.enabled;

        // a generic Dictionary with WeaponType as key
        WEAPON_DICT = new Dictionary<WeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            WEAPON_DICT[def.type] = def;
        }

        // set score to 0
        AddScore(0);

        // set start level
        dataController = GetComponent<DataController>();

        // using PlayerPrefs for setup to avoid race condition where we miss nextLevel setup (moved that to Start())
        if (PlayerPrefs.GetInt("startLevel") == 6)
        {
            InfinitePlay = true;
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
        NextLevel = dataController.StartLevel();
        highScore.text = dataController.GetHighestPlayerScore().ToString();

        // Start Game - decide how to start (if prototypeMode and level 0, do not start game at all)
        if (prototypeMode && startFromLevel > 0)
        {
            PlayerPrefs.SetInt("startLevel", startFromLevel);
            NextLevel = dataController.StartLevel();
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
        TaskAttackAnalytics.StartedLevel(NextLevel, difficulty.difficulty);
        BringBackCheer = true;
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

        if (NextLevel == Levels.Length - 1 && !InfinitePlay) // check if at end of regular levels
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
        BringBackCheer = false;
        LevelText.text = "";
        GameObject go = Instantiate(Levels[NextLevel]);
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
        if (WEAPON_DICT.ContainsKey(wt))
            return (WEAPON_DICT[wt]);

        // if key not found, return empty
        return (new WeaponDefinition());
    }

    public void DelayedRestart(float delay)
    {
        if (!InfinitePlay)
        {
            dataController.SetHighestLevel(NextLevel);

            if (NextLevel == (Levels.Length - 1)) // if player has reached the end of the levels for a certain difficulty, we show a variation of "You Win" message
            {
                if (difficulty.difficulty == 3)
                {
                    LevelText.text = "You Win!\n\n\nYou are\n\nthe\n\nTask MASTER!";
                    Destroy(Player.S.gameObject);
                    Instantiate(masterDance);
                    PlayerPrefs.SetInt("ResetActive", 1); // turn on main menu reset button now that player has beat the entire game
                    PlayerPrefs.SetInt("BeatGame", 1); // used to trigger AskForReview() one last time.
                }
                else if (difficulty.difficulty == 2)
                {
                    // We instantiate the alien dance partners in Celebration script to get timing right
                    if (PlayerPrefs.GetInt("TaskMasterUnlocked") != 1)
                    {
                        LevelText.text = "You Win!\n\n\nTask Master unlocked";
                        Destroy(Player.S.gameObject);
                        Instantiate(hardDance);
                        PlayerPrefs.SetInt("TaskMasterUnlocked", 1);
                        PlayerPrefs.SetInt("FirstTimeMaster", 1);
                    }
                    else
                    {
                        LevelText.text = "You Win!";
                    }
                }
                else if (difficulty.difficulty == 1)
                {
                    if (PlayerPrefs.GetInt("HardUnlocked") != 1)
                    {
                        LevelText.text = "You Win!\n\n\nHard unlocked\n\n+\n\nInfinite play unlocked";
                        PlayerPrefs.SetInt("HardUnlocked", 1);
                        PlayerPrefs.SetInt("FirstTimeInfinite", 1);
                    }
                    else
                    {
                        LevelText.text = "You Win!";
                    }
                }
                else if (difficulty.difficulty == 0)
                {
                    if (PlayerPrefs.GetInt("BeatEasy") != 2)
                    {
                        LevelText.text = "You Win!\n\n\nAre you ready for Normal?";
                        PlayerPrefs.SetInt("BeatEasy", 1);
                    }
                    else
                    {
                        LevelText.text = "You Win!";
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
                LevelText.text = "Game Over\n\nBack to Reality";
            }
        }
        else
        {
            if (score > PlayerPrefs.GetInt("highestInfiniteScore"))
            {
                LevelText.text = "New High Score!\n\n" + score.ToString();
            }
            else
            {
                LevelText.text = "Even infinity has a deadline.";
            }
        }

        dataController.SubmitNewPlayerScore(score);

        if (difficulty.difficulty == 4 && !InfinitePlay) // if final level of the entire game, 67 second pause before return to main menu
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

    public int GetGameLevelsLength()
    {
        return Levels.Length;
    }
}


