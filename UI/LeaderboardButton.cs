using UnityEngine;

public class LeaderboardButton : MonoBehaviour
{
    public void ShowLeaderBoard() 
    {
        Social.ShowLeaderboardUI();
        Debug.Log("Showing Leaderboard");
    }
}