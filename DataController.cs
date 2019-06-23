using UnityEngine;

public class DataController : MonoBehaviour {

    PlayerProgress playerProgress;
    public DifficultyLevel difficultyLevel;

    public DataController()
    {
        playerProgress = new PlayerProgress();
    }

    // Use this for initialization
    void Awake() {
        LoadPlayerProgress();
    }
    
    public void SubmitNewPlayerScore (int newScore)
    {
        if (newScore > playerProgress.highestScore)
        {
            playerProgress.highestScore = newScore;
            SavePlayerProgress();
        }
    }

    public int GetHighestPlayerScore ()
    {
        return playerProgress.highestScore;
    }

    void LoadPlayerProgress ()
    {
        if (PlayerPrefs.GetInt("startLevel") == 6)
        {
            if (PlayerPrefs.HasKey("highestInfiniteScore"))
            {
                playerProgress.highestScore = PlayerPrefs.GetInt("highestInfiniteScore");
            }
        }
        else
        {
            if (PlayerPrefs.HasKey("highestScore"))
            {
                playerProgress.highestScore = PlayerPrefs.GetInt("highestScore");
            }
        }
    }

    private void SavePlayerProgress()
    {
        if (PlayerPrefs.GetInt("startLevel") == 6)
        {
            // GameCenter integration
            Social.ReportScore((long)playerProgress.highestScore, "1", success => {
                Debug.Log(success ? "Reported score successfully" : "Failed to report score");
            });

            PlayerPrefs.SetInt("highestInfiniteScore", playerProgress.highestScore);
        }
        else
        {
            // GameCenter integration
            Social.ReportScore((long)playerProgress.highestScore, "2", success => {
                Debug.Log(success ? "Reported score successfully" : "Failed to report score");
            }); 

            PlayerPrefs.SetInt("highestScore", playerProgress.highestScore);
        }
    }

    public int StartLevel()
    {
        if (PlayerPrefs.HasKey("startLevel"))
        {
            return PlayerPrefs.GetInt("startLevel");
        }
        else
        {
            return 0;
        }
    }

    public void SetHighestLevel(int endLevel) // at end of a game, set highest level based on level of difficulty you are playing
    {
        if (difficultyLevel.difficulty == 0 && endLevel > PlayerPrefs.GetInt("highestLevelEasy"))
        {
            PlayerPrefs.SetInt("highestLevelEasy", endLevel);
        }

        if (difficultyLevel.difficulty == 1 && endLevel > PlayerPrefs.GetInt("highestLevelNormal"))
        {
            PlayerPrefs.SetInt("highestLevelNormal", endLevel);
        }

        if (difficultyLevel.difficulty == 2 && endLevel > PlayerPrefs.GetInt("highestLevelHard"))
        {
            PlayerPrefs.SetInt("highestLevelHard", endLevel);
        }

        if (difficultyLevel.difficulty == 3 && endLevel > PlayerPrefs.GetInt("highestLevelTaskMaster"))
        {
            PlayerPrefs.SetInt("highestLevelTaskMaster", endLevel);
        }
    }
}
