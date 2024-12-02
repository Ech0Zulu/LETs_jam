using UnityEngine;

public class ClearLeaderboardOnce : MonoBehaviour
{
    void Start()
    {
        // Clear leaderboard entries (assuming stored keys)
        for (int i = 0; i < 10; i++) // Adjust range based on your leaderboard size
        {
            PlayerPrefs.DeleteKey($"LeaderboardScore{i}");
        }

        // Save changes
        PlayerPrefs.Save();
        Debug.Log("Leaderboard cleared!");

        // Destroy this script after use
        Destroy(this);
    }
}