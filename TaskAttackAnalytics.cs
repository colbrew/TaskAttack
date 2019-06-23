using UnityEngine;
using UnityEngine.Analytics;

public static class TaskAttackAnalytics
{
    public static void StartedLevel(int level, int difficulty)
    {
        Debug.Log("StartedLevelEvent Called Difficulty:" + difficulty + ", Level: " + (level + 1));
        int levelNumber = (level + 1) + (difficulty * 7); // turning all 4 levels of difficulty into single level progression from 1 to 28 (includes infinity levels)
        Debug.Log("Adding " + levelNumber + " to LevelStart");
        AnalyticsResult result = AnalyticsEvent.LevelStart(levelNumber); // Add 1 because levels are in array starting at 0
        Debug.Log(result);
    }

    public static void LevelComplete(int level, int difficulty)
    {
        Debug.Log("LevelCompleteEvent Called Difficulty:" + difficulty + ", Level: " + (level + 1));
        int levelNumber = (level + 1) + (difficulty * 7); // turning all 4 levels of difficulty into single level progression from 1 to 28 (includes infinity levels)
        Debug.Log("Adding " + levelNumber + " to LevelComplete");
        AnalyticsEvent.LevelComplete(levelNumber); // Add 1 because levels are in array starting at 0
    }
}
