using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.iOS;

public class MainMenu : MonoBehaviour {

    [Header("Set in Inspector")]
    [SerializeField] private GameObject[] checkMarks;
    [Tooltip("Insert the images that appear in the main menu after you beat each level.")]
    [SerializeField] private GameObject[] LevelImage;
    [SerializeField] private DifficultyLevel difficultyLevel;
    [Tooltip("Insert the difficulty level dropdown.")]
    [SerializeField] private Dropdown dropdown;
    [Tooltip("Insert the 'beat X to unlock' sign")]
    [SerializeField] private GameObject unlockBox;
    [SerializeField] private GameObject infinitePlayButton;
    [SerializeField] private GameObject leaderboardButton;
    [Tooltip("Insert the difficulty level Label - used to flash newly unlocked difficulties.")]
    [SerializeField] private GameObject difficultyButton;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private AudioSource mainMenuMusic;
    [SerializeField] private AudioSource buttonClick;

    private int numberOfStartButtons = 6;
    private float startVolume;
    private Text unlockText;
    private Color infiniteColor;
    private Color difficultyColor;
    private float flashTime = 0;
    private float flashSpeed = 3;
    private float flashDuration = 2.25f;
    private bool fadeAndLoad = false;
    private float fadeStart;
    private float fadeSpeed = .5f;
    private CanvasGroup menuCanvas;
    private bool reviewRequested = false;

    private void Awake()
    {
        // Reset(); //use this to reset game. Be sure to do this before uploading build to App Store.

        // PlayerPrefs.SetInt("highestLevelNormal", 5); // for testing
        // PlayerPrefs.SetInt("HardUnlocked", 1); // for testing
        // PlayerPrefs.SetInt("highestLevelHard", 5); // for testing
        /* PlayerPrefs.SetInt("TaskMasterUnlocked", 1); // testing
        PlayerPrefs.SetInt("highestLevelTaskMaster", 5); // for testing*/

        
        // PlayerPrefs.SetInt("FirstTimeInfinite", 1); // for testing
        // PlayerPrefs.SetInt("FirstTimeMaster", 1); // testing
        // PlayerPrefs.SetInt("BeatEasy", 1); // testing

        infiniteColor = infinitePlayButton.GetComponent<Image>().color;
        difficultyColor = difficultyButton.GetComponent<Text>().color;
        dropdown = GetComponentInChildren<Dropdown>();
        unlockText = unlockBox.GetComponentInChildren<Text>();
        menuCanvas = GetComponent<CanvasGroup>();
        startVolume = GetComponent<AudioSource>().volume; 
    }

    // Use this for initialization
    void Start () {
        dropdown.value = difficultyLevel.difficulty;

        if(PlayerPrefs.GetInt("BeatGame") == 1)
        {
            AskForReview();
            PlayerPrefs.SetInt("BeatGame", 2);
        }
        UpdateButtons();

        mainMenuMusic.Play();

        Social.localUser.Authenticate(ProcessAuthentication); // activate Leaderboard if signed into Game Center
    }

    void ProcessAuthentication(bool success) // for Game Center social login error handling
    {
        if (success)
        {
            Debug.Log("Authenticated, checking achievements");
            leaderboardButton.SetActive(true); // turn on the leaderboard button so people can see the high scores
        }
        else
        {
            Debug.Log("Failed to authenticate");
            leaderboardButton.SetActive(false);
        }
    }

    private void Update()
    {
        // infinite play  and difficulty buttons flash the first time after being unlocked
        if(PlayerPrefs.GetInt("FirstTimeMaster") == 1 || PlayerPrefs.GetInt("FirstTimeInfinite") == 1 || PlayerPrefs.GetInt("BeatEasy") == 1)
        {
            flashTime += Time.deltaTime;
            float u = Mathf.Abs(Mathf.Sin(flashTime * flashSpeed));
            infiniteColor.a = difficultyColor.a = Mathf.Lerp(0, 1, u);
            difficultyButton.GetComponent<Text>().color = difficultyColor;

            if (PlayerPrefs.GetInt("FirstTimeInfinite") == 1)
            {
                 
                infinitePlayButton.GetComponent<Image>().color = infiniteColor;
            }

            // if flash time is up and alpha is near 1, back to normal
            if (flashTime > flashDuration && infiniteColor.a > .95f)
            {
                infiniteColor.a = difficultyColor.a = 1;
                infinitePlayButton.GetComponent<Image>().color = infiniteColor;
                difficultyButton.GetComponent<Text>().color = difficultyColor;

                if(PlayerPrefs.GetInt("TaskMasterUnlocked") == 1)
                {
                    PlayerPrefs.SetInt("FirstTimeMaster", 2);
                }
                if (PlayerPrefs.GetInt("BeatEasy") == 1)
                {
                    PlayerPrefs.SetInt("BeatEasy", 2);
                }
                PlayerPrefs.SetInt("FirstTimeInfinite", 2);
                if (!reviewRequested)
                {
                    reviewRequested = true;
                    AskForReview();
                }
            }
        }

        if(fadeAndLoad)
        {
            float u = (Time.time - fadeStart) / fadeSpeed;
            menuCanvas.alpha = Mathf.Lerp(1, 0, u);
            mainMenuMusic.volume = Mathf.Lerp(startVolume, 0, u);
            if(u > .99f)
            {
                Invoke("DelayedLoad", .8f);
            }
        }
    }

    public void StartGame(int startLevel)
    {
        buttonClick.Play();
        PlayerPrefs.SetInt("startLevel", startLevel);
        fadeStart = Time.time;
        fadeAndLoad = true;
    }

    public void DelayedLoad()
    {
        SceneManager.LoadScene("Game"); 
    }

    public void Reset()
    {
        buttonClick.Play();
        PlayerPrefs.SetInt("highestLevelEasy", 0);
        PlayerPrefs.SetInt("highestLevelNormal", 0);
        PlayerPrefs.SetInt("highestLevelHard", 0);
        PlayerPrefs.SetInt("highestLevelTaskMaster", 0);
        PlayerPrefs.SetInt("HardUnlocked", 0);
        PlayerPrefs.SetInt("TaskMasterUnlocked", 0);
        PlayerPrefs.SetInt("highestScore", 0);
        PlayerPrefs.SetInt("highestInfiniteScore", 0);
        PlayerPrefs.SetInt("startLevel", 0);
        PlayerPrefs.SetInt("FirstTimeInfinite", 0);
        PlayerPrefs.SetInt("FirstTimeMaster", 0);
        PlayerPrefs.SetInt("ResetActive", 0);
        PlayerPrefs.SetInt("BeatEasy", 0);
        PlayerPrefs.SetInt("BeatGame", 0);
        difficultyLevel.difficulty = 1;
        Invoke("DelayedReset", .239f);
    }

    public void DelayedReset()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void SetDifficulty()
    {
        difficultyLevel.difficulty = dropdown.value;
    }

    public void UpdateButtons()
    {
        if(PlayerPrefs.GetInt("ResetActive") == 1)
        {
            resetButton.SetActive(true);
        }

        // determine which buttons and checks to show based on player progression
        for (int i = 0; i < numberOfStartButtons; i++)
        {          
            if (difficultyLevel.difficulty == 0)
            {
                unlockBox.SetActive(false);

                if (i <= PlayerPrefs.GetInt("highestLevelNormal") || i <= PlayerPrefs.GetInt("highestLevelEasy"))
                {
                    ActivateButton(i);
                }
                else
                {
                    DeactivatButton(i);
                }

                // Set Checks
                if (i < PlayerPrefs.GetInt("highestLevelNormal") || i < PlayerPrefs.GetInt("highestLevelEasy"))
                {
                    ActivateCheck(i);
                }
                else
                {
                    DeactivateCheck(i);
                }
            }

            if (difficultyLevel.difficulty == 1)
            {
                unlockBox.SetActive(false);

                // Set Start Buttons
                if (i <= PlayerPrefs.GetInt("highestLevelNormal"))
                {
                    ActivateButton(i);
                }
                else
                {
                    DeactivatButton(i);
                }

                // Set Checks and level images
                if (i < PlayerPrefs.GetInt("highestLevelNormal"))
                {
                    ActivateCheck(i);
                    ActivateLevelImage(i);
                }
                else
                {
                    DeactivateCheck(i);
                }

            }

            if (difficultyLevel.difficulty == 2)
            {
                // Set Buttons
                if (i <= PlayerPrefs.GetInt("highestLevelHard") && PlayerPrefs.GetInt("highestLevelNormal") > 5)
                {
                    ActivateButton(i);
                }
                else
                {
                    DeactivatButton(i);
                }

                // Set Checks
                if (i < PlayerPrefs.GetInt("highestLevelHard") && PlayerPrefs.GetInt("highestLevelNormal") > 5)
                {
                    ActivateCheck(i);
                }
                else
                {
                    DeactivateCheck(i);
                }

                if(PlayerPrefs.GetInt("highestLevelNormal") < 6)
                {
                    unlockBox.SetActive(true);
                    unlockText.text = "Beat 'Normal' to Unlock";
                }
                else
                {
                    unlockBox.SetActive(false);
                }
            }

            if (difficultyLevel.difficulty == 3)
            {
                // Set Buttons
                if (i <= PlayerPrefs.GetInt("highestLevelTaskMaster") && PlayerPrefs.GetInt("highestLevelHard") > 5)
                {
                    ActivateButton(i);
                }
                else
                {
                    DeactivatButton(i);
                }

                // Set Checks
                if (i < PlayerPrefs.GetInt("highestLevelTaskMaster") && PlayerPrefs.GetInt("highestLevelHard") > 5)
                {
                    ActivateCheck(i);
                }
                else
                {
                    DeactivateCheck(i);
                }

                if (PlayerPrefs.GetInt("highestLevelHard") < 6)
                {
                    unlockBox.SetActive(true);
                    unlockText.text = "Beat 'Hard' to Unlock";
                }
                else
                {
                    unlockBox.SetActive(false);
                }
            }
        }

        if (PlayerPrefs.GetInt("HardUnlocked") == 1)
        {
            infinitePlayButton.SetActive(true);
            ActivateLevelImages();
        }
        else
        {
            infinitePlayButton.SetActive(false); 
        }
    }

    public void ActivateButton(int x)
    {
        GameObject currentButton = GameObject.Find("Button" + x.ToString());
        currentButton.GetComponent<Button>().interactable = true;
        if (difficultyLevel.difficulty == 0 || difficultyLevel.difficulty == 1)
        {
            currentButton.GetComponent<Image>().color = new Color(.25f, .25f, .25f, 1);
        }
        else if (difficultyLevel.difficulty == 2)
        {
            currentButton.GetComponent<Image>().color = new Color(0, 1, 0, 1);
        }
        else if(difficultyLevel.difficulty == 3)
        {
            currentButton.GetComponent<Image>().color = new Color(1, 0, 0, 1);
        }
    }

    public void DeactivatButton(int x)
    {
        GameObject currentButton = GameObject.Find("Button" + x.ToString());
        currentButton.GetComponent<Button>().interactable = false;
        currentButton.GetComponent<Image>().color = new Color(1, 1, 1, 0.63f);
    }

    public void ActivateCheck(int x)
    {
        checkMarks[x].SetActive(true);
    }

    public void DeactivateCheck(int x)
    {
        checkMarks[x].SetActive(false);
    }

    public void ActivateLevelImage(int x)
    {
        LevelImage[x].SetActive(true);
    }

    public void ActivateLevelImages()
    {
        for(int i = 0; i < numberOfStartButtons; i++)
        {
            LevelImage[i].SetActive(true);
        }
    }

    public void AskForReview()
    {
        Device.RequestStoreReview();
    }
}
